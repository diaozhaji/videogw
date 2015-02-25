using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice
{
    /**
     * videozone 作为根节点，下有5个子节点
     * */
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
        public videos videos = new videos();
    }
}
