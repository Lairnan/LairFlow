using System.Collections.Immutable;
using System.Text;
using LairMorph.Generator.Infrastructure;
using LairMorph.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LairMorph.Generator;

[Generator]
public class AutoMorphMapperGenerator : IIncrementalGenerator
{
    private readonly record struct Assignment(string TargetName, string Expression);

    private readonly record struct PairKey(INamedTypeSymbol Entity, INamedTypeSymbol Dto)
    {
        public bool Equals(PairKey other) =>
            SymbolEqualityComparer.Default.Equals(Entity, other.Entity) &&
            SymbolEqualityComparer.Default.Equals(Dto, other.Dto);

        public override int GetHashCode()
        {
            unchecked
            {
                var h1 = SymbolEqualityComparer.Default.GetHashCode(Entity);
                var h2 = SymbolEqualityComparer.Default.GetHashCode(Dto);
                return (h1 * 397) ^ h2;
            }
        }
    }

    private const string TemplateName = "LairMorph.Generator.Templates.Mapper.scriban";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource("___GEN_ALIVE.g.cs", SourceText.From("// alive", Encoding.UTF8)));

        var allClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    var sym = ctx.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node) as INamedTypeSymbol;
                    return sym;
                })
            .Where(static s => s is not null)!
            .Collect();

        var pipeline = context.CompilationProvider.Combine(allClasses);

        context.RegisterSourceOutput(pipeline, static (spc, data) =>
        {
            var (compilation, classes) = data;

            const string AttrNs = "LairMorph.Abstractions.Attributes";
            const string IfNs = "LairMorph.Abstractions.Interfaces";

            var dtoAttrG = compilation.GetTypeByMetadataName($"{AttrNs}.DtoAttribute`1");
            var dtoAttr = compilation.GetTypeByMetadataName($"{AttrNs}.DtoAttribute");
            var entityAttrG = compilation.GetTypeByMetadataName($"{AttrNs}.EntityAttribute`1");
            var entityAttr = compilation.GetTypeByMetadataName($"{AttrNs}.EntityAttribute");

            var mapperAttr = compilation.GetTypeByMetadataName($"{AttrNs}.MapperAttribute");
            var mapperAttrG = compilation.GetTypeByMetadataName($"{AttrNs}.MapperAttribute`2");

            var ignoreAttr = compilation.GetTypeByMetadataName($"{AttrNs}.IgnoreAttribute");
            var propMapAttr = compilation.GetTypeByMetadataName($"{AttrNs}.PropertyMapAttribute");
            var propMapFromManyAttr = compilation.GetTypeByMetadataName($"{AttrNs}.PropertyMapFromManyAttribute");

            var dtoMarkerIf = compilation.GetTypeByMetadataName($"{IfNs}.IAutoMorphEntityDto`1");

            var pairs = new HashSet<PairKey>();

            foreach (var c in classes)
            {
                if (c is null)
                    continue;

                if (dtoMarkerIf is not null)
                {
                    foreach (var i in c.AllInterfaces)
                    {
                        if (i is not INamedTypeSymbol ni)
                            continue;

                        if (!SymbolEqualityComparer.Default.Equals(ni.OriginalDefinition, dtoMarkerIf))
                            continue;

                        if (ni.TypeArguments.Length == 1 && ni.TypeArguments[0] is INamedTypeSymbol e)
                            pairs.Add(new PairKey(e, c));
                    }
                }

                foreach (var a in c.GetAttributes())
                {
                    var ac = a.AttributeClass;
                    if (ac is null)
                        continue;

                    if (dtoAttrG is not null &&
                        SymbolEqualityComparer.Default.Equals(ac.OriginalDefinition, dtoAttrG))
                    {
                        if (ac.TypeArguments.Length == 1 && ac.TypeArguments[0] is INamedTypeSymbol e)
                            pairs.Add(new PairKey(e, c));

                        continue;
                    }

                    if (dtoAttr is not null &&
                        SymbolEqualityComparer.Default.Equals(ac, dtoAttr))
                    {
                        if (a.ConstructorArguments.Length == 1 &&
                            a.ConstructorArguments[0].Kind == TypedConstantKind.Type &&
                            a.ConstructorArguments[0].Value is INamedTypeSymbol e)
                        {
                            pairs.Add(new PairKey(e, c));
                        }

                        continue;
                    }

                    if (entityAttrG is not null &&
                        SymbolEqualityComparer.Default.Equals(ac.OriginalDefinition, entityAttrG))
                    {
                        if (ac.TypeArguments.Length == 1 && ac.TypeArguments[0] is INamedTypeSymbol d)
                            pairs.Add(new PairKey(c, d));

                        continue;
                    }

                    if (entityAttr is not null &&
                        SymbolEqualityComparer.Default.Equals(ac, entityAttr))
                    {
                        if (a.ConstructorArguments.Length == 1 &&
                            a.ConstructorArguments[0].Kind == TypedConstantKind.Type &&
                            a.ConstructorArguments[0].Value is INamedTypeSymbol d)
                        {
                            pairs.Add(new PairKey(c, d));
                        }

                        continue;
                    }
                }
            }

            pairs.RemoveWhere(p => p.Entity.TypeKind != TypeKind.Class || p.Dto.TypeKind != TypeKind.Class);

            var covered = new HashSet<PairKey>();

            foreach (var c in classes)
            {
                if (c is null)
                    continue;

                foreach (var a in c.GetAttributes())
                {
                    var ac = a.AttributeClass;
                    if (ac is null)
                        continue;

                    if (mapperAttr is not null && SymbolEqualityComparer.Default.Equals(ac, mapperAttr))
                    {
                        if (TryReadTwoTypesFromCtor(a, out var e, out var d))
                            covered.Add(new PairKey(e, d));

                        continue;
                    }

                    if (mapperAttrG is not null &&
                        SymbolEqualityComparer.Default.Equals(ac.OriginalDefinition, mapperAttrG))
                    {
                        if (TryReadTwoTypesFromCtor(a, out var e2, out var d2))
                            covered.Add(new PairKey(e2, d2));

                        continue;
                    }
                }
            }

            TemplateLoaderResult? templateResult = null;

            foreach (var pair in pairs)
            {
                if (covered.Contains(pair))
                    continue;

                var entity = pair.Entity;
                var dto = pair.Dto;

                var ns = entity.ContainingNamespace?.ToDisplayString();
                if (string.IsNullOrWhiteSpace(ns))
                    ns = dto.ContainingNamespace?.ToDisplayString();
                if (string.IsNullOrWhiteSpace(ns))
                    ns = "Generated";

                var entityFull = entity.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var dtoFull = dto.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                var mapperName = $"{entity.Name}__{dto.Name}__AutoMorphMapper";

                var entityProps = entity.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public)
                    .ToImmutableArray();

                var dtoProps = dto.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public)
                    .ToImmutableArray();

                var entityByName = entityProps.ToDictionary(p => p.Name, StringComparer.Ordinal);
                var dtoByName = dtoProps.ToDictionary(p => p.Name, StringComparer.Ordinal);

                var toDtoAssignments = new List<Assignment>();
                foreach (var target in dtoProps.Where(p => p.SetMethod is not null))
                {
                    if (HasAttr(target, ignoreAttr))
                        continue;

                    var expr = BuildExpression(
                        sourceObject: "source",
                        targetProperty: target,
                        sourcePropsByName: entityByName,
                        propMapAttr: propMapAttr,
                        propMapFromManyAttr: propMapFromManyAttr);

                    if (expr is null)
                        continue;

                    toDtoAssignments.Add(new Assignment(target.Name, expr));
                }

                var toEntityAssignments = new List<Assignment>();
                foreach (var target in entityProps.Where(p => p.SetMethod is not null))
                {
                    if (HasAttr(target, ignoreAttr))
                        continue;

                    var expr = BuildExpression(
                        sourceObject: "source",
                        targetProperty: target,
                        sourcePropsByName: dtoByName,
                        propMapAttr: propMapAttr,
                        propMapFromManyAttr: propMapFromManyAttr);

                    if (expr is null)
                        continue;

                    toEntityAssignments.Add(new Assignment(target.Name, expr));
                }

                templateResult ??= LoadTemplateOnce(spc);

                if (templateResult.Value.Template is null)
                    continue;

                var model = new MapperTemplateModel
                {
                    Namespace = ns,
                    ClassName = mapperName,
                    EntityName = entityFull,
                    DtoName = dtoFull,
                    ToDtoAssignments = toDtoAssignments
                        .Select(a => new AssignmentTemplateModel
                        {
                            TargetName = a.TargetName,
                            Expression = a.Expression
                        })
                        .ToArray(),
                    ToEntityAssignments = toEntityAssignments
                        .Select(a => new AssignmentTemplateModel
                        {
                            TargetName = a.TargetName,
                            Expression = a.Expression
                        })
                        .ToArray()
                };

                string source;
                try
                {
                    source = TemplateRenderer.Render(templateResult.Value.Template, new
                    {
                        @namespace = model.Namespace,
                        class_name = model.ClassName,
                        entity_name = model.EntityName,
                        dto_name = model.DtoName,
                        to_dto_assignments = model.ToDtoAssignments.Select(a => new
                        {
                            target_name = a.TargetName,
                            expression = a.Expression
                        }).ToArray(),
                        to_entity_assignments = model.ToEntityAssignments.Select(a => new
                        {
                            target_name = a.TargetName,
                            expression = a.Expression
                        }).ToArray()
                    });
                }
                catch (Exception ex)
                {
                    ReportTemplateDiagnostic(
                        spc,
                        "LMORPH002",
                        "Template render failed",
                        $"Failed to render mapper template for '{entity.Name}' and '{dto.Name}': {ex.Message}");

                    continue;
                }

                spc.AddSource($"{mapperName}.g.cs", SourceText.From(source, Encoding.UTF8));
            }

            static TemplateLoaderResult LoadTemplateOnce(SourceProductionContext spc)
            {
                try
                {
                    return new TemplateLoaderResult(TemplateLoader.Load(TemplateName));
                }
                catch (Exception ex)
                {
                    ReportTemplateDiagnostic(
                        spc,
                        "LMORPH001",
                        "Template load failed",
                        $"Failed to load Scriban template '{TemplateName}': {ex.Message}");

                    return new TemplateLoaderResult(null);
                }
            }

            static bool TryReadTwoTypesFromCtor(AttributeData a, out INamedTypeSymbol entity, out INamedTypeSymbol dto)
            {
                entity = null!;
                dto = null!;

                if (a.ConstructorArguments.Length != 2)
                    return false;

                var a0 = a.ConstructorArguments[0];
                var a1 = a.ConstructorArguments[1];

                if (a0.Kind != TypedConstantKind.Type || a1.Kind != TypedConstantKind.Type)
                    return false;
                if (a0.Value is not INamedTypeSymbol e)
                    return false;
                if (a1.Value is not INamedTypeSymbol d)
                    return false;

                entity = e;
                dto = d;
                return true;
            }

            static bool HasAttr(IPropertySymbol prop, INamedTypeSymbol? attrType)
            {
                if (attrType is null)
                    return false;

                foreach (var a in prop.GetAttributes())
                {
                    if (a.AttributeClass is null)
                        continue;

                    if (SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrType) ||
                        SymbolEqualityComparer.Default.Equals(a.AttributeClass.OriginalDefinition, attrType))
                        return true;
                }

                return false;
            }

            static string? BuildExpression(
                string sourceObject,
                IPropertySymbol targetProperty,
                Dictionary<string, IPropertySymbol> sourcePropsByName,
                INamedTypeSymbol? propMapAttr,
                INamedTypeSymbol? propMapFromManyAttr)
            {
                if (propMapFromManyAttr is not null)
                {
                    var many = targetProperty.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass is not null &&
                            (SymbolEqualityComparer.Default.Equals(a.AttributeClass, propMapFromManyAttr) ||
                             SymbolEqualityComparer.Default.Equals(a.AttributeClass.OriginalDefinition, propMapFromManyAttr)));

                    if (many is not null)
                    {
                        var names = new List<string>();

                        if (many.ConstructorArguments.Length == 1)
                        {
                            var arg = many.ConstructorArguments[0];
                            if (arg.Kind == TypedConstantKind.Array)
                            {
                                foreach (var v in arg.Values)
                                {
                                    if (v.Kind == TypedConstantKind.Primitive &&
                                        v.Value is string s &&
                                        !string.IsNullOrWhiteSpace(s))
                                    {
                                        names.Add(s);
                                    }
                                }
                            }
                        }

                        if (names.Count == 0)
                            return null;

                        var srcExprs = new List<string>();
                        foreach (var n in names)
                        {
                            if (!sourcePropsByName.TryGetValue(n, out var sp) || sp.GetMethod is null)
                                return null;

                            srcExprs.Add($"{sourceObject}.{sp.Name}");
                        }

                        string? format = null;
                        foreach (var kv in many.NamedArguments)
                        {
                            if (kv.Key == "Format" && kv.Value.Value is string s)
                            {
                                format = s;
                                break;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(format))
                        {
                            var fmtLit = ToVerbatimStringLiteral(format);
                            return $"string.Format({fmtLit}, {string.Join(", ", srcExprs)})";
                        }

                        if (srcExprs.Count == 1)
                            return srcExprs[0];

                        var parts = new List<string>();
                        for (var i = 0; i < srcExprs.Count; i++)
                        {
                            if (i > 0)
                                parts.Add("\" \"");

                            parts.Add(srcExprs[i]);
                        }

                        return $"string.Concat({string.Join(", ", parts)})";
                    }
                }

                if (propMapAttr is not null)
                {
                    var one = targetProperty.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass is not null &&
                            (SymbolEqualityComparer.Default.Equals(a.AttributeClass, propMapAttr) ||
                             SymbolEqualityComparer.Default.Equals(a.AttributeClass.OriginalDefinition, propMapAttr)));

                    if (one is not null)
                    {
                        if (one.ConstructorArguments.Length != 1 ||
                            one.ConstructorArguments[0].Kind != TypedConstantKind.Primitive ||
                            one.ConstructorArguments[0].Value is not string srcName ||
                            string.IsNullOrWhiteSpace(srcName))
                        {
                            return null;
                        }

                        if (!sourcePropsByName.TryGetValue(srcName, out var srcProp) || srcProp.GetMethod is null)
                            return null;

                        var srcExpr = $"{sourceObject}.{srcProp.Name}";

                        string? transformation = null;
                        foreach (var kv in one.NamedArguments)
                        {
                            if (kv.Key == "Transformation" && kv.Value.Value is string s)
                            {
                                transformation = s;
                                break;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(transformation))
                        {
                            if (transformation.Contains("{src}", StringComparison.Ordinal))
                                return transformation.Replace("{src}", srcExpr, StringComparison.Ordinal);

                            if (transformation.Contains("{value}", StringComparison.Ordinal))
                                return transformation.Replace("{value}", srcExpr, StringComparison.Ordinal);

                            return transformation;
                        }

                        return srcExpr;
                    }
                }

                if (sourcePropsByName.TryGetValue(targetProperty.Name, out var same) && same.GetMethod is not null)
                    return $"{sourceObject}.{same.Name}";

                return null;

                static string ToVerbatimStringLiteral(string s)
                {
                    return "@\"" + s.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
                }
            }

            static void ReportTemplateDiagnostic(
                SourceProductionContext spc,
                string id,
                string title,
                string message)
            {
                var descriptor = new DiagnosticDescriptor(
                    id,
                    title,
                    message,
                    "LairMorph",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

                spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
            }
        });
    }

    private readonly record struct TemplateLoaderResult(Scriban.Template? Template);
}