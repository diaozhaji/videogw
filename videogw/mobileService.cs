using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;

namespace Bupt.ImageCommLab.uvideoservice
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class mobileService
    {

        //命名空间
        public string ns = "http://www.uv.bupt.cn";
        /*
         * 读取本地xml文件
         * C:\Program Files\Common Files\microsoft shared\DevServer\10.0是默认路径,问题
         * 在app_data目录下，用Server.MapPath()方法解决
         */
        public XmlDocument LoadXml()
        {
            XmlDocument doc = new XmlDocument();
            //本地测试
            string path = System.Web.HttpContext.Current.Server.MapPath("/App_Data/videozone.xml");

            //IIS发布 正式发布是还要改~！ 下面save也要改
            //string path = System.Web.HttpContext.Current.Server.MapPath("/VideogwTest/App_Data/videozone.xml");
            doc.Load(path);
            return doc;
        }


        /*
         * 保存当前xml文件的修改
         */
        public void SaveXml(XmlDocument d)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("/App_Data/videozone.xml");
            //string path = System.Web.HttpContext.Current.Server.MapPath("/VideogwTest/App_Data/videozone.xml");
            d.Save(path);
        }

        /*
         * 生成16位ID的函数
         * 这个方法有点笨
         */
        public string CreateKey_16()
        {
            //睡眠50毫秒，保证调用很快时，生成的数不同
            System.Threading.Thread.Sleep(50);
            Random ran = new Random();
            int a, b;
            a = ran.Next(10000000, 99999999);
            b = ran.Next(10000000, 99999999);
            //C#存在问题，要new一个新的字符串
            string randomKey = new string(' ', 17);
            randomKey = Convert.ToString(a) + Convert.ToString(b);
            return randomKey;
        }

        /*
         * 视频区域
         * /videozone下操作
         * 支持GET,PUT
         * 获取当前视频区的信息
         */
        [WebGet(UriTemplate = "/videozone/all")]
        [XmlSerializerFormat]
        public videozone GetVideoZone()
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            videozone vz = new videozone();
            if (doc == null)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
            }
            else
            {
                string eName = doc.SelectSingleNode("/ns:videozone/ns:english-name", nsMgr).InnerText;
                string chName = doc.SelectSingleNode("/ns:videozone/ns:chinese-name", nsMgr).InnerText;
                vz.english_name = eName;
                vz.chinese_name = chName;
                vz.groups = null;
                vz.videos = null;
                return vz;
            }

        }

        [WebGet(UriTemplate = "/mobile /videozone /{videozone_ename}")]
        [XmlSerializerFormat]
        public videozone GetVideoZone(String videozone_ename) 
        {
            return null;
        }


    }
}