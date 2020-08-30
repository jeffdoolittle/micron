namespace Micron.TestClient
{
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class StringFns
    {
        public static string ToKebabCase(string str)
        {
            var pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
            return string.Join("-", pattern.Matches(str)).ToLower();
        }

        public static string ToSnakeCase(string str)
        {
            var pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
            return string.Join("_", pattern.Matches(str)).ToLower();
        }

        public static string ToCamelCase(string str)
        {
            var pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
            return new string(
              new CultureInfo("en-US", false)
                .TextInfo
                .ToTitleCase(
                  string.Join(" ", pattern.Matches(str)).ToLower()
                )
                .Replace(@" ", "")
                .Select((x, i) => i == 0 ? char.ToLower(x) : x)
                .ToArray()
            );
        }

        public static string ToPascalCase(string str)
        {
            var camelCase = ToCamelCase(str);
            if (str.Length > 0)
            {
                return char.ToUpper(camelCase[0]) + camelCase[1..];
            }
            return "";
        }

        public static string ToTitleCase(string str)
        {
            var pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
            return new CultureInfo("en-US", false)
              .TextInfo
              .ToTitleCase(
                string.Join(" ", pattern.Matches(str)).ToLower()
              );
        }
    }
}
