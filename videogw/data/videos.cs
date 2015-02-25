using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Bupt.ImageCommLab.uvideoservice
{
    [XmlRootAttribute(ElementName = "videos")]
    public class videos:List<video>
    {

    }

    //上面这个用在了all_videos类中，大小写什么的以后再改
    
    //下面这个是在search中使用的，我觉得名字需要再改改
    [XmlRootAttribute(ElementName="videos")]
    public class search_video_list:List<video>
    {

    }
    /*
    public class Video 
    {
        [XmlElement(ElementName="source")]
        public Source src = new Source();
    }
    public class Source 
    {
        [XmlElement(ElementName="address")]
        public string address;
    }*/
}