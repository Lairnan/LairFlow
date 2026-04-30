namespace LairMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class PropertyMapFromManyAttribute : Attribute
{
    public string[] SourcePropertyNames { get; }
    public string? Format { get; set; }
    
    public PropertyMapFromManyAttribute(params string[] sourcePropertyNames)
    {
        SourcePropertyNames = sourcePropertyNames;
    }
}