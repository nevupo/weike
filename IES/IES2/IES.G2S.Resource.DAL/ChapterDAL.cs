﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using IES.DataBase;
using Dapper;
using IES.Resource.Model;


namespace IES.G2S.Resource.DAL
{
    public class ChapterDAL
    {

        #region 列表

        /// <summary>
        /// 获取章节树形列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<Chapter> Chapter_List(Chapter model)
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@OCID", model.OCID);
                    return conn.Query<Chapter>("Chapter_List", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }


        /// <summary>
        /// 章节关联的文件列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<File> Chapter_File_List(Chapter model , Ken ken )
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@ChapterID", model.ChapterID);
                    p.Add("@UserID", model.CreateUserID);
                    p.Add("@KenID", ken.KenID);
                    return conn.Query<File>("Chapter_File_List", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }

        /// <summary>
        /// 章节关联的习题信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<Exercise> Chapter_Exercise_List(Chapter model, Ken ken)
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@ChapterID", model.ChapterID);
                    p.Add("@UserID", model.CreateUserID);
                    p.Add("@KenID", ken.KenID );
                    return conn.Query<Exercise>("Chapter_Exercise_List", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }



            

        #endregion 


        #region 详细信息



        #endregion 


        #region 新增

        /// <summary>
        /// 章节新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Chapter Chapter_ADD(Chapter model)
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@ChapterID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    p.Add("@OCID", model.OCID);
                    p.Add("@CourseID", model.CourseID);
                    p.Add("@OwnerUserID", model.OwnerUserID);
                    p.Add("@CreateUserID", model.CreateUserID); 
                    p.Add("@Title", model.Title );
                    p.Add("@Orde", model.Orde);
                    p.Add("@ParentID", model.ParentID );
                    conn.Execute("Chapter_ADD", p, commandType: CommandType.StoredProcedure);
                    model.ChapterID = p.Get<int>("ChapterID");
                    return model;
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }

        #endregion 


        #region 更新

        /// <summary>
        /// 章节更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool Chapter_Upd(Chapter model)
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@ChapterID", model.ChapterID);
                    p.Add("@Title", model.Title);
                    p.Add("@Orde", model.Orde);
                    p.Add("@parentID", model.ParentID);
                    conn.Execute("Chapter_Upd", p, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }


        /// <summary>
        /// 章节移动
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool Chapter_ParentID_Upd(Chapter model)
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@ChapterID", model.ChapterID);
                    p.Add("@ParentID", model.ParentID );
                    conn.Execute("Chapter_ParentID_Upd", p, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }   



        #endregion 


        #region 删除 
        /// <summary>
        ///  章节删除
        /// </summary>
        /// <param name="id"></param>
        public static bool Chapter_Del(Chapter model)
        {
            try
            {
                using (var conn = DbHelper.ResourceService())
                {
                    var p = new DynamicParameters();
                    p.Add("@ChapterID", model.ChapterID);
                    conn.Execute("Chapter_Del", p, commandType: CommandType.StoredProcedure);
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
