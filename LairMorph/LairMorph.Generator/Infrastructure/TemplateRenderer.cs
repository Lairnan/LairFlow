using Scriban;
using Scriban.Runtime;

namespace LairMorph.Generator.Infrastructure;

internal static class TemplateRenderer
{
    public static string Render(Template template, object model)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(model, renamer: member => member.Name);

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        return template.Render(context);
    }
}