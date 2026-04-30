namespace LairMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PropertyMapAttribute : Attribute
{
    public string SourcePropertyName { get; }
    public string? Transformation { get; set; }
    
    public PropertyMapAttribute(string sourcePropertyName)
    {
        SourcePropertyName = sourcePropertyName;
    }
}