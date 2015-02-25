using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice.data
{
    [XmlRootAttribute]
    public class response
    {
        [XmlElement(Order = 1, ElementName = "msg")]
        public string msg { get; set; }

        [XmlElement(Order = 2, ElementName = "data")]
        public string data { get; set; }
    }
}