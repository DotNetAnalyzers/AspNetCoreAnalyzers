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
            Text.SkipWhiteSpace(text, ref start);

            var end = text.IndexOf('}', start);
            if (end < 0)
            {
                result = default(TemplateParameter);
                return false;
            }

            Text.BackWhiteSpace(text, ref end);

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
                Text.SkipWhiteSpace(text, ref typeStart);
                var typeEnd = text.IndexOf('}', typeStart);
                if (typeEnd < 0)
                {
                    return null;
                }

                Text.BackWhiteSpace(text, ref typeEnd);
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
    }
}
