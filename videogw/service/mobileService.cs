using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;
using Bupt.ImageCommLab.uvideoservice.data;

namespace Bupt.ImageCommLab.uvideoservice
{
    /*
     * 为使web服务统一于一个程序，故新建一个mobileService类来实现对手机客户端的web请求
     * 使功能分开，具体实现与Service类本身非常相似，且同时操作一个xml配置文件。
     */
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class mobileService
    {
        //与Service共用的xml
        private XmlDocument doc;
        private XmlNamespaceManager nsMgr;

        //保存手机用户信息的xml
        private XmlDocument userDos;

        private Boolean debug = true;
        //命名空间
        public string ns = "http://www.uv.bupt.cn";

        public XmlDocument LoadXml(String fileName)
        {
            XmlDocument d = new XmlDocument();
            String path;
            if (debug)
            {
                path = System.Web.HttpContext.Current.Server.MapPath("/App_Data/" + fileName);
            }
            else
            {
                // App_Data/前需要加入IIS发布的虚拟路径（根据发布方式不同，需要做一定的调整）
                path = System.Web.HttpContext.Current.Server.MapPath("/VideogwTest/App_Data/" + fileName);
            }
            d.Load(path);
            return d;
        }
        // 读取配置文件
        private XmlDocument LoadVideozoneXml()
        {
            return LoadXml("videozone.xml");
        }
        // 读取用户信息
        private XmlDocument LoadUsersXml()
        {
            return LoadXml("users.xml");
        }

        /*
         * 保存文件
         * 
         * @para 文档,文件名
         */
        public void saveXml(XmlDocument d, String fileName)
        {
            String path;
            if (debug)
            {
                path = System.Web.HttpContext.Current.Server.MapPath("/App_Data/" + fileName);
            }
            else
            {
                path = System.Web.HttpContext.Current.Server.MapPath("/VideogwTest/App_Data/" + fileName);
            }
            d.Save(path);
        }

        private void saveVideozoneXml(XmlDocument d)
        {
            saveXml(d, "videozone.xml");
        }

        private void saveUserXml(XmlDocument d)
        {
            saveXml(d, "users.xml");
        }


        /*
         * 初始化
         */
        public void init()
        {
            doc = LoadVideozoneXml();
            nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);
        }

        public void initUsers()
        {
            userDos = LoadUsersXml();
            nsMgr = new XmlNamespaceManager(userDos.NameTable);
            nsMgr.AddNamespace("ns", ns);
        }

        /*
         * all接口为我自己设计，在文档中并未出现，为多个视频区不同xml文件扩展
         * 具体实现的方法未完成，只是复制了Service类中获取当前视频区的信息的方法
         */
        [WebGet(UriTemplate = "/videozone/all")]
        [XmlSerializerFormat]
        public videozone GetVideoZone()
        {
            init();

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

        /*
         * 视频资源
         * 获取视频区中所有的视频资源以及所属组的信息(文档中新加入的方法)
         * videozone/videos下操作
         * GET方式
         */
        [WebGet(UriTemplate = "/videozone/{videozone_ename}")]
        [XmlSerializerFormat]
        public videozone GetAllVideos(String videozone_ename)
        {
            init();

            //all_videos分2个部分，一部分是groups，一部分是videos

            videozone allvideos = new videozone();
            allvideos.id = null;
            allvideos.english_name = null;
            allvideos.chinese_name = null;

            //groups部分
            XmlNodeList nodes = doc.SelectNodes("/ns:videozone/ns:groups/ns:group", nsMgr);
            foreach (XmlNode node in nodes)
            {
                XmlNode n_id = node.SelectSingleNode("ns:id", nsMgr);
                XmlNode n_eName = node.SelectSingleNode("ns:english-name", nsMgr);
                XmlNode n_chName = node.SelectSingleNode("ns:chinese-name", nsMgr);
                group g = new group();
                g.id = n_id.InnerText;
                g.english_name = n_eName.InnerText;
                g.chinese_name = n_chName.InnerText;
                allvideos.groups.Add(g);
            }

            //video部分
            XmlNodeList node_videos = doc.SelectNodes("/ns:videozone/ns:videos/ns:video", nsMgr);
            for (int i = 1; i < node_videos.Count + 1; i++)
            {
                video v = new video();
                v.id = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:id", nsMgr).InnerText;
                v.english_name = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:english-name", nsMgr).InnerText;
                v.chinese_name = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:chinese-name", nsMgr).InnerText;
                v.description = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:description", nsMgr).InnerText;
                v.p.x = Double.Parse(doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:position/ns:x", nsMgr).InnerText);
                v.p.y = Double.Parse(doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:position/ns:y", nsMgr).InnerText);
                v.security_level = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:security-level", nsMgr).InnerText;
                v.src.address = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:address", nsMgr).InnerText;
                v.src.u.name = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:user/ns:name", nsMgr).InnerText;
                v.src.u.password = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:user/ns:password", nsMgr).InnerText;
                //v.pub.addr.ad = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:publish/ns:address", nsMgr).InnerText;
                //注意属性的写法
                //v.pub.addr.type = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:publish/ns:address", nsMgr).Attributes["type"].Value;
                //publish也是个数组，可以有多个<address>节点                
                XmlNodeList nodes_publish = doc.SelectNodes("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:publish/ns:address", nsMgr);
                foreach (XmlNode node in nodes_publish)
                {
                    address addre = new address();
                    addre.ad = node.InnerText;
                    addre.type = node.Attributes["type"].Value;
                    v.pub.addr_list.Add(addre);
                }
                if (doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:static/ns:generate", nsMgr).InnerText == "true") { v.s.generate = true; }
                else { v.s.generate = false; }
                if (doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:static/ns:abstract", nsMgr).InnerText == "true") { v.s.abstracts = true; }
                else { v.s.abstracts = false; }
                XmlNodeList groups_nodes = doc.SelectNodes("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:groupings/ns:group", nsMgr);
                foreach (XmlNode node in groups_nodes)
                {
                    group_id gr_id = new group_id();
                    gr_id.id = node.FirstChild.InnerText;
                    v.grs.gid.Add(gr_id);
                }
                v.status = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:status", nsMgr).InnerText;
                //新添的TYPE已经加在这里了
                v.TPYE = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:type", nsMgr).InnerText;

                allvideos.videos.Add(v);
            }
            return allvideos;
        }

        /*
         * 注册
         * POST
         */
        [WebInvoke(UriTemplate = "/login", Method = "POST")]
        [XmlSerializerFormat]
        public response createUser(user u) 
        {
            initUsers();
            response mRes = new response();

            XmlNodeList nameNodes = userDos.SelectNodes("//ns:name", nsMgr);
            // 判断新用户名是否存在
            foreach (XmlNode nameNode in nameNodes)
            {
                if (u.name == nameNode.InnerText)
                {
                    mRes.msg = "0";
                    mRes.data = "用户名已存在";
                    return mRes;
                }
            }

            XmlNode node = userDos.SelectSingleNode("/ns:users", nsMgr);
            XmlElement main = userDos.CreateElement("user", ns);
            node.AppendChild(main);
            XmlElement sub1 = userDos.CreateElement("name", ns);
            sub1.InnerText = u.name;
            main.AppendChild(sub1);
            XmlElement sub2 = userDos.CreateElement("password", ns);
            sub2.InnerText = u.password;
            main.AppendChild(sub2);
            saveUserXml(userDos);
            mRes.msg = "1";
            mRes.data = "注册成功";

            return mRes;

        }

        /*
         * 登陆
         * GET
         */
        [WebGet(UriTemplate = "/login/{name}/{password}")]
        [XmlSerializerFormat]
        public response isUser(String name, String password)
        {
            initUsers();

            response mRes = new response();

            XmlNodeList nameNodes = userDos.SelectNodes("//ns:name", nsMgr);
            foreach (XmlNode nameNode in nameNodes)
            {
                Console.Write("node name = " + nameNode.InnerText);
                if (name == nameNode.InnerText)
                {
                    XmlNode passwordNode = nameNode.ParentNode.SelectSingleNode("ns:password", nsMgr);
                    if (password == passwordNode.InnerText)
                    {
                        mRes.msg = "1";
                        mRes.data = "登陆成功";
                        return mRes;
                    }
                    mRes.msg = "0";
                    mRes.data = "密码错误";
                    return mRes;
                }
            }
            mRes.msg = "0";
            mRes.data = "用户不存在";
            return mRes;
        }

        /**
         * 修改密码
         * PUT
         */
        [WebInvoke(UriTemplate = "/login", Method = "PUT")]
        [XmlSerializerFormat]
        public response UpdateVideoZone(user u)
        {
            initUsers();

            response mRes = new response();

            XmlNodeList nameNodes = userDos.SelectNodes("//ns:name", nsMgr);
            foreach (XmlNode nameNode in nameNodes)
            {
                if (u.name == nameNode.InnerText)
                {
                    XmlNode passwordNode = nameNode.ParentNode.SelectSingleNode("ns:password", nsMgr);
                    if (u.password == passwordNode.InnerText)
                    {
                        mRes.msg = "0";
                        mRes.data = "与原密码相同";
                        return mRes;
                    }
                    else
                    {

                        passwordNode.InnerText = u.password;
                        saveUserXml(userDos);
                        mRes.msg = "1";
                        mRes.data = "修改密码成功";
                        return mRes;
                    }
                }
            }
            mRes.msg = "0";
            mRes.data = "用户名错误";
            return mRes;
        }

    }
}