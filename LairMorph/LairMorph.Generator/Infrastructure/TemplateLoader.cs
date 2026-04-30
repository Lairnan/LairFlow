using System.Reflection;
using Scriban;

namespace LairMorph.Generator.Infrastructure;

internal static class TemplateLoader
{
    public static Template Load(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(templateName)
            ?? throw new InvalidOperationException($"Template resource not found: {templateName}");

        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        var template = Template.Parse(text);
        if (template.HasErrors)
        {
            throw new InvalidOperationException(
                $"Failed to parse Scriban template '{templateName}': {string.Join(", ", template.Messages)}");
        }

        return template;
    }
}