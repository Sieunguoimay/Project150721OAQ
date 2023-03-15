using System.Collections.Generic;
using System.Linq;

namespace Common.UnityExtend.Reflection.Tools
{
    public class RuntimeObjectExpose
    {
        public IReadOnlyList<ObjectExposedItem> ExposeObject(object targetObject)
        {
            var type = targetObject.GetType();
            var allFields = ReflectionUtility.GetAllFields(type);
            var allProperties = ReflectionUtility.GetAllProperties(type);
            var exposedItems = new List<ObjectExposedItem>();
            
            foreach (var fieldInfo in allFields)
            {
                var value = fieldInfo.GetValue(targetObject);
                exposedItems.Add(new ObjectExposedItem
                {
                    FieldName = fieldInfo.Name,
                    DisplayValue = value?.ToString(),
                });
            }

            return exposedItems;
        }

        public class ObjectExposedItem
        {
            public string FieldName;
            public string DisplayValue;
        }
    }
}