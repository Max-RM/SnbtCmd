﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using fNbt;
using TryashtarUtils.Utility;

namespace TryashtarUtils.Nbt
{
    public class SnbtOptions
    {
        public bool Minified;
        public bool IsJsonLike;
        public Predicate<string> ShouldQuoteKeys;
        public Predicate<string> ShouldQuoteStrings;
        public QuoteMode KeyQuoteMode;
        public QuoteMode StringQuoteMode;
        public bool NumberSuffixes;
        public bool ArrayPrefixes;
        public INewlineHandler NewlineHandling;

        public enum QuoteMode
        {
            Automatic,
            DoubleQuotes,
            SingleQuotes
        }

        public interface INewlineHandler
        {
            string Handle();
        }

        public class IgnoreHandler : INewlineHandler
        {
            public static readonly IgnoreHandler Instance = new();
            public string Handle() => Environment.NewLine;
        }

        public class EscapeHandler : INewlineHandler
        {
            public static readonly EscapeHandler Instance = new();
            public string Handle() => Snbt.STRING_ESCAPE + "n";
        }

        public class ReplaceHandler : INewlineHandler
        {
            public readonly string Replacement;
            public ReplaceHandler(string replacement)
            {
                Replacement = replacement;
            }
            public string Handle() => Replacement;
        }

        public SnbtOptions Expanded()
        {
            this.Minified = false;
            return this;
        }

        public SnbtOptions WithHandler(INewlineHandler handler)
        {
            this.NewlineHandling = handler;
            return this;
        }

        private static readonly Regex StringRegex = new("^[A-Za-z0-9._+-]+$", RegexOptions.Compiled);

        public static SnbtOptions Default => new()
        {
            Minified = true,
            ShouldQuoteKeys = x => !StringRegex.IsMatch(x),
            ShouldQuoteStrings = x => true,
            KeyQuoteMode = QuoteMode.Automatic,
            StringQuoteMode = QuoteMode.Automatic,
            NumberSuffixes = true,
            ArrayPrefixes = true,
            NewlineHandling = EscapeHandler.Instance
        };
        public static SnbtOptions DefaultExpanded => Default.Expanded();

        public static SnbtOptions JsonLike => new()
        {
            Minified = true,
            IsJsonLike = true,
            ShouldQuoteKeys = x => true,
            ShouldQuoteStrings = x => x != "null",
            KeyQuoteMode = QuoteMode.DoubleQuotes,
            StringQuoteMode = QuoteMode.DoubleQuotes,
            NumberSuffixes = false,
            ArrayPrefixes = false,
            NewlineHandling = EscapeHandler.Instance
        };
        public static SnbtOptions JsonLikeExpanded => JsonLike.Expanded();

        public static SnbtOptions Preview => new()
        {
            Minified = true,
            ShouldQuoteKeys = x => false,
            ShouldQuoteStrings = x => false,
            NumberSuffixes = false,
            ArrayPrefixes = false,
            NewlineHandling = new ReplaceHandler("⏎")
        };

        public static SnbtOptions MultilinePreview => Preview.WithHandler(IgnoreHandler.Instance);
    }
    public static class Snbt
    {
        public const char BYTE_SUFFIX = 'b';
        public const char SHORT_SUFFIX = 's';
        public const char LONG_SUFFIX = 'L';
        public const char FLOAT_SUFFIX = 'f';
        public const char DOUBLE_SUFFIX = 'd';
        public const char BYTE_ARRAY_PREFIX = 'B';
        public const char INT_ARRAY_PREFIX = 'I';
        public const char LONG_ARRAY_PREFIX = 'L';
        public const char NAME_VALUE_SEPARATOR = ':';
        public const char VALUE_SEPARATOR = ',';
        public const char ARRAY_DELIMITER = ';';
        public const char LIST_OPEN = '[';
        public const char LIST_CLOSE = ']';
        public const char COMPOUND_OPEN = '{';
        public const char COMPOUND_CLOSE = '}';
        public const char STRING_ESCAPE = '\\';
        public const char STRING_PRIMARY_QUOTE = '"';
        public const char STRING_SECONDARY_QUOTE = '\'';
        public const string VALUE_SPACING = " ";
        public const string INDENTATION = "    ";

        // convert a tag to its string form
        // expanded: for compounds and lists of non-numeric type, creates pretty indented structure. for all tags, causes spaces between values
        // delimit: for numeric types, determines whether the suffix should be included. for strings, determines whether the value should be quoted
        // include_name: whether to put the tag's name (if it has one) in front of its value
        public static string ToSnbt(this NbtTag tag, SnbtOptions options, bool include_name = false)
        {
            string name = include_name ? GetNameBeforeValue(tag, options) : String.Empty;
            if (tag is NbtByte b)
                return name + b.ToSnbt(options);
            if (tag is NbtShort s)
                return name + s.ToSnbt(options);
            if (tag is NbtInt i)
                return name + i.ToSnbt(options);
            if (tag is NbtLong l)
                return name + l.ToSnbt(options);
            if (tag is NbtFloat f)
                return name + f.ToSnbt(options);
            if (tag is NbtDouble d)
                return name + d.ToSnbt(options);
            if (tag is NbtString str)
                return name + str.ToSnbt(options);
            if (tag is NbtByteArray ba)
                return name + ba.ToSnbt(options);
            if (tag is NbtIntArray ia)
                return name + ia.ToSnbt(options);
            if (tag is NbtLongArray la)
                return name + la.ToSnbt(options);
            if (tag is NbtList list)
                return name + list.ToSnbt(options);
            if (tag is NbtCompound compound)
                return name + compound.ToSnbt(options);
            throw new ArgumentException($"Can't convert tag of type {tag.TagType} to SNBT");
        }

        private static string OptionalSuffix(SnbtOptions options, char suffix)
        {
            return options.NumberSuffixes ? suffix.ToString() : String.Empty;
        }

        public static string ToSnbt(this NbtByte tag, SnbtOptions options) => (sbyte)tag.Value + OptionalSuffix(options, BYTE_SUFFIX);
        public static string ToSnbt(this NbtShort tag, SnbtOptions options) => tag.Value + OptionalSuffix(options, SHORT_SUFFIX);
        public static string ToSnbt(this NbtInt tag, SnbtOptions options) => tag.Value.ToString();
        public static string ToSnbt(this NbtLong tag, SnbtOptions options) => tag.Value + OptionalSuffix(options, LONG_SUFFIX);
        public static string ToSnbt(this NbtFloat tag, SnbtOptions options)
        {
            string result;
            if (float.IsPositiveInfinity(tag.Value))
                result = "Infinity";
            else if (float.IsNegativeInfinity(tag.Value))
                result = "-Infinity";
            else if (float.IsNaN(tag.Value))
                result = "NaN";
            else
                result = DataUtils.FloatToString(tag.Value);
            return result + OptionalSuffix(options, FLOAT_SUFFIX);
        }
        public static string ToSnbt(this NbtDouble tag, SnbtOptions options)
        {
            string result;
            if (double.IsPositiveInfinity(tag.Value))
                result = "Infinity";
            else if (double.IsNegativeInfinity(tag.Value))
                result = "-Infinity";
            else if (double.IsNaN(tag.Value))
                result = "NaN";
            else
                result = DataUtils.DoubleToString(tag.Value);
            return result + OptionalSuffix(options, DOUBLE_SUFFIX);
        }
        public static string ToSnbt(this NbtString tag, SnbtOptions options)
        {
            return QuoteIfRequested(tag.Value, options.ShouldQuoteStrings, options.StringQuoteMode, options.NewlineHandling);
        }

        public static string ToSnbt(this NbtByteArray tag, SnbtOptions options)
        {
            return ListToString("" + BYTE_ARRAY_PREFIX + ARRAY_DELIMITER, x => ((sbyte)x).ToString() + (options.NumberSuffixes ? BYTE_SUFFIX.ToString() : String.Empty), tag.Value, options);
        }

        public static string ToSnbt(this NbtIntArray tag, SnbtOptions options)
        {
            return ListToString("" + INT_ARRAY_PREFIX + ARRAY_DELIMITER, x => x.ToString(), tag.Value, options);
        }

        public static string ToSnbt(this NbtLongArray tag, SnbtOptions options)
        {
            return ListToString("" + LONG_ARRAY_PREFIX + ARRAY_DELIMITER, x => x.ToString() + (options.NumberSuffixes ? LONG_SUFFIX.ToString() : String.Empty), tag.Value, options);
        }

        public static string ToSnbt(this NbtList tag, SnbtOptions options)
        {
            if (options.Minified)
                return ListToString("", x => x.ToSnbt(options, include_name: false), tag.Tags, options);
            else
            {
                var sb = new StringBuilder();
                AddSnbtList(tag, options, sb, INDENTATION, 0, false);
                return sb.ToString();
            }
        }

        public static string ToSnbt(this NbtCompound tag, SnbtOptions options)
        {
            var sb = new StringBuilder();
            if (options.Minified)
            {
                sb.Append(COMPOUND_OPEN);
                sb.Append(String.Join(VALUE_SEPARATOR.ToString(), tag.Tags.Select(x => x.ToSnbt(options, include_name: true)).ToArray()));
                sb.Append(COMPOUND_CLOSE);
            }
            else
                AddSnbtCompound(tag, options, sb, INDENTATION, 0, false);
            return sb.ToString();
        }

        // shared technique for single-line arrays
        // (list, int array, byte array)
        private static string ListToString<T>(string list_prefix, Func<T, string> function, IEnumerable<T> values, SnbtOptions options)
        {
            if (!options.ArrayPrefixes)
                list_prefix = String.Empty;
            // spacing between values
            string spacing = options.Minified ? String.Empty : VALUE_SPACING;
            // spacing between list prefix and first value
            string prefix_separator = !options.Minified && list_prefix.Length > 0 && values.Any() ? VALUE_SPACING : String.Empty;
            var s = new StringBuilder(LIST_OPEN + list_prefix + prefix_separator);
            string contents = String.Join(VALUE_SEPARATOR + spacing, values.Select(x => function(x)));
            s.Append(contents);
            s.Append(LIST_CLOSE);
            return s.ToString();
        }

        private static string QuoteIfRequested(string str, Predicate<string> should_quote, SnbtOptions.QuoteMode mode, SnbtOptions.INewlineHandler newlines)
        {
            if (should_quote(str))
                return QuoteAndEscape(str, mode, newlines);
            else
                return str?.Replace("\n", newlines.Handle());
        }

        public static string GetName(NbtTag tag, SnbtOptions options)
        {
            return QuoteIfRequested(tag.Name, options.ShouldQuoteKeys, options.KeyQuoteMode, options.NewlineHandling);
        }

        private static string GetNameBeforeValue(NbtTag tag, SnbtOptions options)
        {
            if (tag.Name == null)
                return String.Empty;
            return GetName(tag, options) + NAME_VALUE_SEPARATOR + (options.Minified ? String.Empty : VALUE_SPACING);
        }

        // adapted directly from minecraft's (decompiled) source
        private static string QuoteAndEscape(string input, SnbtOptions.QuoteMode mode, SnbtOptions.INewlineHandler newlines)
        {
            const char PLACEHOLDER_QUOTE = '\0';
            var builder = new StringBuilder(PLACEHOLDER_QUOTE.ToString()); // dummy value to be replaced at end
            char preferred_quote;
            if (mode == SnbtOptions.QuoteMode.DoubleQuotes)
                preferred_quote = '"';
            else if (mode == SnbtOptions.QuoteMode.SingleQuotes)
                preferred_quote = '\'';
            else
                preferred_quote = PLACEHOLDER_QUOTE; // dummy value when we're not sure which quote type to use yet
            foreach (char c in input)
            {
                if (c == STRING_ESCAPE)
                    builder.Append(STRING_ESCAPE);
                else if (c == STRING_PRIMARY_QUOTE || c == STRING_SECONDARY_QUOTE)
                {
                    // if we find one of the quotes in the actual string text, use the other one for quoting
                    if (preferred_quote == PLACEHOLDER_QUOTE)
                        preferred_quote = (c == STRING_PRIMARY_QUOTE ? STRING_SECONDARY_QUOTE : STRING_PRIMARY_QUOTE);
                    if (c == preferred_quote)
                        builder.Append(STRING_ESCAPE);
                }
                if (c == '\n')
                    builder.Append(newlines.Handle());
                else
                    builder.Append(c);
            }
            if (preferred_quote == PLACEHOLDER_QUOTE)
                preferred_quote = STRING_PRIMARY_QUOTE;
            builder[0] = preferred_quote;
            builder.Append(preferred_quote);
            return builder.ToString();
        }

        private static void AddIndents(StringBuilder sb, string indent_string, int indent_level)
        {
            for (int i = 0; i < indent_level; i++)
            {
                sb.Append(indent_string);
            }
        }

        // add contents of tag to stringbuilder
        // used for aligning indents for multiline compounds and lists
        private static void AddSnbt(NbtTag tag, SnbtOptions options, StringBuilder sb, string indent_string, int indent_level, bool include_name)
        {
            if (tag is NbtCompound compound)
                AddSnbtCompound(compound, options, sb, indent_string, indent_level, include_name);
            else if (tag is NbtList list)
                AddSnbtList(list, options, sb, indent_string, indent_level, include_name);
            else
            {
                AddIndents(sb, indent_string, indent_level);
                sb.Append(tag.ToSnbt(options, include_name: include_name));
            }
        }

        private static void AddSnbtCompound(NbtCompound tag, SnbtOptions options, StringBuilder sb, string indent_string, int indent_level, bool include_name)
        {
            AddIndents(sb, indent_string, indent_level);
            if (include_name)
                sb.Append(GetNameBeforeValue(tag, options));
            sb.Append(COMPOUND_OPEN);
            if (tag.Count > 0)
            {
                sb.Append(Environment.NewLine);
                var children = tag.Tags.ToArray();
                for (int i = 0; i < children.Length; i++)
                {
                    AddSnbt(children[i], options, sb, indent_string, indent_level + 1, true);
                    if (i < children.Length - 1)
                        sb.Append(VALUE_SEPARATOR);
                    sb.Append(Environment.NewLine);
                }
                AddIndents(sb, indent_string, indent_level);
            }
            sb.Append('}');
        }

        private static void AddSnbtList(NbtList tag, SnbtOptions options, StringBuilder sb, string indent_string, int indent_level, bool include_name)
        {
            AddIndents(sb, indent_string, indent_level);
            if (include_name)
                sb.Append(GetNameBeforeValue(tag, options));
            bool compressed = ShouldCompressListOf(tag.ListType);
            if (compressed)
                sb.Append(ListToString("", x => x.ToSnbt(options, include_name: false), tag.Tags, options));
            else
            {
                sb.Append(LIST_OPEN);
                if (tag.Count > 0)
                {
                    sb.Append(Environment.NewLine);
                    for (int i = 0; i < tag.Count; i++)
                    {
                        AddSnbt(((NbtContainerTag)tag)[i], options, sb, indent_string, indent_level + 1, false);
                        if (i < tag.Count - 1)
                            sb.Append(VALUE_SEPARATOR);
                        sb.Append(Environment.NewLine);
                    }
                    AddIndents(sb, indent_string, indent_level);

                }
                sb.Append(LIST_CLOSE);
            }
        }

        // when a multiline list contains this type, should it keep all the values on one line anyway?
        private static bool ShouldCompressListOf(NbtTagType type)
        {
            return type switch
            {
                NbtTagType.String or
                NbtTagType.List or
                NbtTagType.Compound or
                NbtTagType.IntArray or
                NbtTagType.ByteArray or
                NbtTagType.LongArray => false,
                _ => true,
            };
        }
    }
}
