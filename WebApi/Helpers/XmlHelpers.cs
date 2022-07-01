using System.Xml;
using System.Xml.Serialization;
using WebApi.Models;

namespace WebApi.Helpers
{
    public static class XmlHelpers
    {
        public static string XmlSerialize<T>(this T pdfDocument)
        {
            var xml = "";
            var serializer = new XmlSerializer(typeof(T));
            using (var sww = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    serializer.Serialize(writer, pdfDocument);
                    xml = sww.ToString(); // Your XML
                }
            }
            return xml;
            //var jsonString = JsonConvert.SerializeObject(pdfDocument);
            //bf.Serialize(ms, pdfDocument);
            //
        }
    }
}
