using System;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace XmlSearch.XML
{
    public class DomXmlAnalyser
    {
        public DomXmlAnalyser()
        {

        }
        static List<string> leaf = new List<string>();

        public static void parsingXmlDocument()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("/Users/rsnhn/Projects/XmlSearch/XmlSearch/sales.xml");
            RecurseNodes(xmlDoc.DocumentElement);
        }

        private static void RecurseNodes(XmlNode node)
        {
            var sb = new StringBuilder();
            //починаємо рекурсивний перегляд з рівня 0
            RecurseNodes(node, 0, sb);
            //друкуємо сформований рядок
            Debug.WriteLine(sb.ToString());
            Debug.WriteLine("-----------------------");
            string leafsString = String.Join(", ", leaf);
            Debug.WriteLine(leafsString);

        }

        private static void RecurseNodes(XmlNode node, int level, StringBuilder sb)
        {
            sb.AppendFormat("{0,-2} Type:{1,-9} Name:{2,-13} Attr: ", level, node.NodeType, node.Name);

            if (node.NodeType == XmlNodeType.Text) leaf.Add(node.ParentNode.Name);

            if (node.Attributes == null)
            {
                sb.AppendLine("Text: " + (node.HasChildNodes ? node.FirstChild.InnerXml : node.InnerXml));
                return;
            }
            
            foreach (XmlAttribute attr in node.Attributes)
            {
                sb.AppendFormat("{0}={1} ", attr.Name, attr.Value);
                leaf.Add(attr.Name);
            }
            sb.AppendLine("Text: " + (node.HasChildNodes ? node.FirstChild.InnerXml : node.InnerXml));

            foreach (XmlNode n in node.ChildNodes)
            {
                RecurseNodes(n, level + 1, sb);
            }
        }

        private static string getFilePath(string fileName)
        {
            return Path.Combine("/Users/rsnhn/Projects/XmlSearch/XmlSearch/Resources", fileName);
        }

        public static List<string> getLeaf() { return leaf; }

    }
}

