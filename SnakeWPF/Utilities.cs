using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnakeWPF
{
    public static class Utilities
    {
        public static void LoadList<T>(string xmlName, ObservableCollection<T> list)
        {
            if (File.Exists("highscorelist.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
                using (Stream reader = new FileStream(xmlName, FileMode.Open))
                {
                    List<T> tempList = (List<T>)serializer.Deserialize(reader);
                    list.Clear();
                    foreach (var item in tempList)
                        list.Add(item);
                }
            }
        }

        public static void SaveList<T>(string xmlName, ObservableCollection<T> list)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<T>));
            using (Stream writer = new FileStream(xmlName, FileMode.Create))
            {
                serializer.Serialize(writer, list);
            }
        }
    }
}