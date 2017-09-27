using System;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace CSharpHtmlFormatter
{
    public class HtmlFormatter
    {
        private static readonly string[] IgnoreTextAsName = { ",", HtmlEntities.Less, HtmlEntities.Greater };

        private readonly HtmlDocument htmlDocument = new HtmlDocument();

        public string Transform(string sourceHtml)
        {
            string updatedText = sourceHtml.
                Trim().
                Replace("style=\"color:blue;\"", "class=\"keyword\"").
                Replace("style=\"color:#2b91af;\"", "class=\"type\"").
                Replace(HtmlEntities.Less, "<wbr>" + HtmlEntities.Less);

            htmlDocument.LoadHtml(updatedText);

            string memberNameText = (from n in htmlDocument.DocumentNode.ChildNodes
                                     where n.NodeType == HtmlNodeType.Text
                                     let text = n.InnerText.Trim()
                                     where !string.IsNullOrEmpty(text)
                                     where !string.IsNullOrWhiteSpace(IgnoreTextAsName.Aggregate(text, (result, ignore) => result.Replace(ignore, null)))
                                     select n.InnerText).
                                     First();

            int startIndexOfBody = updatedText.IndexOf(memberNameText);

            if (memberNameText.StartsWith(HtmlEntities.Greater))
                startIndexOfBody += 4;
            else if (memberNameText.StartsWith("[]"))
                startIndexOfBody += 2;

            string head = startIndexOfBody == 0 ? null : updatedText.Substring(0, startIndexOfBody);
            string body = updatedText.Substring(startIndexOfBody).TrimStart();

            string tail = ExtractMemberTail(ref body);

            string[] memberWhereClauses = ExtractMemberWhere(ref tail);

            return Format(head, body, tail, memberWhereClauses);
        }

        private string ExtractMemberTail(ref string memberBody)
        {
            string memberTail = null;

            string memberBodyCopy = memberBody;
            int[] indicesOfTailSeparator = new[] { "(", "{", "= " }.Select(x => memberBodyCopy.IndexOf(x)).Where(x => x != -1).ToArray();
            int startIndexOfTail = indicesOfTailSeparator.Any() ? indicesOfTailSeparator.Min() : -1;

            if (startIndexOfTail != -1)
            {
                if (memberBody[startIndexOfTail - 1] == ' ')
                    startIndexOfTail--;

                memberTail = memberBody.Substring(startIndexOfTail);
                if (memberTail == "()")
                {
                    memberTail = null;
                }
                else
                {
                    memberBody = memberBody.Substring(0, startIndexOfTail);
                }
            }

            return memberTail;
        }

        private string[] ExtractMemberWhere(ref string tail)
        {
            string spanOfWhere = "<span class=\"keyword\">where</span>";

            if (tail != null)
            {
                string[] tailParts = tail.Split(new[] { spanOfWhere }, StringSplitOptions.RemoveEmptyEntries);

                if (tailParts.Length > 1)
                {
                    tail = tailParts[0].Trim();

                    return tailParts.Skip(1).Select(x => spanOfWhere + x.TrimEnd()).ToArray();
                }
            }
            return new string[0];
        }

        private string Format(string head, string body, string tail, string[] whereClauses)
        {
            StringBuilder builder = new StringBuilder().
                AppendLine("<div class=\"member\">");

            if (head != null)
            {
                builder.
                    Append($"    <span class=\"head\">{head}</span>").AppendLine();
            }

            builder.
                Append($"    <h3><span class=\"body\">{body}</span>");

            if (tail != null)
            {
                builder.Append($"<span class=\"tail\">{tail}</span>");
            }

            builder.AppendLine("</h3>");

            if (whereClauses.Any())
            {
                foreach (string memberWhere in whereClauses)
                {
                    builder.AppendLine($"    <span class=\"where\">{memberWhere}</span>");
                }
            }

            builder.Append("</div>");

            return builder.ToString();
        }

        public static class HtmlEntities
        {
            public const string Less = "&lt;";

            public const string Greater = "&gt;";
        }
    }
}
