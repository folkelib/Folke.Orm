﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Folke.Orm.Mapping
{
    public class TypeMapping<T> : TypeMapping
    {
        public TypeMapping(IMapper mapper):base(typeof(T), mapper)
        {
        }

        public void ToTable(string name, string schema = null)
        {
            TableName = name;
            TableSchema = schema;
        }

        public void HasKey(Expression<Func<T, object>> expression)
        {
            var propertyInfo = TableHelpers.GetExpressionPropertyInfo(expression);
            Key = Columns[propertyInfo.Name];
            Key.IsKey = true;
            if (propertyInfo.PropertyType == typeof (int) || propertyInfo.PropertyType == typeof(long))
                Key.IsAutomatic = true;
        }

        public FluentPropertyMapping Property(Expression<Func<T, object>> expression)
        {
            var property = TableHelpers.GetExpressionPropertyInfo(expression);
            var propertyMapping = Columns[property.Name];
             return new FluentPropertyMapping(this, propertyMapping);
        }

        public class FluentPropertyMapping
        {
            private readonly TypeMapping typeMapping;
            private readonly PropertyMapping propertyMapping;

            public FluentPropertyMapping(TypeMapping typeMapping, PropertyMapping propertyMapping)
            {
                this.typeMapping = typeMapping;
                this.propertyMapping = propertyMapping;
            }

            public FluentPropertyMapping HasColumnName(string name)
            {
                propertyMapping.ColumnName = name;
                return this;
            }
        }
    }

    public class TypeMapping
    {
        public Type Type { get; set; }

        public string TableName { get; set; }

        public string TableSchema { get; set; }

        public PropertyMapping Key { get; set; }

        public Dictionary<string, PropertyMapping> Columns { get; set; }

        public Dictionary<string, MappedCollection> Collections { get; set; }

        public TypeMapping(Type type, IMapper mapper)
        {
            Type = type;
            Columns = new Dictionary<string, PropertyMapping>();
            Collections = new Dictionary<string, MappedCollection>();
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                TableName = tableAttribute.Name;
                TableSchema = tableAttribute.Schema;
            }
            else
            {
                TableName = type.Name;
            }
            
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                bool nullable = false;
                if (Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                    nullable = true;
                }

                if (propertyType.IsGenericType)
                {
                    if (propertyType.GetInterface("IEnumerable") != null)
                    {
                        var foreignType = propertyType.GenericTypeArguments[0];
                        if (foreignType.GetInterface("IFolkeTable") != null)
                        {
                            var folkeList = typeof(FolkeList<>).MakeGenericType(foreignType);
                            if (propertyType.IsAssignableFrom(folkeList))
                            {
                                var joins =
                                    propertyInfo.GetCustomAttributes<FolkeListAttribute>().Select(x => x.Join).ToArray();
                                var constructor =
                                    folkeList.GetConstructor(new[] { typeof(IFolkeConnection), typeof(Type), typeof(int), typeof(string[]) });
                                var mappedCollection = new MappedCollection
                                {
                                    propertyInfo = propertyInfo,
                                    listJoins = joins,
                                    listConstructor = constructor
                                };
                                Collections[propertyInfo.Name] = mappedCollection;
                            }
                        }
                    }
                    continue;
                }

                var propertyMapping = new PropertyMapping(propertyInfo) {Nullable = nullable};
                Columns[propertyInfo.Name] = propertyMapping;

                if (mapper.IsMapped(propertyInfo.PropertyType))
                {
                    propertyMapping.Reference = mapper.GetTypeMapping(propertyInfo.PropertyType);
                }

                var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null)
                {
                    propertyMapping.ColumnName = columnAttribute.Name;
                    propertyMapping.MaxLength = columnAttribute.MaxLength;
                    propertyMapping.Index = columnAttribute.Index;
                    propertyMapping.OnDelete = columnAttribute.OnDelete;
                    propertyMapping.OnUpdate = columnAttribute.OnUpdate;
                }
                
                if (propertyMapping.ColumnName == null)
                {
                    if (propertyMapping.Reference != null)
                        propertyMapping.ColumnName = propertyInfo.Name + "_id";
                    else
                        propertyMapping.ColumnName = propertyInfo.Name;
                }

                if ((propertyInfo.Name == "Id" && type.GetInterface("IFolkeTable") != null) ||
                    propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                {
                    Key = propertyMapping;
                    Key.IsKey = true;
                    if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(long))
                        Key.IsAutomatic = true;
                }

                propertyMapping.Readonly = propertyMapping.IsAutomatic;

                
            }
        }
    }
}