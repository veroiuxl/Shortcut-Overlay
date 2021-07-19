using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ShortcutOverlay
{
    class FileLayoutManager
    {
        public void SerializeXml(List<SaveableButton> information,FileStream stream)
        {
            using (FileStream writer = stream)
            {
                // You can just parse in a List and it will Serialize it for you correctly!
                DataContractSerializer ser = new DataContractSerializer(typeof(List<SaveableButton>)); 
                ser.WriteObject(writer,information);
                Debug.WriteLine("Wrote " + writer.Length + " bytes");
                writer.Close();
            }
        }
        public List<SaveableButton> DeserializeXML(FileStream stream)
        {
            using (FileStream fs = stream)
            {
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(List<SaveableButton>));

                // Deserialize the data and read it from the instance.

                List<SaveableButton> deserializedButton =
                    (List<SaveableButton>)ser.ReadObject(reader, true);
                Debug.WriteLine("Read " + fs.Length + " bytes");
                reader.Close();
                fs.Close();
                return deserializedButton;
            }

        }
    }
}
