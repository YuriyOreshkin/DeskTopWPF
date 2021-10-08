using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DeskTopWPF.Domain
{
    [Serializable]
    public class Item
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Display { get; set; }
        [XmlAttribute]
        public bool Visible { get; set; }

        public Command Command { get; set; }
        public List<Item> MenuItems { get; set; }
    }

    [Serializable]
    public class Settings
    {
        public List<Item> MenuItems { get; set; }
    }

    [Serializable]
    public class Command
    {
        [XmlAttribute]
        public int Timeout { get; set; }
        [XmlText]
        public string FileName { get; set; }
    }

}

