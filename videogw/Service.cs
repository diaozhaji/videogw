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
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    // NOTE: If the service is renamed, remember to update the global.asax.cs file
    public class Service
    {
        // TODO: Implement the collection resource that will contain the SampleItem instances
        
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
            doc.Load(path) ;
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
            string randomKey = new string(' ',17);
            randomKey = Convert.ToString(a) + Convert.ToString(b);
            return randomKey;
        }


        /*
         * 视频区域
         * /videozone下操作
         * 支持GET,PUT
         * 获取当前视频区的信息
         */
        [WebGet(UriTemplate = "")]
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
        
        /*
         * 更新当前视频区的信息
         * /videozone下操作
         * PUT
         * DataContract序列化器与xml序列化器混用时，一定要注明该方法具体使用了哪个
         */
        [WebInvoke(UriTemplate = "", Method = "PUT")]
        [XmlSerializerFormat]
        public void UpdateVideoZone(videozone v)
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);
            XmlNode chNameNode = doc.SelectSingleNode("/ns:videozone/ns:chinese-name", nsMgr);
            chNameNode.InnerText = v.chinese_name;
            XmlNode eNameNode = doc.SelectSingleNode("/ns:videozone/ns:english-name", nsMgr);
            eNameNode.InnerText = v.english_name;
            SaveXml(doc);

        }
        /*
         * 搜索接口
         */
        [WebGet(UriTemplate = "/search")]
        [XmlSerializerFormat]
        public search_video_list GetSourceAddress() 
        {
            IntPtr str = Search.GetDevInfo();
            string s = Marshal.PtrToStringAnsi(str);
            //test
            string test = "10.102.0.217|10.102.0.225|10.102.0.224|";
            //string[] sArry =test.Split('|');
            string[] sArry = s.Split('|');
            search_video_list v_list = new search_video_list();
            for (int i = 0; i < sArry.Length - 1; i++) 
            {
                Video V = new Video();
                V.src.address = sArry[i];
                v_list.Add(V);
            }
            return v_list;
        }
        
        /*
         * 视频组  
         * /videozone/videogroups下的操作   ！这个暂时删掉了，不用这个方法
         * GET
         * 获取当前视频区中所有的视频组信息
         */
        [WebGet(UriTemplate = "/videogroups")]
        [XmlSerializerFormat]
        public Groups GetGroups()
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            XmlNodeList nodes = doc.SelectNodes("/ns:videozone/ns:groups/ns:group", nsMgr);
            Groups gs = new Groups();
            foreach (XmlNode node in nodes)
            {
                XmlNode n_id = node.SelectSingleNode("ns:id", nsMgr);
                XmlNode n_eName = node.SelectSingleNode("ns:english-name", nsMgr);
                XmlNode n_chName = node.SelectSingleNode("ns:chinese-name", nsMgr);
                group g = new group();
                g.id = n_id.InnerText;
                g.english_name = n_eName.InnerText;
                g.chinese_name = n_chName.InnerText;
                gs.Add(g);

            }
            return gs;

        }

        
        /*
         * 得到某一个视频组的信息，文档中没有，但调试时做POST时有用
         * /videozone/videogroup/{id}下操作
         */
        [WebGet(UriTemplate = "/videogroup/{id}")]
        [XmlSerializerFormat]
        public group GetGroup(string id)
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            XmlNodeList nodes = doc.SelectNodes("/ns:videozone/ns:groups/ns:group", nsMgr);
            group g = new group();

            foreach (XmlNode node in nodes) 
            {
                XmlNode n_id = node.SelectSingleNode("ns:id", nsMgr);
                if (id == n_id.InnerText)
                {
                    XmlNode n_eName = node.SelectSingleNode("ns:english-name", nsMgr);
                    XmlNode n_chName = node.SelectSingleNode("ns:chinese-name", nsMgr);
                    g.id = n_id.InnerText;
                    g.english_name = n_eName.InnerText;
                    g.chinese_name = n_chName.InnerText;
                }
                else 
                {
                    //do noing
                }
            }
            return g;

        }


        /*
         * 增加一个视频组
         * /videozone/videogroup操作
         * POST方式
         * 新添加的group有命名空间的问题
         * 声明子节点的ns，解决
         * 通过XmlDocument编辑Xml 这个方法比较笨
         * id要再写一个查重的机制
         */
        [WebInvoke(UriTemplate = "/videogroup", Method = "POST")]
        [XmlSerializerFormat]
        public void CreateGroup(group G)
        {
            group g = new group();
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            XmlNodeList nodes_groups = doc.SelectNodes("/ns:videozone/ns:groups/ns:group", nsMgr);
            //group的id是16位随机数
            g.id = CreateKey_16();
            g.english_name = G.english_name;
            g.chinese_name = G.chinese_name;

            XmlNode node = doc.SelectSingleNode("/ns:videozone/ns:groups", nsMgr);
            if (node == null) return;

            XmlElement main = doc.CreateElement("group", ns);
            node.AppendChild(main);
            XmlElement sub1 = doc.CreateElement("id", ns);
            sub1.InnerText = g.id;
            main.AppendChild(sub1);
            XmlElement sub2 = doc.CreateElement("english-name", ns);
            sub2.InnerText = g.english_name;
            main.AppendChild(sub2);
            XmlElement sub3 = doc.CreateElement("chinese-name", ns);
            sub3.InnerText = g.chinese_name;
            main.AppendChild(sub3);     //总感觉没有</root> 但是实际上是有的
            SaveXml(doc);

        }


        /*
         * 更新某个视频组
         * /videozone/videogroup/{id}下操作哦
         * PUT方式
         * 对id的理解问题
         * 关于id，已经改成16位
         */
        [WebInvoke(UriTemplate = "/videogroup/{id}", Method = "PUT")]
        [XmlSerializerFormat]
        public void UpdateGroup(string id, group G)
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);
            XmlNodeList group_list = doc.SelectNodes("ns:videozone/ns:groups/ns:group",nsMgr);
            foreach (XmlNode node in group_list) 
            {
                XmlNode g_id = node.SelectSingleNode("ns:id", nsMgr);
                string s_id = g_id.InnerText;
                if (s_id == id) 
                {
                    XmlNode eNameNode = node.SelectSingleNode("ns:english-name", nsMgr);
                    eNameNode.InnerText = G.english_name;
                    XmlNode chNameNode = node.SelectSingleNode("ns:chinese-name", nsMgr);
                    chNameNode.InnerText = G.chinese_name;
                }
            }

            SaveXml(doc);
        }


        /*
         * 删除一个视频组
         * /videozone/videogroup/{id}下操作
         * DELETE方式
         */
        [WebInvoke(UriTemplate = "/videogroup/{id}", Method = "DELETE")]
        [XmlSerializerFormat]
        public void DeleteGroup(string id)
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            XmlNodeList group_list = doc.SelectNodes("ns:videozone/ns:groups/ns:group", nsMgr);
            foreach (XmlNode node in group_list)
            {
                XmlNode g_id = node.SelectSingleNode("ns:id", nsMgr);
                string s_id = g_id.InnerText;
                if (s_id == id)
                {
                    XmlElement element = (XmlElement)node;
                    element.ParentNode.RemoveChild(node);
                }
            }
            //最好设计一个标志位，来是否删除成功，现在这个方法无论是否删除了，都会给出200的提示码

            /*
            XmlNode main = doc.SelectSingleNode("/ns:videozone/ns:groups/ns:group[" + id + "]", nsMgr);
            XmlElement element = (XmlElement)main;
            //解决了会剩下<group>标签的问题，以前用的是当前节点的removeall()方法
            element.ParentNode.RemoveChild(main);
             */

            SaveXml(doc);

        }

        /*
         * 视频资源
         * 获取视频区中所有的视频资源以及所属组的信息(文档中新加入的方法)
         * videozone/videos下操作
         * GET方式
         */
        [WebGet(UriTemplate = "/videos")]
        [XmlSerializerFormat]
        public videozone GetAllVideos()
        {
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);
            
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
         * 视频资源
         * 获取某个动态视频资源的信息
         * /videozone/video/{id}下操作
         * id是资源的id，如111，而不是资源的顺序
         * GET方式
         */
        [WebGet(UriTemplate = "/video/{id}")]
        [XmlSerializerFormat]
        public video GetVideo(string id)
        {
            id = id + "";
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            video v = new video();
            XmlNodeList node_videos = doc.SelectNodes("/ns:videozone/ns:videos/ns:video", nsMgr);
            //Xpath是从1开始，不是从0开始，所以i=1
            for (int i = 1; i < node_videos.Count + 1; i++)
            {
                XmlNode id_node = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:id", nsMgr);
                if (id == id_node.InnerText)
                {
                    v.id = id_node.InnerText;
                    v.english_name = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:english-name", nsMgr).InnerText;
                    v.chinese_name = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:chinese-name", nsMgr).InnerText;
                    v.description = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:description", nsMgr).InnerText;
                    v.p.x = Double.Parse(doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:position/ns:x", nsMgr).InnerText);
                    v.p.y = Double.Parse(doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:position/ns:y", nsMgr).InnerText);
                    v.security_level = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:security-level", nsMgr).InnerText;
                    v.src.address = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:address", nsMgr).InnerText;
                    v.src.u.name = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:user/ns:name", nsMgr).InnerText;
                    v.src.u.password = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:user/ns:password", nsMgr).InnerText;
                    
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
                    v.TPYE = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:type", nsMgr).InnerText;

                }
                /*
                else
                {
                    throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
                }*/
            }
            return v;

        }

        /*
         * 添加视频动态资源
         * POST
         * 尝试不使用xmlDocument 使用xmlWriter
         * 这里有groupings 作为数组list的写入,但是PUT需要重写，思路不同
         * 
         * 由于演示，改了很多，添加了查重功能
         * 
         */
        [WebInvoke(UriTemplate = "/video/{id}", Method = "POST")]
        [XmlSerializerFormat]
        public void CreateVideo(string id,video V)
        {
            id = id + "";
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            //XmlNodeList nodes_videos = doc.SelectNodes("/ns:videozone/ns:videos/ns:video", nsMgr);
            //V.id = nodes_videos.Count + 1 + "";

            XmlNodeList node_videos = doc.SelectNodes("/ns:videozone/ns:videos/ns:video", nsMgr);
            //Xpath是从1开始，不是从0开始，所以i=1
            int flag = 0;
            int reId = 0;
            for (int i = 1; i < node_videos.Count + 1; i++)
            {
                XmlNode address_node = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:address", nsMgr);
                if (V.src.address == address_node.InnerText) 
                {
                    flag++;
                    reId = i;
                }
            }
            if (flag == 0)
            {
                V.id = CreateKey_16();

                XmlNode node = doc.SelectSingleNode("/ns:videozone/ns:videos", nsMgr);
                if (node == null) return;

                XmlElement main = doc.CreateElement("video", ns);
                node.AppendChild(main);
                XmlElement sub1 = doc.CreateElement("id", ns);
                sub1.InnerText = V.id;
                main.AppendChild(sub1);
                //这里是修改过的，所以命名本来在后面
                XmlElement sub11 = doc.CreateElement("status", ns);
                sub11.InnerText = V.status;
                main.AppendChild(sub11);
                XmlElement sub12 = doc.CreateElement("type", ns);
                sub12.InnerText = V.TPYE;
                main.AppendChild(sub12);
                XmlElement sub2 = doc.CreateElement("english-name", ns);
                sub2.InnerText = V.english_name;
                main.AppendChild(sub2);
                XmlElement sub3 = doc.CreateElement("chinese-name", ns);
                sub3.InnerText = V.chinese_name;
                main.AppendChild(sub3);
                XmlElement sub4 = doc.CreateElement("description", ns);
                sub4.InnerText = V.description;
                main.AppendChild(sub4);
                XmlElement sub5 = doc.CreateElement("position", ns);
                main.AppendChild(sub5);
                XmlElement sub5_x = doc.CreateElement("x", ns);
                sub5_x.InnerText = V.p.x + "";
                sub5.AppendChild(sub5_x);
                XmlElement sub5_y = doc.CreateElement("y", ns);
                sub5_y.InnerText = V.p.y + "";
                sub5.AppendChild(sub5_y);
                XmlElement sub6 = doc.CreateElement("security-level", ns);
                sub6.InnerText = V.security_level + "";
                main.AppendChild(sub6);
                XmlElement sub7 = doc.CreateElement("source", ns);
                main.AppendChild(sub7);
                XmlElement sub7_address = doc.CreateElement("address", ns);
                sub7_address.InnerText = V.src.address;
                sub7.AppendChild(sub7_address);
                XmlElement sub7_user = doc.CreateElement("user", ns);
                sub7.AppendChild(sub7_user);
                XmlElement sub7_user_name = doc.CreateElement("name", ns);
                sub7_user_name.InnerText = V.src.u.name;
                sub7_user.AppendChild(sub7_user_name);
                XmlElement sub7_user_password = doc.CreateElement("password", ns);
                sub7_user_password.InnerText = V.src.u.password;
                sub7_user.AppendChild(sub7_user_password);
                //publish同groupings
                XmlElement sub8 = doc.CreateElement("publish", ns);
                main.AppendChild(sub8);
                int address_count = V.pub.addr_list.Count;
                string ran = CreateKey_16();
                for (int j = 0; j < address_count; j++)
                {
                    XmlElement sub8_address = doc.CreateElement("address", ns);
                    sub8_address.SetAttribute("type", V.pub.addr_list[j].type);
                    //V.src.address = "10.102.5.226/live";
                    if (V.pub.addr_list[j].type == "stream")
                    {
                        //sub8_address.InnerText = "rtmp://" + V.src.address + "/" + ran;
                        sub8_address.InnerText = "rtmp://10.102.5.226/live/" + ran;
                    }
                    if (V.pub.addr_list[j].type == "picture")
                    {
                        //sub8_address.InnerText = "http://" + V.src.address + "/" + ran;
                        sub8_address.InnerText = "http://10.102.5.226/live/" + ran;
                    }
                    //sub8_address.InnerText = V.pub.addr_list[i].ad;
                    sub8.AppendChild(sub8_address);

                }
                XmlElement sub9 = doc.CreateElement("static", ns);
                main.AppendChild(sub9);
                XmlElement sub9_generate = doc.CreateElement("generate", ns);
                if (V.s.generate == true) sub9_generate.InnerText = "true";
                else sub9_generate.InnerText = "false";
                sub9.AppendChild(sub9_generate);
                XmlElement sub9_abstracts = doc.CreateElement("abstract", ns);
                if (V.s.abstracts == true) sub9_abstracts.InnerText = "true";
                else sub9_abstracts.InnerText = "false";
                sub9.AppendChild(sub9_abstracts);
                //groupings 比较复杂
                XmlElement sub10 = doc.CreateElement("groupings", ns);
                main.AppendChild(sub10);
                //设置一个group数量的变量，只为直观
                int group_cont = V.grs.gid.Count;
                for (int j = 0; j < group_cont; j++)
                {
                    XmlElement node_group = doc.CreateElement("group", ns);
                    sub10.AppendChild(node_group);
                    XmlElement group_id = doc.CreateElement("id", ns);
                    group_id.InnerText = V.grs.gid[j].id;
                    node_group.AppendChild(group_id);

                }
                SaveXml(doc);


            }
            else 
            {
                XmlNode status_node = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + reId + "]/ns:status", nsMgr);
                status_node.InnerText = "active";
                SaveXml(doc);
                //throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
            }

            
        }


        /*
         * 更新某个动态视频资源的信息
         * PUT方式
         * 暂时用这种依次修改的思路，
         * 其实可以完全删除这个要修改的节点内容，再调用之前的方法，修改xml文件
         */
        [WebInvoke(UriTemplate = "/video/{id}", Method = "PUT")]
        [XmlSerializerFormat]
        public void UpdateVideo(string id,video V) 
        {
            //id = id + "";
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            XmlNodeList node_videos = doc.SelectNodes("/ns:videozone/ns:videos/ns:video", nsMgr);
            //Xpath是从1开始，不是从0开始，所以i=1
            for (int i = 1; i < node_videos.Count + 1; i++)
            {
                XmlNode id_node = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:id", nsMgr);
                if (id == id_node.InnerText)
                {
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:english-name", nsMgr).InnerText = V.english_name;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:chinese-name", nsMgr).InnerText = V.chinese_name;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:description", nsMgr).InnerText = V.description;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:position/ns:x", nsMgr).InnerText = V.p.x + "";
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:position/ns:y", nsMgr).InnerText = V.p.y + "";
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:security-level", nsMgr).InnerText = V.security_level + "";
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:address", nsMgr).InnerText = V.src.address;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:user/ns:name", nsMgr).InnerText = V.src.u.name;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:source/ns:user/ns:password", nsMgr).InnerText = V.src.u.password;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:publish", nsMgr).RemoveAll();
                    int address_count = V.pub.addr_list.Count;
                    for (int j = 0; j < address_count; j++)
                    {
                        XmlElement sub8_address = doc.CreateElement("address", ns);
                        sub8_address.InnerText = V.pub.addr_list[j].ad;
                        doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:publish", nsMgr).AppendChild(sub8_address);
                        sub8_address.SetAttribute("type", V.pub.addr_list[j].type);
                    }
                    //注意属性的写法，不用这个了，文档有修改
                    //doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:publish/ns:address", nsMgr).Attributes["type"].Value = V.pub.addr.type;
                    if (V.s.generate == true){
                        doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:static/ns:generate", nsMgr).InnerText = "true";
                    }
                    else{
                        doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:static/ns:generate", nsMgr).InnerText = "false";
                    }
                    if (V.s.abstracts == true){
                        doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:static/ns:abstract", nsMgr).InnerText = "true";
                    }
                    else{
                        doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:static/ns:abstract", nsMgr).InnerText = "false";
                    }
                    //doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:groupings/ns:group/ns:id", nsMgr).InnerText = V.grs.gri.id;
                    //这里改好了 可以多个group传过来修改
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:groupings", nsMgr).RemoveAll();
                    int group_cont = V.grs.gid.Count;
                    for (int j = 0; j < group_cont; j++)
                    {
                        XmlElement node_group = doc.CreateElement("group", ns);
                        doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:groupings", nsMgr).AppendChild(node_group);
                        XmlElement group_id = doc.CreateElement("id", ns);
                        group_id.InnerText = V.grs.gid[j].id;
                        node_group.AppendChild(group_id);

                    }

                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:status", nsMgr).InnerText = V.status;
                    doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:type", nsMgr).InnerText = V.TPYE;

                }

                else
                {
                    //throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
                }
            }
            SaveXml(doc);

        }


        /*
         * 删除某个动态视频资源，将它不可用，而不会“删除”
         * DELETE方式
         */
        [WebInvoke(UriTemplate = "/video/{id}", Method = "DELETE")]
        [XmlSerializerFormat]
        public void DeleteVideo(string id) 
        {
            id = id + "";
            XmlDocument doc = new XmlDocument();
            doc = LoadXml();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns", ns);

            XmlNodeList node_videos = doc.SelectNodes("/ns:videozone/ns:videos/ns:video", nsMgr);
            //Xpath是从1开始，不是从0开始，所以i=1
            for (int i = 1; i < node_videos.Count + 1; i++)
            {
                XmlNode id_node = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:id", nsMgr);
                if (id == id_node.InnerText)
                {
                    XmlNode status_node = doc.SelectSingleNode("/ns:videozone/ns:videos/ns:video[" + i + "]/ns:status", nsMgr);
                    status_node.InnerText = "inactive";
                }

                else
                {
                    //throw new WebFaultException(System.Net.HttpStatusCode.NotFound);
                }
            }
            SaveXml(doc);

        }

       
    }
}
