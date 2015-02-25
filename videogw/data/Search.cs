using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;

namespace Bupt.ImageCommLab.uvideoservice
{
    public class Search
    {
        [DllImport("Searchdll.dll", EntryPoint = "GetDevInfo", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetDevInfo();
    }
}