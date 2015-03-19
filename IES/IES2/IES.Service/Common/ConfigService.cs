﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IES.JW.Model;
using IES.CC.OC.Model;
using IES.G2S.JW.DAL;
using IES.G2S.JW.BLL;
using IES.Cache;
using IES.Resource.Model;
using IES.G2S.Resource.DAL;
using IES.Security;

namespace IES.Service.Common
{
    /// <summary>
    /// 系统配置服务
    /// </summary>
    public class ConfigService
    {

        /// <summary>
        /// 获取系统的描述信息列表
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<CfgSchool> CfgSchool_List()
        {
            CfgSchoolBLL cfgbll = new CfgSchoolBLL();
            return cfgbll.CfgSchool_List();
        }

        /// <summary>
        /// 获取课程中心系统的基本描述信息
        /// </summary>
        public static CfgSchool CfgSchool_CC
        {
            get
            {
                return CfgSchool_List().Where(o => o.SysID == 1).ToList<CfgSchool>()[0];
            }
        }

        /// <summary>
        /// 身份验证地址
        /// </summary>
        public static string CASURL
        {
            get
            {
                return "http://192.168.4.231:6667/portal/login.aspx";
            }
        }

        public static string G2SURL
        {
            get
            {
                return "http://192.168.4.231:513/G2S/";
            }
        }

        public static string ResourceURL
        {
            get
            {
                return "http://192.168.4.231:513/Resource/";
            }
        }

        public static string AdminURL
        {
            get
            {
                return "http://192.168.4.231:513/Admin/";
            }
        }

    }
}
