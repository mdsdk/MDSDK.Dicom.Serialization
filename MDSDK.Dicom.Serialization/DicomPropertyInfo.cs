using System;
using System.Collections.Generic;
using System.Reflection;

namespace MDSDK.Dicom.Serialization
{
    internal class DicomPropertyInfo
    {
        public DicomPropertyInfo Parent { get; }

        public PropertyInfo Property { get; }

        public Type PropertyType => Property.PropertyType;

        private DicomPropertyInfo(DicomPropertyInfo parent, PropertyInfo property)
        {
            Parent = parent;
            Property = property;
        }

        public DicomAttribute DicomAttribute { get; set; }

        public override string ToString()
        {
            var path = string.Empty;
            var parent = Parent;
            while (parent != null)
            {
                path = $"{parent.Property.Name}.{path}";
                parent = parent.Parent;
            }
            return $"{path}{Property}";
        }

        private object GetPropertyValue(PropertyInfo property, object obj, bool createDefaultIfNull)
        {
            var value = Property.GetValue(obj);
            if ((value == null) && createDefaultIfNull)
            {
                value = Activator.CreateInstance(Property.PropertyType);
                Property.SetValue(obj, value);
            }
            return value;
        }

        public object GetValue(object obj, bool createDefaultIfNull = false)
        {
            if (Parent == null)
            {
                return GetPropertyValue(Property, obj, createDefaultIfNull);
            }
            var parentPropertyValue = Parent.GetValue(obj, createDefaultIfNull);
            if (parentPropertyValue != null)
            {
                return GetPropertyValue(Property, parentPropertyValue, createDefaultIfNull);
            }
            return null;
        }

        public void SetValue(object obj, object value)
        {
            if (Parent == null)
            {
                Property.SetValue(obj, value);
            }
            else
            {
                var parentPropertyValue = Parent.GetValue(obj, createDefaultIfNull: value != null);
                if (parentPropertyValue != null)
                {
                    Property.SetValue(parentPropertyValue, value);
                }
            }
        }

        public static IEnumerable<DicomPropertyInfo> GetDicomProperties(DicomPropertyInfo parent, Type type)
        {
            foreach (var property in type.GetProperties())
            {
                var dicomProperty = new DicomPropertyInfo(parent, property);
                var dicomAttributeField = typeof(DicomAttribute).GetField(property.Name, BindingFlags.Public | BindingFlags.Static);
                if (dicomAttributeField != null)
                {
                    dicomProperty.DicomAttribute = (DicomAttribute)dicomAttributeField.GetValue(null);
                    yield return dicomProperty;
                }
                else
                {
                    var count = 0;
                    foreach (var descendant in GetDicomProperties(dicomProperty, property.PropertyType))
                    {
                        count++;
                        yield return descendant;
                    }
                    if (count == 0)
                    {
                        throw new NotSupportedException($"Property {property.Name} in {type.Name} is not the keyword of a known DICOM attribute");
                    }
                }
            }
        }
    }
}
