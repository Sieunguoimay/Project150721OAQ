namespace Common.UnityExtend.Serialization
{
    public static class SerializeUtility
    {
        public static string FormatBackingFieldPropertyName(string name)
        {
            return $"<{name}>k__BackingField";
        }
    }
}