using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice
{
    [XmlRootAttribute]
    public class SampleItem
    {
        [XmlElement(Order = 1, ElementName = "id")]
        public int Id { get; set; }
        [XmlElement(Order = 2, ElementName = "String")]
        public string StringValue { get; set; }
    }
}