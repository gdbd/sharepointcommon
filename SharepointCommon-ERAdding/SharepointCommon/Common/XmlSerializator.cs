using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SharepointCommon.Common
{
    internal class XmlSerializator
    {
        internal static string Serialize<T>(T obj) where T : class
        {
            try
            {
                string xmlizedString;
                var memoryStream = new MemoryStream();
                var xs = new XmlSerializer(typeof(T));
                var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xs.Serialize(xmlTextWriter, obj);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                xmlizedString = Utf8ByteArrayToString(memoryStream.ToArray());
                return xmlizedString;
            }
            catch
            {
                return null;
            }
        }

        internal static T Deserialize<T>(string str) where T : class
        {
            try
            {
                var xs = new XmlSerializer(typeof(T));
                var memoryStream = new MemoryStream(StringToUtf8ByteArray(str));
                return (T)xs.Deserialize(memoryStream);
            }
            catch
            {
                return null;
            }
        }

        private static string Utf8ByteArrayToString(byte[] characters)
        {
            var encoding = new UTF8Encoding();
            var constructedString = encoding.GetString(characters);
            return constructedString;
        }

        private static byte[] StringToUtf8ByteArray(string pXmlString)
        {
            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
    }
}
