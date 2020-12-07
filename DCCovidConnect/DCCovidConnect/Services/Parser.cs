using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.Services
{
    class Parser
    {private StringBuilder output = new StringBuilder();
        private StringBuilder parsedTag = new StringBuilder();
        private StringBuilder parsedTagContents = new StringBuilder();
        private StringBuilder parsedText = new StringBuilder();
        private Stack<Boolean> tagStack = new Stack<Boolean>();
        enum Property
        {
            NONE, TYPE, HREF, TEXT, CHILDREN, SRC
        }
        enum Type
        {
            NONE, TITLE, ITEM, A, P, BOLD, UL, OL, LI, TEXT, COLOR, IMG, BR, H1, H2, H3, H4, H5, H6, SPAN, FIGURE, S, SUP, TABLE, TBODY, TD, TR, IFRAME
        }
        private string source = "";
        private int index = 0;
        public string Output
        {
            get => output.ToString();
        }
        public Parser(string source)
        {
            this.source = source;
        }
        private void ParseComment()
        {
            while (source[index] != '>')
            {
                index++;
            }
        }
        private void ParseTag()
        {
            parsedTag.Clear();
            parsedTagContents.Clear();
            index++;
            while (!" />".Contains(source[index]))
            {
                parsedTag.Append(source[index]);
                index++;
            }
            Boolean escaped = false;
            while (source[index] != '>' || escaped)
            {
                if (escaped) escaped = false;
                if (source[index] == '\\') escaped = true;
                parsedTagContents.Append(source[index]);
                index++;
            }
        }
        private void ParseText()
        {
            parsedText.Clear();
            Boolean escaped = false;
            while (!EOF() && (source[index] != '<' || escaped))
            {
                if (escaped) escaped = false;
                if (source[index] == '\\') escaped = true;
                if (source[index] == '"')
                    parsedText.Append('\\');
                parsedText.Append(source[index]);
                index++;
            }
        }
        private void ParseClosingTag()
        {
            while (source[index] != '>')
            {
                index++;
            }
            if (tagStack.Pop())
            {
                output.Append("]}");
            }
        }
        private void ProcessTag()
        {
            if (output[output.Length - 1] == '}')
            {
                output.Append(',');
            }
            Type tag = Type.NONE;
            Boolean selfClosing = false;
            switch (parsedTag.ToString().ToLower())
            {
                case "hr":
                case "svg":
                case "path":
                case "style":
                case "section":
                    tagStack.Push(false);
                    return;
                case "div":
                    if (parsedTagContents.ToString().ToLower().Contains("item"))
                    {
                        tag = Type.ITEM;
                        break;
                    }
                    else
                    {
                        tagStack.Push(false);
                        return;
                    }
                case "strong":
                case "em":
                    tag = Type.BOLD;
                    break;
                case "img":
                case "br":
                    selfClosing = true;
                    break;
            }
            string contents = parsedTagContents.ToString().ToLower();
            if (parsedTagContents.ToString().ToLower().Contains("title"))
                tag = Type.TITLE;
            else if (parsedTagContents.ToString().ToLower().Contains("color"))
                tag = Type.COLOR;
            if (tag == Type.NONE)
            {
                try
                {
                    tag = (Type)Enum.Parse(typeof(Type), parsedTag.ToString(), true);
                }
                catch
                {
                    Console.WriteLine("Tag: {0} is not recognized. Contents: {1}", parsedTag, parsedTagContents);
                    tagStack.Push(false);
                    return;
                }
            }
            output.Append($"{{\"{Property.TYPE}\":\"{tag}\"");

            if (contents.Contains("href", StringComparison.InvariantCultureIgnoreCase))
            {
                AddAttribute(Property.HREF, ref contents);
            }
            if (contents.Contains("src", StringComparison.InvariantCultureIgnoreCase))
            {
                AddAttribute(Property.SRC, ref contents);
            }

            if (selfClosing)
            {
                output.Append('}');
            }
            else
            {
                output.Append($",\"{Property.CHILDREN}\":[");
                tagStack.Push(true);
            }
        }

        private void AddAttribute(Property prop, ref string contents)
        {
            output.Append($",\"{prop}\":\"");
            int i = contents.IndexOf(prop.ToString(), StringComparison.InvariantCultureIgnoreCase) + prop.ToString().Length + 1;
            Boolean escaped = false;
            while (contents[++i] != '"' || escaped)
            {
                if (contents[i] == '\\') escaped = true;
                if (escaped) escaped = false;
                output.Append(contents[i]);
            }
            output.Append('"');
        }
        private void ProcessText()
        {
            if (output[output.Length - 1] == '}')
            {
                output.Append(',');
            }
            output.Append($"{{\"{Property.TYPE}\":\"{Type.TEXT}\",");
            output.Append($"\"{Property.TEXT}\":");
            output.Append('"');
            output.Append(parsedText.TrimEnd());
            output.Append("\"}");
        }
        public void Parse()
        {
            output.Append('[');
            while (!EOF())
            {
                switch (source[index])
                {
                    case '<':
                        switch (source[index + 1])
                        {
                            case '!':
                                ParseComment();
                                break;
                            case '/':
                                ParseClosingTag();
                                break;
                            default:
                                ParseTag();
                                ProcessTag();
                                break;
                        }
                        index++;
                        break;
                    case ' ':
                    case '\n':
                    case '\t':
                    case '\r':
                        index++;
                        break;
                    default:
                        ParseText();
                        ProcessText();
                        break;
                }
            }
            output.Append(']');

        }
        private bool EOF()
        {
            return index >= source.Length;
        }
    }
    static class Extenstions
    {
        public static StringBuilder TrimEnd(this StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return sb;

            int i = sb.Length - 1;
            for (; i >= 0; i--)
                if (!char.IsWhiteSpace(sb[i]))
                    break;

            if (i < sb.Length - 1)
                sb.Length = i + 1;

            return sb;
        }
    }
}
