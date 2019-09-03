using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONI_String_Assembler
{
    interface IGamepedia
    {
        string ToGamepedia();
    }

    class CodexNode: IGamepedia
    {
        public string stringKey;
        public string style;
        public int? preferredWidth;

        public string ToGamepedia()
        {
            if (preferredWidth == null)
            {
                var str = Utility.get_string_gamepedia(stringKey);
                switch (style)
                {
                    case "Title":
                        return "";
                    case "Subtitle":
                        return $"=== {str} ===";
                    case "Body":
                        return str;
                    default:
                        Console.WriteLine($"Unknown style '{style}', skipping.");
                        return str;
                }
            }
            else
            {
                return "\n----";
            }
        }

        public override string ToString()
        {
            if (preferredWidth == null)
            {
                return Utility.get_string(stringKey);
            } else
            {
                return "~";
            }
        }
    }

    class EmailContent : IGamepedia
    {
        public string contentLayout;
        public string lockID;
        public CodexNode[] content;

        public override string ToString()
        {
            return String.Join("\n", content.Select(node => node.ToString()));
        }

        public string ToGamepedia()
        {
            return String.Join("\n", content.Select(node => node.ToGamepedia()));
        }
    }

    class EmailContainer : IGamepedia
    {
        public string id;
        public string title;
        public string sortString;

        public EmailContent[] contentContainers;
        public string ToGamepedia()
        {
            var title = Utility.get_string(this.title);
            var contentstring = String.Join("\n", contentContainers.Select(content => content.ToGamepedia()));
            contentstring = contentstring.Replace("\n", "\n\n");
            return $"== {title} ==\n\n:''Unique name: <code>{id}</code>''{contentstring}";
        }

        public override string ToString()
        {
            var title = Utility.get_string(this.title);
            var contentstring = String.Join("", contentContainers.Select(content => content.ToString()));
            return $" {title} [{id}] \n{contentstring}";
        }
    }
}
