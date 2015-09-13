﻿using System;
using System.Reflection;

namespace Folke.Orm.Mapping
{
    public interface IMapper
    {
        TypeMapping GetTypeMapping(Type type);
        string GetColumnName(MemberInfo memberInfo);
        PropertyMapping GetKey(Type type);
        bool IsMapped(Type type);

        /// <summary>
        /// Add a new mapping, or replace an existing mapping
        /// </summary>
        /// <param name="mapping"></param>
        void AddMapping(TypeMapping mapping);
    }
}