using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice
{
    // TODO: Edit the SampleItem class
    
    [XmlRootAttribute("videozone")]
    public class videozone
    {
        [XmlElement(Order = 1)]
        public string id { get; set; }
        [XmlElement(Order = 2, ElementName = "english-name")]
        public string english_name { get; set; }
        [XmlElement(Order = 3, ElementName = "chinese-name")]
        public string chinese_name { get; set; }

        [XmlElement(Order = 4, ElementName = "groups")]
        public Groups groups = new Groups();
        [XmlElement(Order = 5, ElementName = "videos")]
        public video_list videos = new video_list();
    }
    /*
    [DataContract]
    public class videozone
    {
        [DataMember]
        public string id { get; set; }
        [DataMember(Order = 1, Name = "english-name")]
        public string english_name { get; set; }
        [DataMember(Order = 2, Name = "chinese-name")]
        public string chinese_name { get; set; }
    }*/


}
