using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Main.Helper
{
    public static class XmlHelper
    {
        public static void SaveXml<T>(
            this T obj,
            string filePath
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var serializer = new XmlSerializer(typeof(T));
            //serializer.UnknownNode += Serializer_UnknownNode;
            //serializer.UnknownElement += Serializer_UnknownElement;
            //serializer.UnknownAttribute += Serializer_UnknownAttribute;
            //serializer.UnreferencedObject += Serializer_UnreferencedObject;
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fileStream, obj);
            }
        }


        public static T ReadXml<T>(
            this string filePath
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var serializer = new XmlSerializer(typeof(T));
            //serializer.UnknownNode += Serializer_UnknownNode;
            //serializer.UnknownElement += Serializer_UnknownElement;
            //serializer.UnknownAttribute += Serializer_UnknownAttribute;
            //serializer.UnreferencedObject += Serializer_UnreferencedObject;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var result = (T)serializer.Deserialize(fileStream);

                return result;
            }

        }

        //private static void Serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        //{
        //    throw new InvalidOperationException(e.UnreferencedObject.ToString());
        //}

        //private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        //{
        //    throw new InvalidOperationException(e.Attr.OuterXml);
        //}

        //private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        //{
        //    throw new InvalidOperationException(e.Element.OuterXml);
        //}

        //private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        //{
        //    throw new InvalidOperationException(e.Text);
        //}
    }
}
