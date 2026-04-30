namespace LairMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MapperAttribute<TEntity, TDto> : Attribute
{
    public TEntity Entity { get; }
    public TDto Dto { get; }

    public MapperAttribute(TEntity entity, TDto dto)
    {
        Entity = entity;
        Dto = dto;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class MapperAttribute : MapperAttribute<Type, Type>
{
    public MapperAttribute(Type entity, Type dto) : base(entity, dto)
    { }
}