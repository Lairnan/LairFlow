namespace LairMorph.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EntityAttribute<T> : Attribute
    {
        public T? DtoType { get; }

        public EntityAttribute()
        {
        }

        public EntityAttribute(T dtoType)
        {
            DtoType = dtoType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EntityAttribute : EntityAttribute<Type>
    {
        public EntityAttribute() : base()
        {
        }

        public EntityAttribute(Type dtoType) : base(dtoType)
        {
        }
    }
}