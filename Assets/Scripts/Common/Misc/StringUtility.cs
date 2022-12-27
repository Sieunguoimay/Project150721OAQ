using System.Text.RegularExpressions;

namespace Common.Misc
{
    public static class StringUtility
    {
        public static string SeparateCapitalBySpace(string text)
        {
            return Regex.Replace(text, "([a-z])_?([A-Z])", "$1 $2");
        }
    }
}