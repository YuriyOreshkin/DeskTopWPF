using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DeskTopWPF.Domain
{
    public static class Service
    {
        

        public static void SaveSettings(Settings settings,string filename)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Settings));
            TextWriter writer = new StreamWriter(filename, false, Encoding.GetEncoding(1251));
            formatter.Serialize(writer, settings);
            writer.Close();

        }

        public static Settings ReadSettings(string filename)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Settings));

            using (StreamReader fs = new StreamReader(filename, Encoding.GetEncoding(1251), false))
            {
                Settings settings = (Settings)formatter.Deserialize(fs);
                fs.Close();
                return settings;
            }
        }
    }
}
