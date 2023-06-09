using UnityEngine;

namespace Screw
{
    public class LocaleResource : ScriptableObject
    {
        [SerializeField] private Category[] categories;
        public Category[] Categories => categories;
        public class Category
        {
            public string name;
            public Translation[] Translations;
        }
        public class Translation
        {
            public string key;
            public string ValueText;
        }
    }
}