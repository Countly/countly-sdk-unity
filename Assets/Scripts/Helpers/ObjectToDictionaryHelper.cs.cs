using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Helpers
{
    public static class ObjectToDictionaryHelper
    {
        public static IDictionary<string, object> ToDictionary<T>(this T source)
        {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, object>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (value != null)
                {
                    dictionary.Add(property.Name, value);
                }
            }
            return dictionary;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }
    }
}
