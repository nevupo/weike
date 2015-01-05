﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IES.Portal.Model;
using System.Data;
using System.Data.SqlClient;
using IES.DataBase;
using Dapper;

namespace IES.G2S.Portal.DAL
{
    public class HelpDAL
    {
        #region 列表
        public static List<Help> Help_List(Help model, int PageIndex, int PageSize)
        {
            try
            {
                using (var conn = DbHelper.PortalService())
                {
                    var p = new DynamicParameters();
                    p.Add("@PageSize", PageSize);
                    p.Add("@PageIndex", PageIndex);

                    return conn.Query<Help>("Help_List", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region  新增
        public static Help Help_ADD(Help model)
        {
            try
            {
                using (var conn = DbHelper.PortalService())
                {
                    var p = new DynamicParameters();                  
                    p.Add("@Title", model.Title);
                    p.Add("@Content", model.Content);
                    p.Add("@SysID", model.Sysid);
                    p.Add("@OrganizationID", model.Organizationid);
                    p.Add("@ModuleID", model.Moduleid);
                    p.Add("@Clicks", model.Clicks);

                    conn.Execute("Help_ADD", p, commandType: CommandType.StoredProcedure);
                    model.Helpid = p.Get<int>("HelpID");
                    return model;
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }



        #endregion

        #region 删除
        public static bool Help_Del(Help model)
        {
            try
            {
                using (var conn = DbHelper.PortalService())
                {
                    var p = new DynamicParameters();
                    p.Add("@HelpID", model.Helpid);
                    conn.Execute("Help_Del", p, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion

        #region 更新

        public static bool Help_Upd(Help model)
        {
            try
            {
                using (var conn = DbHelper.PortalService())
                {
                    var p = new DynamicParameters();
                    p.Add("@HelpID", model.Helpid);
                    p.Add("@Title", model.Title);
                    p.Add("@Content", model.Content);
                    
                    conn.Execute("Help_Upd", p, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion
    }
}
