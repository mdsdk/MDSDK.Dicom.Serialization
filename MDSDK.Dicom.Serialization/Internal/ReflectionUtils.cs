// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.Reflection;

namespace MDSDK.Dicom.Serialization.Internal
{
    internal static class ReflectionUtils
    {
        private static bool IsGenericTypeAssignableFrom(Type genericTypeDefinition, Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == genericTypeDefinition))
            {
                return true;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == genericTypeDefinition))
                {
                    return true;
                }
            }

            return (type.BaseType == null) ? false : IsGenericTypeAssignableFrom(genericTypeDefinition, type.BaseType);
        }

        public static bool IsAssignableFrom(this ParameterInfo parameter, Type type)
        {
            var parameterType = parameter.ParameterType;
            return parameterType.IsGenericType
                ? IsGenericTypeAssignableFrom(parameterType.GetGenericTypeDefinition(), type)
                : parameterType.IsAssignableFrom(type);
        }
    }
}
