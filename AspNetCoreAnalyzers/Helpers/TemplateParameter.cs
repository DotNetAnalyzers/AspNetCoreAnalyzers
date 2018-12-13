namespace AspNetCoreAnalyzers
{
    using System;

    public struct TemplateParameter : IEquatable<TemplateParameter>
    {
        public TemplateParameter(TextAndLocation name, bool isOptional, TextAndLocation? type)
        {
            this.Name = name;
            this.IsOptional = isOptional;
            this.Type = type;
        }

        public TextAndLocation Name { get; }

        public bool IsOptional { get; }

        public TextAndLocation? Type { get; }

        public static bool operator ==(TemplateParameter left, TemplateParameter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TemplateParameter left, TemplateParameter right)
        {
            return !left.Equals(right);
        }

        public static bool TryParse(TextAndLocation textAndLocation, out TemplateParameter result)
        {
            var text = textAndLocation.Text;
            var start = text.IndexOf('{');
            if (start < 0 ||
                text.IndexOf('}') < start ||
                text.IndexOf('{', start) > 0)
            {
                result = default(TemplateParameter);
                return false;
            }

            start++;
            SkipWhiteSpace(text, ref start);

            var end = text.IndexOf('}', start);
            if (end < 0)
            {
                result = default(TemplateParameter);
                return false;
            }

            BackWhiteSpace(text, ref end);

            for (var i = start; i < end; i++)
            {
                switch (text[i])
                {
                    case '?':
                    case ':':
                        end = i;
                        break;
                }
            }

            result = new TemplateParameter(textAndLocation.Substring(start, end - start), text.Contains("?"), Type());
            return true;

            TextAndLocation? Type()
            {
                var typeStart = text.IndexOf(':', start);
                if (typeStart < 0)
                {
                    return null;
                }

                typeStart++;
                SkipWhiteSpace(text, ref typeStart);
                var typeEnd = text.IndexOf('}', typeStart);
                if (typeEnd < 0)
                {
                    return null;
                }

                BackWhiteSpace(text, ref typeEnd);
                return textAndLocation.Substring(typeStart, typeEnd - typeStart);
            }
        }

        public bool Equals(TemplateParameter other)
        {
            return this.Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            return obj is TemplateParameter other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        private static void SkipWhiteSpace(string text, ref int pos)
        {
            while (pos < text.Length &&
                   text[pos] == ' ')
            {
                pos++;
            }
        }

        private static void BackWhiteSpace(string text, ref int pos)
        {
            while (pos >= 0 &&
                   text[pos] == ' ')
            {
                pos--;
            }
        }
    }
}
