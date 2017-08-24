using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace nopCommerceLanguagePack
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var xmlEn = await LoadXmlWithUtf8Bom(@"C:\src\nopCommerce_3.90_git\src\Presentation\Nop.Web\App_Data\Localization\defaultResources.nopres.xml");
            var xmlZh = await LoadXmlWithUtf8Bom(@"C:\src\nopCommerce-ChineseLanguagePack\zhs.xml");

            var enResources = GetResources(xmlEn);
            var zhResources = GetResources(xmlZh);

            Console.WriteLine($"en count: {enResources.Count}");
            Console.WriteLine($"zh count: {zhResources.Count}");

            var enNames = enResources.Select(t => t.Item1);
            var zhNames = zhResources.Select(t => t.Item1);
            Console.WriteLine($"intersect count: {enNames.Intersect(zhNames, StringComparer.InvariantCultureIgnoreCase).Count()}");

            //var zhAdditionNames = zhNames.Where(n => !enNames.Contains(n));
            //foreach (var zhAdditionName in zhAdditionNames)
            //{
            //    Console.WriteLine(zhAdditionName);
            //}

            /*
            // Add missing values
            for (var i = 0; i < xmlZh.LastChild.ChildNodes.Count; ++i)
            {
                var resource = xmlZh.LastChild.ChildNodes[i];
                var name = resource.Attributes["Name"].InnerText;
                var value = resource.FirstChild.InnerText;
                if (string.IsNullOrEmpty(value))
                {
                    resource.FirstChild.InnerText = enResources.FirstOrDefault(enres => enres.Item1.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Item2;
                }
            }
            //*/

            var enAdditionResources = enResources.Where(enr => !zhResources.Any(zhr => string.Equals(zhr.Item1, enr.Item1, StringComparison.InvariantCultureIgnoreCase)));
            Console.WriteLine(enAdditionResources.Count());

            /*
            // add missing fields
            foreach (var enAddRes in enAdditionResources)
            {
                var nameAttribute = (XmlAttribute)xmlZh.CreateNode(XmlNodeType.Attribute, "Name", null);
                nameAttribute.InnerText = enAddRes.Item1;

                var valueNode = xmlZh.CreateNode(XmlNodeType.Element, "Value", null);
                valueNode.InnerText = enAddRes.Item2;

                var node = xmlZh.CreateNode(XmlNodeType.Element, "LocaleResource", null);
                node.Attributes.Append(nameAttribute);
                node.AppendChild(valueNode);

                xmlZh.LastChild.AppendChild(node);
            }
            //*/

            //await SaveXmlWithUtf8Bom(xmlZh, @"C:\src\nopCommerce-ChineseLanguagePack\zhs2.xml");
        }

        static async Task SaveXmlWithUtf8Bom(XmlDocument xml, string path)
        {
            var _byteOrderMarkUtf8 = Encoding.UTF8.GetPreamble();
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                await stream.WriteAsync(_byteOrderMarkUtf8, 0, _byteOrderMarkUtf8.Length);
                xml.Save(stream);
            }
        }

        static List<Tuple<string, string>> GetResources(XmlDocument xml)
        {
            var result = new List<Tuple<string, string>>();
            for (var i = 0; i < xml.LastChild.ChildNodes.Count; ++i)
            {
                var resource = xml.LastChild.ChildNodes[i];
                var name = resource.Attributes["Name"].InnerText;
                var value = resource.FirstChild.InnerText;
                result.Add(Tuple.Create(name, value));
            }

            return result;
        }

        static async Task<XmlDocument> LoadXmlWithUtf8Bom(string path)
        {
            var text = await File.ReadAllTextAsync(path);
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (text.StartsWith(_byteOrderMarkUtf8))
            {
                var lastIndexOfUtf8 = _byteOrderMarkUtf8.Length - 1;
                text = text.Remove(0, lastIndexOfUtf8);
            }

            var xml = new XmlDocument();
            xml.Load(new StringReader(text));
            return xml;
        }
    }
}