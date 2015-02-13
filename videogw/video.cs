using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice
{
    [XmlRootAttribute]
    public class video
    {
        [XmlElement(Order = 1)]
        public string id { get; set; }
        //第二版时，修改了这里，后面PUT,POST中要做出相应修改
        [XmlElement(Order = 2, ElementName = "status")]
        public string status { get; set; }

        [XmlElement(Order = 3, ElementName = "type")]
        public string TPYE { get; set; }

        [XmlElement(Order = 4, ElementName = "english-name")]
        public string english_name { get; set; }

        [XmlElement(Order = 5, ElementName = "chinese-name")]
        public string chinese_name { get; set; }

        [XmlElement(Order = 6)]
        public string description { get; set; }

        [XmlElement(Order = 7, ElementName = "position")]
        public position p = new position();

        [XmlElement(Order = 8, ElementName = "security-level")]
        public string security_level { get; set; }

        [XmlElement(Order = 9, ElementName = "source")]
        public source src = new source();

        [XmlElement(Order = 10, ElementName = "publish")]
        public publish pub = new publish();

        [XmlElement(Order = 11, ElementName = "static")]
        public statics s = new statics();

        [XmlElement(Order = 12, ElementName = "groupings")]
        public groupings grs = new groupings();

        
    }


    public class position
    {
        [XmlElement(Order = 1)]
        public double x { get; set; }
        [XmlElement(Order = 2)]
        public double y { get; set; }
    }

    public class statics
    {
        [XmlElement(Order = 1)]
        public bool generate { get; set; }
        [XmlElement(Order = 2, ElementName = "abstract")]
        public bool abstracts { get; set; }
    }

    public class publish
    {
        [XmlElement(ElementName = "address")]
        //public address addr = new address();
        public List<address> addr_list = new List<address>();
    }

    public class address
    {
        [XmlAttribute]
        //public string type = "stream";
        public string type { get; set; }
        [XmlText]
        public string ad { get; set; }
    }

    public class groupings
    {
        [XmlElement(ElementName = "group")]
        public List<group_id> gid = new List<group_id>();
    }

    public class source
    {
        [XmlElement]
        public string address { get; set; }

        [XmlElement(ElementName = "user")]
        public user u = new user();
    }


    public class user
    {
        public string name { get; set; }
        public string password { get; set; }
    }

    public class group_id
    {
        public string id { get; set; }
    }
}