namespace LairMorph.Generator.Models;

internal sealed class MapperTemplateModel
{
    public string? Namespace { get; init; }
    public string ClassName { get; init; } = null!;
    public string EntityName { get; init; } = null!;
    public string DtoName { get; init; } = null!;
    
    public IReadOnlyList<AssignmentTemplateModel> ToDtoAssignments { get; init; } = [];
    public IReadOnlyList<AssignmentTemplateModel> ToEntityAssignments { get; init; } = [];
}