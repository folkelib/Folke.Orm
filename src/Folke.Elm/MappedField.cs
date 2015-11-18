using System.Reflection;

namespace Folke.Elm
{
    /// <summary>A field that has been selected and mapped from a class</summary>
    public class MappedField
    {
        public BaseQueryBuilder.FieldAlias selectedField;
        public PropertyInfo propertyInfo;
        public MappedClass mappedClass;
    }
}