using System;
using System.IO;
using System.Text;
using System.Xml;

namespace MSNChatlogConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.xml");
            foreach (var file in files)
            {
                ParseFile(file);
            }
        }

        private static void ParseFile(string file)
        {
            try
            {   
                XmlDocument document = new XmlDocument();
                document.Load(file);
                
                FileInfo fileInfo = new FileInfo(file);
                string outputName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length) + ".html";

                string htmlOutput = ParseDocument(document);
                using (StreamWriter sw = new StreamWriter(outputName))
                {
                    sw.Write(htmlOutput);
                }

                Console.WriteLine($"Finished creating file {outputName}");
            }
            catch
            {
                Console.Write($"Failed to parse {file}", ConsoleColor.Red);
            }
        }
        
        private static string ParseDocument(XmlDocument document)
        {
            /*
             * Structure of XML file:
             * <Log FirstSessionID="1" LastSessionID="1" Archive="true">
             *   <Message Date="2010-01-01" Time="12:00:00" SessionID="1">
             *     <From>
             *       <User FriendlyName=""/>
             *     </From>
             *     <To>
             *       <User FriendlyName=""/>
             *     </To>
             *     <Text Style="font-family:Comic Sans MS; font-weight:bold; color:#000000; ">
             *       message
             *     </Text
             *   </Message>
             * </log>
             */

            var htmlOutput = new StringBuilder();

            htmlOutput.Append("<html><head><title>Nuusie was here.</title></head><body>");
            
            var logElements = document.GetElementsByTagName("Log");
            foreach (XmlNode logElement in logElements)
            {
                var messages = logElement.SelectNodes("Message");
                foreach (XmlNode message in messages)
                {
                    string date = GetAttributeValue(message, "Date");
                    string time = GetAttributeValue(message, "Time");
                    
                    var from = message.SelectSingleNode("From")?.SelectSingleNode("User");
                    string fromName = GetAttributeValue(from, "FriendlyName");
                    
                    var text = message.SelectSingleNode("Text");
                    var style = GetAttributeValue(text, "Style");

                    htmlOutput.Append($"<p style='{style}'>\n" +
                                      $"  <span>{date} {time}</span>" +
                                      $"  <span>{fromName}:</span>" +
                                      $"  <span>{text.InnerText}</span>" +
                                      "</p>\n");
                }
            }

            htmlOutput.Append("</body>");
            return htmlOutput.ToString();
        }

        private static string GetAttributeValue(XmlNode node, string attributeName)
        {
            if (node == null)
                return "";
            
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name == attributeName)
                {
                    return attribute.Value;
                }
            }

            return "";
        }
    }
}