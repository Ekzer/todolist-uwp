using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Todolist
{
    public static class Utils
    {
        public static string ToXML(Object o)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                var serializer = new XmlSerializer(o.GetType());
                serializer.Serialize(stringWriter, o);
                return stringWriter.ToString();
            }
        }

        public static T LoadFromXMLString<T>(string xmlText)
        {
            using (StringReader stringReader = new StringReader(xmlText))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        /// <summary>
		/// Get date in ISO8601 format with local timezone offset
		/// </summary>
		/// <returns>Date as ISO8601 string</returns>
		public static string GetDate()
        {
            return DateTime.Now.ToString("o");
        }

        public static bool IsAny<T>(this IEnumerable<T> data)
        {
            return data != null && data.Any();
        }
    }
}
