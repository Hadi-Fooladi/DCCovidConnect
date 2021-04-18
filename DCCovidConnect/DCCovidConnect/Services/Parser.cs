using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCCovidConnect.Services
{
    /// <summary>
    /// This class parses the HTML from a WordPress page into a usuable JSON Object.
    /// </summary>
    public class Parser
    {
        private StringBuilder output = new StringBuilder();
        private StringBuilder parsedTag = new StringBuilder();
        private StringBuilder parsedTagContents = new StringBuilder();
        private StringBuilder parsedText = new StringBuilder();
        public List<string> ItemList { get; private set; } = new List<string>();
        /// <summary>
        /// This variable keeps track of the html element nesting
        /// </summary>
        private Stack<Type> tagStack = new Stack<Type>();
        enum Property
        {
            NONE, TYPE, HREF, TEXT, CHILDREN, SRC
        }
        enum Type
        {
            NONE, TITLE, ITEM, A, P, BOLD, UL, OL, LI, TEXT, COLOR, IMG, BR, H1, H2, H3, H4, H5, H6, SPAN, FIGURE, S, SUP, TABLE, TBODY, TD, TR, IFRAME
        }
        private Type[] textTags = new[] { Type.A, Type.P, Type.H1, Type.H2, Type.H3, Type.H4, Type.H5, Type.H6, Type.SPAN, Type.LI, Type.BOLD, Type.COLOR, Type.TITLE };
        private string source = "";
        /// <summary>
        /// This variable keeps track of the position of the parser in the HTML document.
        /// </summary>
        private int index = 0;
        public string Output
        {
            get => output.ToString().Replace("&nbsp;", "").Replace("&amp;", "&");
        }
        /// <summary>
        /// This constructor takes in the contents of the HTML document.
        /// </summary>
        /// <param name="source"></param>
        public Parser(string source)
        {
            this.source = source;
        }
        /// <summary>
        /// This method moves the <c>index</c> to the end of the HTML comment.
        /// </summary>
        private void ParseComment()
        {
            while (source[index] != '>')
            {
                index++;
            }
        }
        /// <summary>
        /// This method parses and stores the tag and its contents in the class variables, <c>parsedTag</c> and <c>parsedTagContents</c>
        /// It also moves the <c>index</c> to the end of the tag.
        /// </summary>
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
        /// <summary>
        /// This method moves the <c>index</c> to the end of the text and stores it into the <c>parsedText</c> variable.
        /// </summary>
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
            if (parsedTagContents.ToString().ToLower().Contains("title")) {
                ItemList.Add(parsedText.ToString());
            }
        }
        /// <summary>
        /// This method moves the <c>index</c> to the end of the closing tag and closes the JSON object.
        /// </summary>
        private void ParseClosingTag()
        {
            while (source[index] != '>')
            {
                index++;
            }
            if (tagStack.Pop() != Type.NONE)
            {
                output.Append("]}");
            }
        }
        /// <summary>
        /// This method processes the <c>parsedTag</c> and <c>parsedTagContents</c> and appends it to the outputted JSON.
        /// </summary>
        private void ProcessTag()
        {
            if (output[output.Length - 1] == '}')
            {
                output.Append(',');
            }
            Type tag = Type.NONE;
            Boolean selfClosing = false;
            // Checks the tag type
            switch (parsedTag.ToString().ToLower())
            {
                case "hr":
                case "svg":
                case "path":
                case "style":
                case "section":
                    tagStack.Push(Type.NONE);
                    return;
                case "div":
                    if (parsedTagContents.ToString().ToLower().Contains("item"))
                    {
                        tag = Type.ITEM;
                        break;
                    }
                    else
                    {
                        tagStack.Push(Type.NONE);
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
            // If this element has specific css classes, change the type.
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
                    tagStack.Push(Type.NONE);
                    return;
                }
            }
            output.Append($"{{\"{Property.TYPE}\":\"{tag}\"");

            // check the tag for attributes.
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
                tagStack.Push(tag);
            }
        }

        /// <summary>
        /// This method appends the attribute to the current node in the JSON object.
        /// </summary>
        /// <param name="prop">The type of property.</param>
        /// <param name="contents">The content of the property.</param>
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

        /// <summary>
        /// This method processes the text stored in <c>parsedText</c> and appends it to the outputted JSON object.
        /// </summary>
        private void ProcessText()
        {
            if (output[output.Length - 1] == '}')
            {
                output.Append(',');
            }
            output.Append($"{{\"{Property.TYPE}\":\"{Type.TEXT}\",");
            output.Append($"\"{Property.TEXT}\":");
            output.Append('"');
            output.Append(parsedText);
            output.Append("\"}");
        }

        /// <summary>
        /// This is the main method used to parse the inputted HTML source.
        /// </summary>
        public void Parse()
        {
            output.Append('[');
            while (!EOF())
            {
                // Check the type of tag, if it's not a tag, treat it as text
                if (source[index] == '<')
                {
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
                }
                else
                {
                    if (tagStack.Count != 0 && textTags.Contains(tagStack.Peek()))
                    {

                        ParseText();
                        ProcessText();
                    }
                    else index++;
                }
            }
            output.Append(']');
        }

        /// <summary>
        /// This method checks if the index exceeds the length of the HTML source.
        /// </summary>
        /// <returns>Returns if the length exceeds the source.</returns>
        private bool EOF()
        {
            return index >= source.Length;
        }
    }
}
