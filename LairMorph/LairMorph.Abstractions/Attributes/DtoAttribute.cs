namespace LairMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DtoAttribute<T> : Attribute
{
    public T? DtoType { get; }
    
    public DtoAttribute()
    {}
    
    public DtoAttribute(T dtoType)
    {
        DtoType = dtoType;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DtoAttribute : DtoAttribute<Type>
{
    public DtoAttribute() : base()
    {}
    
    public DtoAttribute(Type dtoType) : base(dtoType)
    {
    }
}