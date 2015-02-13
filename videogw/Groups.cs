using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice
{
    [XmlRootAttribute(ElementName = "groups")]
    public class Groups : List<group>
    {
    }

    public class group
    {
        [XmlElement(Order = 1)]
        public string id { get; set; }

        [XmlElement(Order = 2, ElementName = "english-name")]
        public string english_name { get; set; }

        [XmlElement(Order = 3, ElementName = "chinese-name")]
        public string chinese_name { get; set; }
    }

    /*
    [CollectionDataContract(Name = "groups")]
    public class Groups : List<group>
    {
    }
    [DataContract]
    public class group
    {
        [DataMember(Order = 1)]
        public string id { get; set; }

        [DataMember(Order = 2, Name = "english-name")]
        public string english_name { get; set; }

        [DataMember(Order = 3, Name = "chinese-name")]
        public string chinese_name { get; set; }

    }*/
    
    
}