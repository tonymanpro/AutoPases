using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AutoPases.Integracion
{
    public class ReadXml
    {
        public static string GetSqlStatement(string sqlName, string queryFile = "QueriesSql")
        {
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"Configuration\{0}.xml", queryFile));
            var document = XDocument.Load(filePath);
            var sqlPath = string.Format("/QUERIES/{0}", sqlName);
            var result = document.XPathSelectElements(sqlPath);
            if (result != null && result.Any())
            {
                return result.FirstOrDefault().Value.Trim();
            }
            return string.Empty;
        }
    }
}