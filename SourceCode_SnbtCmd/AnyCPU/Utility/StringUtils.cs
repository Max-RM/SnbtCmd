using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryashtarUtils.Utility
{
    public static class StringUtils
    {
        // helper for inlining pluralization in message boxes
        public static string Pluralize(int amount, string singular, string plural)
        {
            if (amount == 1)
                return $"1 {singular}";
            return $"{amount} {plural}";
        }
        public static string Pluralize(int amount, string singular) => Pluralize(amount, singular, singular + "s");

        // replaces the last space with a non-break space, preventing orhaning of the last word
        public static string DeOrphan(string input)
        {
            if (input == null)
                return "";
            int place = input.LastIndexOf(' ');
            if (place == -1)
                return input;
            return input.Remove(place, 1).Insert(place, "\u00A0"); // non-break space
        }

        public static string FastReplace(this string str, string find, string replace, StringComparison comparison)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (str.Length == 0)
                return str;
            if (find == null)
                throw new ArgumentNullException(nameof(find));
            if (find.Length == 0)
                throw new ArgumentException(nameof(find));

            var result = new StringBuilder(str.Length);
            bool empty_replacement = string.IsNullOrEmpty(replace);

            int found;
            int search = 0;
            while ((found = str.IndexOf(find, search, comparison)) != -1)
            {
                int chars = found - search;
                if (chars != 0)
                    result.Append(str, search, chars);
                if (!empty_replacement)
                    result.Append(replace);
                search = found + find.Length;
                if (search == str.Length)
                    return result.ToString();
            }

            result.Append(str, search, str.Length - search);
            return result.ToString();
        }

        public static IEnumerable<string> SplitLines(string text)
        {
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
