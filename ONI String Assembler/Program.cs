using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using HtmlAgilityPack;

namespace ONI_String_Assembler
{
    class Utility
    {
        static public string get_string(string accessor)
        {
            try
            {
                var names = accessor.Split('.');
                Assembly asm = Assembly.GetExecutingAssembly();
                var my_type = asm.GetTypes()
                        .Where(type => type.Namespace == names[0])
                        .Where(type => type.Name == names[1])
                        .Single();
                for (var i = 2; i < names.Length - 1; i++)
                {
                    my_type = my_type.GetNestedType(names[i]);
                }
                return (string)my_type.GetField(names.Last()).GetValue(null);
            }
            catch (InvalidOperationException e)
            {
                return accessor;
            }
        }

        static public string get_string_gamepedia(string accessor)
        {
                var str = $"{get_string(accessor)}";
                // Replace all strings with format <xxx@yyy> with the html escaped version
                str = Regex.Replace(str, @"<(\w+@\w+\.\w+)>", match => $"&lt;{match.Groups[1].Value}&gt;\n");
                str = Regex.Replace(str, @"<alpha=#(\w+)>", match => $"<color opacity=\"#{match.Groups[1].Value}\">");
                str = Regex.Replace(str, @"<(\w+)=([\w\d#%]+)>", match => $"<{match.Groups[1].Value} arg=\"{match.Groups[2].Value}\">");
                str = str.Replace("------------------\n", "<hr></hr>");
                str = str.Replace("\n\n", "\n");
                var tree = new HtmlDocument();
                tree.LoadHtml(str);
                return RewriteHtmlNode(tree.DocumentNode, tree);
        }

        // rewrites ONI's fake-html. Need to recursively replace from leaf to root.
        static public string RewriteHtmlNode(HtmlNode node, HtmlDocument doc)
        {
            for (var i = 0; i < node.ChildNodes.Count; i++) {
                var child = node.ChildNodes[i];
                if (child.NodeType != HtmlNodeType.Text)
                {
                    var newnode = doc.CreateTextNode(RewriteHtmlNode(child, doc));
                    node.ReplaceChild(newnode, child);
                }
            }
            // now they're all text nodes, concat and then wrap in formatting.
            var inner = node.InnerText;
            var arg = "";

            switch (node.Name)
            {
                case "b":
                    return $"'''{inner}'''";
                case "color":
                    if (node.Attributes.Contains("opacity"))
                    {
                        var opac = Convert.ToInt32(node.Attributes["opacity"].Value.Substring(1), 16) / 255.0 * 100;
                        opac = Math.Floor(opac);
                        return $"<span style=\"opacity: {opac}%;\">{inner}</span>";
                    } else
                    {
                        arg = node.Attributes["arg"].Value;
                        return $"<span style=\"color: {arg};\">{inner}</span>";
                    }
                case "hr":
                    return $"−−−−−−−−−−−−−−−−−−";
                case "i":
                    return $"''{node.InnerText}''";
                case "indent":
                    arg = node.Attributes["arg"].Value;
                    var indent_degree = Convert.ToInt32(arg.Substring(0, arg.Length - 1)) / 5;
                    var prefix = new string(':', indent_degree);
                    // Must append every new line with the colons.
                    var prefixed_lines = inner.Split('\n').Select(line => $"{prefix} {line}");
                    return string.Join("\n", prefixed_lines);
                case "size":
                    arg = node.Attributes["arg"].Value;
                    return $"<span style=\"font-size: {arg}pt;\">{inner}</span>";
                case "smallcaps":
                    // Need to split by \n and wrap each line in an individual smallcaps.
                    var thing = inner.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(line => $"<span style=\"font-variant: small-caps;\">{line}</span>");
                    return string.Join("\n", thing);
                case "#document":
                    return inner;
                case "#email":
                    return $"<{node.Name}>";
                default:
                    return node.WriteTo();
            }
        }
    }

    class SortBySortstring : Comparer<EmailContainer>
    {
        public override int Compare(EmailContainer x, EmailContainer y)
        {
            if (x.sortString == null)
            {
                if (y.sortString == null)
                {
                    if (x.id.StartsWith("MyLog"))
                    {
                        var xval = int.Parse(x.id.Substring(5));
                        var yval = int.Parse(y.id.Substring(5));
                        return xval.CompareTo(yval);
                    }
                    return x.id.CompareTo(y.id);
                }
                return -1;
            }
            else if (y.sortString == null)
            {
                return 1;
            }
            return x.sortString.CompareTo(y.sortString);
        }
    }

    class Program
    {
        static T read_lore<T>(string path) where T: IGamepedia
        {
            string text = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder()
                .WithTagMapping("!CodexText", typeof(CodexNode))
                .WithTagMapping("!CodexDividerLine", typeof(CodexNode))
                .Build();
            return deserializer.Deserialize<T>(text);
        }

        static string assemble_lore<T>(EmailContainer lore)
        {
            return lore.ToGamepedia();
        }

        static void Main(string[] args)
        {
            var type = "Buildings";
            // var types = ["Emails", "Investigations", "MyLog", "Journals", "Notices", "ResearchNotes", "Plants"];
            var filenames = Directory.GetFiles("codex/" + type, "*", SearchOption.TopDirectoryOnly);
            var s = new System.Text.StringBuilder();

            var lores = new List<EmailContainer>();
            foreach(var path in filenames)
            {
                string result;
                lores.Add(read_lore <EmailContainer>(path));
            }

            lores.Sort(new SortBySortstring());

            File.WriteAllText($"output/{type}", string.Join("\n\n", lores.Select(email => email.ToGamepedia())));

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
