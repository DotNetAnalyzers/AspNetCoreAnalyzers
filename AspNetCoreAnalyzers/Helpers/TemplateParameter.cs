namespace AspNetCoreAnalyzers
{
    public struct TemplateParameter
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
            while (start < text.Length &&
                   text[start] == ' ')
            {
                start++;
            }

            var end = text.IndexOf('}', start);
            if (end < 0)
            {
                result = default(TemplateParameter);
                return false;
            }

            while (text[end] == ' ')
            {
                end--;
            }

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

                var typeEnd = text.IndexOf('}', typeStart);
                if (typeEnd < 0)
                {
                    return null;
                }

                while (text[typeEnd] == ' ')
                {
                    typeEnd--;
                }

                return textAndLocation.Substring(typeStart, typeEnd - typeStart);
            }
        }
    }
}
