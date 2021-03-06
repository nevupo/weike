﻿using App.Score.Data;
using App.Score.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace App.Web.Score.DataProvider
{
    public partial class Analyze : System.Web.UI.Page
    {
        [WebMethod]
        public static IList<GradeCourse> GetCourses(int micYear, GradeCode gradeCode, TestLogin testLogin)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = "Select testno,coursecode from s_tb_testlogin"
                       + " where academicyear=@micYear"
                       + " and testno=@testNo";
                DataTable table = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo });
                if (string.IsNullOrEmpty(table.Rows[0]["coursecode"].ToString()) || table.Rows[0]["coursecode"].ToString() == "00000")
                {
                    sql = "select a.Academicyear, a.GradeNo, a.CourseCode, b.FullName from tbcourseuse a,tdcoursecode b"
                               + " where academicyear=@micYear"
                               + " and gradeno=@gradeNo"
                               + " and a.coursecode=b.coursecode";
                    return bll.FillListByText<GradeCourse>(sql, new { micYear = micYear, gradeNo = gradeCode.GradeNo });
                }
                else
                {
                    sql = "select * from tdcourseCode where coursecode=@courseCode";
                    return bll.FillListByText<GradeCourse>(sql, new { courseCode = table.Rows[0]["coursecode"].ToString() });
                }
            }
        }

        [WebMethod]
        public static IList<GradeCourse> GetCoursesByTestLogin(int micYear, TestLogin testLogin)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = "Select * from s_tb_testlogin"
                       + " where academicyear=@micYear"
                       + " and testno=@testNo";
                DataTable table = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo });

                var mTestType = string.IsNullOrEmpty(table.Rows[0]["testtype"].ToString()) ? "0" : table.Rows[0]["testtype"].ToString();
                var mGradeNo1 = string.IsNullOrEmpty(table.Rows[0]["GradeNo"].ToString()) ? "" : table.Rows[0]["GradeNo"].ToString();
                var testMark = string.IsNullOrEmpty(table.Rows[0]["Marktypecode"].ToString()) ? "1100" : table.Rows[0]["Marktypecode"].ToString();
                var testCourse = table.Rows[0]["CourseCode"].ToString();
                if (testCourse == "00000")
                {
                    sql = " SELECT {0} as Academicyear, b.GradeNo, b.CourseCode, a.FullName "
                          + " FROM  tdCourseCode a INNER JOIN "
                          + "  tbCourseUse b ON a.CourseCode = b.coursecode"
                          + " where b.AcademicYear=@micYear"
                          + " and gradeno=@gradeNo"
                          + " group by a.BriefName, b.coursecode, b.GradeNo, a.FullName ";
                    sql = string.Format(sql, micYear);
                    return bll.FillListByText<GradeCourse>(sql, new { micYear = micYear, gradeno = mGradeNo1 });
                }
                else
                {
                    sql = "select {0} as Academicyear, FullName, CourseCode  from tdcourseCode where coursecode=@courseCode";
                    return bll.FillListByText<GradeCourse>(sql, new { courseCode = table.Rows[0]["coursecode"].ToString() });
                }
            }
        }
        private static void gf_ScoreOrderA(string OrderSQL, string courseCode, string writeTableName, string fieldName, int writeBack)
        {
            using (AppBLL bll = new AppBLL())
            {
                var tempTableName = App.Score.Db.UtilBLL.mf_getTable();
                try
                {
                    var sql = "create table {0}(academicYear char(4),Semester char(2),TestType char(1),TestNo char(5),CourseCode char(5),srid char(19),Score numeric(5,1),OrderNO integer)";
                    sql = string.Format(sql, tempTableName);
                    bll.ExecuteNonQueryByText(sql);

                    sql = "insert into {0}(academicYear,Semester,TestType,TestNo,CourseCode,srid,Score,OrderNO) {1}";
                    sql = string.Format(sql, tempTableName, OrderSQL);
                    bll.ExecuteNonQueryByText(sql);

                    //开始排名
                    sql = string.Format("select row_number() over(order by score desc) as num, score from (select distinct score from {0}) t", tempTableName);
                    DataTable table = bll.FillDataTableByText(sql);
                    var length = table.Rows.Count;
                    var orderNo = 1;
                    for (int i = 0; i < length; i++)
                    {
                        sql = string.Format("update {0} set OrderNO={1} where score=@score", tempTableName, orderNo++);
                        var score = float.Parse(table.Rows[i]["score"].ToString());
                        bll.ExecuteNonQueryByText(sql, new { score = score });
                    }
                    if (writeBack == 9)
                    {
                        //写回原表
                        sql = "UPDATE {0}"
                               + " SET {1} = a.OrderNo"
                               + " FROM {2} as a INNER JOIN {0} as b "
                               + " ON a.SRID = b.SRID "
                               + " and a.Academicyear= b.Academicyear"
                               + " and a.TestNo=b.testno";
                        sql = string.Format(sql, writeTableName, fieldName, tempTableName);
                        bll.ExecuteNonQueryByText(sql);
                        //写成绩表
                        sql = " UPDATE s_tb_normalscore"
                                    + " SET GradeOrder = a.OrderNo "
                                    + " FROM {0} as a INNER JOIN s_tb_normalscore as b"
                                    + " ON a.SRID = b.SRID"
                                    + " and a.Academicyear= b.Academicyear"
                                    + " and a.TestNo=b.testno"
                                    + " and b.coursecode =@courseCode";
                        sql = string.Format(sql, tempTableName);
                        bll.ExecuteNonQueryByText(sql, new { courseCode = courseCode });
                    }
                    else if (writeBack == 0)
                    {
                        sql = " UPDATE {0}"
                                   + " SET {1} = a.OrderNo "
                                   + " FROM {2} as a INNER JOIN {0} as b "
                                   + " ON a.SRID = b.SRID"
                                   + " and a.Academicyear= b.Academicyear"
                                   + " and a.TestNo=b.testno";
                        sql = string.Format(sql, writeTableName, fieldName, tempTableName);
                        bll.ExecuteNonQueryByText(sql);
                    }
                }
                finally
                {
                    //删除临时表
                    var sql = "if exists(select * from sysobjects where name = '{0}' and xtype='U') drop table {0}";
                    sql = string.Format(sql, tempTableName);
                    bll.ExecuteNonQueryByText(sql);
                }
            }
        }

        private static int gf_GetStdScoreB(int micYear, string testNo, string courseCode, string classCode)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = "select avg(Numscore) as AvgScore,stdevp(numscore) as stdScore"
                          + " from s_vw_ClassScoreNum"
                          + " where Academicyear=@micYear"
                          + " and TestNo=@testNo"
                          + " and CourseCode=@courseCode"
                          + " and ClassCode in ({0})"
                          + " and Numscore<200"
                          + " and Numscore is not Null";
                sql = string.Format(sql, classCode);
                DataTable table = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testNo, courseCode = courseCode });
                var avgScore = float.Parse(table.Rows[0]["AvgScore"].ToString());
                var s = float.Parse(table.Rows[0]["stdScore"].ToString());
                //计算标准分
                if (s == 0)
                {
                    //方差为零，标准分为零
                    sql = " Update s_tb_Normalscore Set NormalScore = 0"
                                + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                + " ON a.SRID = b.SRID"
                                + " and a.Academicyear= b.Academicyear"
                                + " and a.TestNo=b.testno"
                                + " and a.coursecode=b.coursecode"
                                + " where a.ClassCode in (@classCode)"
                                + " and b.Academicyear=@micYear"
                                + " and b.TestNo=@testNo"
                                + " and b.CourseCode=@courseCode"
                                + " and b.Numscore is not Null";
                    bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testNo, courseCode = courseCode, classCode = classCode });
                }
                else
                {
                    sql = " update s_tb_normalscore "
                               + " set NormalScore=(b.NumScore-@avgScore)/(@Sscore)"
                               + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                               + " ON a.SRID = b.SRID"
                               + " and a.Academicyear= b.Academicyear"
                               + " and a.TestNo=b.testno"
                               + " and a.coursecode=b.coursecode"
                               + " where a.Academicyear=@micYear"
                               + " and a.TestNo=@testNo"
                               + " and a.ClassCode in (@classCode)"
                               + " and a.CourseCode=@courseCode"
                               + " and b.Numscore<200 "
                               + " and b.Numscore is not Null ";
                    bll.ExecuteNonQueryByText(sql, new
                    {
                        micYear = micYear,
                        testNo = testNo,
                        courseCode = courseCode,
                        classCode = classCode,
                        avgScore = avgScore,
                        Sscore = s
                    });
                }
                return 0;
            }
        }

        private static void gp_GetTScoreB(int micYear, string testNo, string courseCode, string classCode)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = " select max(normalscore) maxBZScore,min(normalscore) minBZScore "
                       + " from s_vw_ClassScoreNum "
                       + " where Academicyear=@micYear"
                       + " and Testno=@testNo"
                       + " and classcode in ({0})"
                       + " and CourseCode=@courseCode";
                sql = string.Format(sql, classCode);

                DataTable table = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testNo, courseCode = courseCode });
                if (table.Rows.Count == 0) return;
                var XT = float.Parse(table.Rows[0]["maxBZScore"].ToString());
                var YT = float.Parse(table.Rows[0]["minBZScore"].ToString());
                if (XT == 0 || YT == 0) return;
                int k = (int)Math.Floor(25 / XT);
                if (k > Math.Abs(Math.Floor(75 / YT))) k = (int)Math.Abs(Math.Floor(75 / YT));

                sql = " UPDATE b"
                        + " SET b.standardScore = (75+b.Normalscore*({0}))"
                        + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                        + " ON a.SRID = b.SRID"
                        + " and a.Academicyear= b.Academicyear"
                        + " and a.TestNo=b.testno"
                        + " and a.coursecode=b.coursecode"
                        + " where a.Academicyear=@micYear"
                        + " and a.Testno=@testNo"
                        + " and a.classcode in ({1})"
                        + " and a.CourseCode=@courseCode"; ;
                sql = string.Format(sql, k, classCode);
                bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testNo, courseCode = courseCode });
            }
        }

        private static void gp_ScoreTjA(int micYear, string Semester, string testType, string testNo, string courseCode, string gradeOrClassNo, int flag)
        {
            //先将有本年本学期的统计数据清空
            using (AppBLL bll = new AppBLL())
            {
                var sql = "";
                sql = "Delete from {0} where Academicyear='{1}' and Testno='{2}' and CourseCode='{3}' and {4}='{5}'";
                sql = string.Format(sql, flag == 0 ? "s_tb_GradeStat" : "s_tb_ClassStat", micYear, testNo, courseCode, flag == 0 ? "GradeNo" : "ClassNo", gradeOrClassNo);
                bll.ExecuteNonQueryByText(sql);
                //先插入数据
                if (flag == 0)
                {
                    sql = "insert Into s_tb_GradeStat(AcademicYear,Semester,CourseCode,TestType,TestNo,GradeNo)"
                           + " values('{0}','{1}','{2}','{3}','{4}', '{5}')";
                    sql = string.Format(sql, micYear, Semester, courseCode, testType, testNo, gradeOrClassNo);
                }
                else
                {
                    sql = "insert Into s_tb_ClassStat(AcademicYear,Semester,CourseCode,TestType,TestNo,ClassNo)"
                            + " values('{0}','{1}','{2}','{3}','{4}', '{5}')";
                    sql = string.Format(sql, micYear, Semester, courseCode, testType, testNo, gradeOrClassNo);

                }
                bll.ExecuteNonQueryByText(sql);
                //将统计数据修改为当前正确值
                //先将0-0.05数据添入

                sql = " select count(*) as S_5 from s_vw_ClassScoreNum"
                       + " where NumScore/cast(substring(Markcode,2,3) as numeric(5,2) )<0.05"
                       + " and substring(Markcode,1,1)='1'"
                       + " and Academicyear='{0}'"
                       + " and TestNo='{1}'"
                       + " and CourseCode='{2}'"
                       + " and STATE is NULL "
                       + " and {3}={4}";
                sql = string.Format(sql, micYear, testNo, courseCode, flag == 0 ? "GradeNo" : "ClassCode", gradeOrClassNo);
                DataTable table = bll.FillDataTableByText(sql);
                var s_num = int.Parse(table.Rows[0]["S_5"].ToString());

                //更新到数据库
                if (flag == 0)
                {
                    sql = "Update s_tb_GradeStat set S_5={4}"
                           + " where AcademicYear='{0}'"
                           + " and TestNo='{1}'"
                           + " and CourseCode='{2}'"
                           + " and GradeNo='{3}'";
                }
                else
                {
                    sql = "Update s_tb_ClassStat set S_5={4}"
                          + " where AcademicYear='{0}'"
                          + " and TestNo='{1}'"
                          + " and CourseCode='{2}'"
                          + " and GradeNo='{3}'";
                }
                sql = string.Format(sql, micYear, testNo, courseCode, gradeOrClassNo, s_num);
                bll.ExecuteNonQueryByText(sql);

                //二将S_100数据添入 
                sql = "select count(*) as S_5 from s_vw_ClassScoreNum"
                       + " where NumScore/cast(substring(Markcode,2,3) as numeric(5,2) )>=0.95 "
                       + " and substring(Markcode,1,1)='1'"
                       + " and Academicyear='{0}'"
                       + " and TestNo='{1}'"
                       + " and CourseCode='{2}'"
                       + " and STATE is NULL ";
                sql += flag == 0 ? " and GradeNo='{3}'" : " and ClassCode={3}";
                sql = string.Format(sql, micYear, testNo, courseCode, gradeOrClassNo);
                table = bll.FillDataTableByText(sql);
                s_num = int.Parse(table.Rows[0]["S_5"].ToString());

                //更新到数据库
                if (flag == 0)
                {
                    sql = " Update s_tb_GradeStat set S_100={4}"
                            + " where AcademicYear='{0}'"
                                       + " and TestNo='{1}'"
                                       + " and CourseCode='{2}'"
                                       + " and GradeNo='{3}'";
                }
                else
                    sql = "Update s_tb_ClassStat set S_100={4}"
                            + " where AcademicYear='{0}'"
                                       + " and TestNo='{1}'"
                                       + " and CourseCode='{2}'"
                                       + " and GradeNo='{3}'";
                sql = string.Format(sql, micYear, testNo, courseCode, gradeOrClassNo, s_num);
                bll.ExecuteNonQueryByText(sql);

                var LowScore = 0.05f;
                var highScore = 0.1f;
                for (int i = 0; i <= 17; i++)
                {
                    sql = " select count(*) as S_5 from s_vw_ClassScoreNum "
                             + " where NumScore/cast(substring(Markcode,2,3) as numeric(5,2) )>={4}"
                             + " and NumScore/cast(substring(Markcode,2,3) as numeric(5,2) )<{5}"
                             + " and substring(Markcode,1,1)='1'"
                             + " and AcademicYear='{0}'"
                             + " and TestNo='{1}'"
                             + " and CourseCode='{2}'"
                             + " and STATE is NULL ";
                    sql += flag == 0 ? " and GradeNo='{3}'" : " and ClassCode={3}";
                    sql = string.Format(sql, micYear, testNo, courseCode, gradeOrClassNo, LowScore, highScore);
                    table = bll.FillDataTableByText(sql);
                    s_num = int.Parse(table.Rows[0]["S_5"].ToString());

                    switch (i)
                    {
                        case 0:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_10={0}" : "Update s_tb_ClassStat Set S_10={0}";
                            break;
                        case 1:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_15={0}" : "Update s_tb_ClassStat Set S_15={0}";
                            break;
                        case 2:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_20={0}" : "Update s_tb_ClassStat Set S_20={0}";
                            break;
                        case 3:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_25={0}" : "Update s_tb_ClassStat Set S_25={0}";
                            break;
                        case 4:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_30={0}" : "Update s_tb_ClassStat Set S_30={0}";
                            break;
                        case 5:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_35={0}" : "Update s_tb_ClassStat Set S_35={0}";
                            break;
                        case 6:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_40={0}" : "Update s_tb_ClassStat Set S_40={0}";
                            break;
                        case 7:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_45={0}" : "Update s_tb_ClassStat Set S_45={0}";
                            break;
                        case 8:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_50={0}" : "Update s_tb_ClassStat Set S_50={0}";
                            break;
                        case 9:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_55={0}" : "Update s_tb_ClassStat Set S_55={0}";
                            break;
                        case 10:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_60={0}" : "Update s_tb_ClassStat Set S_60={0}";
                            break;
                        case 11:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_65={0}" : "Update s_tb_ClassStat Set S_65={0}";
                            break;
                        case 12:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_70={0}" : "Update s_tb_ClassStat Set S_70={0}";
                            break;
                        case 13:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_75={0}" : "Update s_tb_ClassStat Set S_75={0}";
                            break;
                        case 14:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_80={0}" : "Update s_tb_ClassStat Set S_80={0}";
                            break;
                        case 15:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_85={0}" : "Update s_tb_ClassStat Set S_85={0}";
                            break;
                        case 16:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_90={0}" : "Update s_tb_ClassStat Set S_90={0}";
                            break;
                        default:
                            sql = flag == 0 ? "Update s_tb_GradeStat Set S_95={0}" : "Update s_tb_ClassStat Set S_95={0}";
                            break;
                    }

                    sql += " Where AcademicYear='{1}'"
                           + " and TestNo='{2}'"
                           + " and CourseCode='{3}'";

                    sql += flag == 0 ? " and GradeNo='{4}'" : " and ClassCode={4}";
                    sql = string.Format(sql, s_num, micYear, testNo, courseCode, gradeOrClassNo);
                    bll.ExecuteNonQueryByText(sql);

                    LowScore += 0.05f;
                    highScore += 0.05f;
                }
            }
        }

        [WebMethod]
        public static IList<ResultEntry> AnalyzeSuper(
            int micYear,
            GradeCode gradeCode,
            IList<GradeClass> gradeClasses,
            IList<GradeCourse> gradeCourses,
            TestType testType,
            TestLogin testLogin,
            int outItem,
            int cksr,
            int cbDC,
            int setting,
            int valueA,
            int valueB,
            int valueC,
            int valueD,
            int valueE,
            string strlevel,
            string strlevel1
            )
        {
            IList<ResultEntry> results = new List<ResultEntry>();
            using (AppBLL bll = new AppBLL())
            {
                var classes = "";
                var courses = "";
                foreach (var gradeClass in gradeClasses)
                {
                    classes += string.Format("'{0}',", gradeClass.ClassNo);
                }
                classes = classes.Substring(0, classes.Length - 1);

                foreach (var gradeCourse in gradeCourses)
                {
                    courses += gradeCourse.CourseCode + ",";
                }
                courses = courses.Substring(0, courses.Length - 1);

                //clear old data
                var sql = "delete from s_tb_scorerep";
                bll.ExecuteNonQueryByText(sql);
                if (gradeCode.GradeNo == "33")
                {
                    sql = "Insert into s_tb_scorerep(academicyear,srid,stdname,classcode,classsn,testno,"
                               + " yw,sx,wy,zz,wl,hx,dl,ls,sw,jsj,ty,zzx)"
                               + " select AcademicYear,srid,stdName,gradename + '('+substring(classCode,3,2)+')班' classcode, classsn,testno,"
                               + " sum(case When CourseCode='21001' then numscore else 0 end) 'yw',"
                               + " sum(case When CourseCode='21002' then numscore else 0 end) 'sx',"
                               + " sum(case When CourseCode='21003' then numscore else 0 end) 'wy',"
                               + " sum(case When CourseCode='21004' then numscore else 0 end) 'zz',"
                               + " sum(case When CourseCode='21005' then numscore else 0 end) 'wl',"
                               + " sum(case When CourseCode='21006' then numscore else 0 end) 'hx',"
                               + " sum(case When CourseCode='21007' then numscore else 0 end) 'dl',"
                               + " sum(case When CourseCode='21008' then numscore else 0 end) 'ls',"
                               + " sum(case When CourseCode='21009' then numscore else 0 end) 'sw',"
                               + " sum(case When CourseCode='21010' then numscore else 0 end) 'jsj',"
                               + " sum(case When CourseCode='21013' then numscore else 0 end) 'ty',"
                               + " sum(case When CourseCode='31017' then numscore else 0 end) 'ZZX'"
                               + " from s_vw_classScoreNum "
                               + " where Academicyear=@micYear"
                               + " and testno=@testNo"
                               + " and classCode in ({0})";
                    sql = string.Format(sql, classes);
                }
                else
                {
                    sql = "Insert into s_tb_scorerep(academicyear,srid,stdname,classcode,classsn,testno,"
                                                  + " yw,sx,wy,zz,wl,hx,dl,ls,sw,jsj,yy,ms,ty) "
                                                  + " select AcademicYear,srid,stdName,gradename+'('+substring(classCode,3,2)+')班' classcode, classsn,testno,"
                                                  + " sum(case When CourseCode='21001' then numscore else 0 end) 'yw',"
                                                  + " sum(case When CourseCode='21002' then numscore else 0 end) 'sx',"
                                                  + " sum(case When CourseCode='21003' then numscore else 0 end) 'wy',"
                                                  + " sum(case When CourseCode='21004' then numscore else 0 end) 'zz',"
                                                  + " sum(case When CourseCode='21005' then numscore else 0 end) 'wl',"
                                                  + " sum(case When CourseCode='21006' then numscore else 0 end) 'hx',"
                                                  + " sum(case When CourseCode='21007' then numscore else 0 end) 'dl',"
                                                  + " sum(case When CourseCode='21008' then numscore else 0 end) 'ls',"
                                                  + " sum(case When CourseCode='21009' then numscore else 0 end) 'sw',"
                                                  + " sum(case When CourseCode='21010' then numscore else 0 end) 'jsj',"
                                                  + " sum(case When CourseCode='21011' then numscore else 0 end) 'yy',"
                                                  + " sum(case When CourseCode='21012' then numscore else 0 end) 'ms',"
                                                  + " sum(case When CourseCode='21013' then numscore else 0 end) 'ty' "
                                                  + " from s_vw_classScoreNum "
                                                  + " where Academicyear=@micYear"
                                                  + " and testno=@testNo"
                                                  + " and classCode in ({0})";
                    sql = string.Format(sql, classes);
                }

                if (cksr == 1) sql = sql + " and State is null";
                sql += " group by AcademicYear,srid,stdName,gradename,classcode, classsn,testno";
                bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo });

                var length = gradeCourses.Count();
                for (int i = 0; i < length; i++)
                {
                    string str_kc = "";
                    GradeCourse gradeCourse = gradeCourses[i];
                    if (int.Parse(gradeCourse.CourseCode) > 21009 && int.Parse(gradeCourse.CourseCode) != 31017) continue;
                    switch (gradeCourse.CourseCode)
                    {
                        case "21001": str_kc = "yw"; break;
                        case "21002": str_kc = "sx"; break;
                        case "21003": str_kc = "wy"; break;
                        case "21004": str_kc = "zz"; break;
                        case "21005": str_kc = "wl"; break;
                        case "21006": str_kc = "hx"; break;
                        case "21007": str_kc = "dl"; break;
                        case "21008": str_kc = "ls"; break;
                        case "21009": str_kc = "sw"; break;
                        case "31017": str_kc = "zzx"; break;
                        default: break;
                    }
                    sql = "select academicYear,'1' Semester,'1' TestType,TestNo,{0} CourseCode,srid,"
                                                + "{1},0 as OrderNO from s_tb_scorerep "
                                                + " where Academicyear={2}"
                                                + " and testno={3} and {1}>0";
                    sql = string.Format(sql, gradeCourse.CourseCode, str_kc, micYear, testLogin.TestLoginNo);
                    gf_ScoreOrderA(sql, gradeCourse.CourseCode, "s_tb_scorerep", str_kc + "M", 9);
                    gf_GetStdScoreB(micYear, testLogin.TestLoginNo.ToString(), gradeCourse.CourseCode, classes);
                    gp_GetTScoreB(micYear, testLogin.TestLoginNo.ToString(), gradeCourse.CourseCode, classes);

                    if (setting == 1)
                    {
                        sql = "Select count(*) as RS from s_vw_ClassScoreNum"
                                                   + " where Academicyear=@micYear"
                                                   + " and testno=@testNo"
                                                   + " and classcode in ({0})"
                                                   + " and CourseCode=@courseCode";
                        if (cksr == 0) sql += " and state is null";
                        sql = string.Format(sql, classes);
                        DataTable table = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode });
                        var renshu = int.Parse(table.Rows[0]["RS"].ToString());

                        var tempSql = " UPDATE s_tb_normalscore SET levelscore = '{0}'"
                                                    + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                                    + " ON a.SRID = b.SRID"
                                                    + " and a.Academicyear= b.Academicyear"
                                                    + " and a.TestNo=b.testno"
                                                    + " and a.coursecode=b.coursecode"
                                                    + " where a.classcode in ({1})"
                                                    + " and b.Academicyear=@micYear"
                                                    + " and b.TestNo=@testNo"
                                                    + " and b.CourseCode=@courseCode";

                        sql = tempSql + " and b.GradeOrder<=@iNumA";
                        sql = string.Format(sql, "A", classes);
                        var iNumA = renshu * valueA / 100;
                        var iNumB = -1;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA });

                        sql = tempSql + " and b.GradeOrder<=@iNumB and b.GradeOrder>@iNumA";
                        sql = string.Format(sql, "B", classes);
                        iNumA = renshu * valueA / 100;
                        iNumB = renshu * valueB / 100;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        sql = tempSql + " and b.GradeOrder<=@iNumB and b.GradeOrder>@iNumA";
                        sql = string.Format(sql, "C", classes);
                        iNumA = renshu * valueB / 100;
                        iNumB = renshu * valueC / 100;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        sql = tempSql + " and b.GradeOrder<=@iNumB and b.GradeOrder>@iNumA";
                        sql = string.Format(sql, "D", classes);
                        iNumA = renshu * valueC / 100;
                        iNumB = renshu * valueD / 100;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        sql = tempSql + " and b.GradeOrder<=@iNumB and b.GradeOrder>@iNumA";
                        sql = string.Format(sql, "E", classes);
                        iNumA = renshu * valueD / 100;
                        iNumB = renshu;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        if (cbDC == 1)
                        {
                            sql = " UPDATE s_tb_normalscore"
                                   + "  SET levelscore = '{0}'"
                                   + "  FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                   + "  ON a.SRID = b.SRID "
                                   + "  and a.Academicyear= b.Academicyear "
                                   + "  and a.TestNo=b.testno "
                                   + "  and a.coursecode=b.coursecode "
                                   + "  where a.classcode in ({2})"
                                   + "  and b.Academicyear=@micYear"
                                   + "  and b.TestNo=@testNo"
                                   + "  and b.CourseCode=@courseCode"
                                   + "  and b.NumScore>= cast(right(b.markcode,3) as int)*0.6"
                                   + "  and b.LevelScore='{1}'";
                            sql = string.Format(sql, strlevel, strlevel1, classes);
                            bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode });
                        }
                    }
                    else
                    {
                        var tempSql = "UPDATE s_tb_normalscore SET levelscore = '{0}'"
                                                     + "  FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                                     + "  ON a.SRID = b.SRID"
                                                     + "  and a.Academicyear= b.Academicyear"
                                                     + "  and a.TestNo=b.testno"
                                                     + "  and a.coursecode=b.coursecode"
                                                     + "  where a.classcode in ({1})"
                                                     + "  and b.Academicyear=@micYear"
                                                     + "  and b.TestNo=@testNo"
                                                     + "  and b.CourseCode=@courseCode";

                        sql = tempSql + " and b.NumScore>=@iNumA";
                        sql = string.Format(sql, "A", classes);
                        var iNumA = valueA;
                        var iNumB = -1;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA });

                        sql = tempSql + " and b.NumScore>=@iNumB and b.NumScore<@iNumA";
                        sql = string.Format(sql, "B", classes);
                        iNumA = valueA;
                        iNumB = valueB;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        sql = tempSql + " and b.NumScore>=@iNumB and b.NumScore<@iNumA";
                        sql = string.Format(sql, "C", classes);
                        iNumA = valueB;
                        iNumB = valueC;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        sql = tempSql + " and b.NumScore>=@iNumB and b.NumScore<@iNumA";
                        sql = string.Format(sql, "D", classes);
                        iNumA = valueC;
                        iNumB = valueD;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA, iNumB = iNumB });

                        sql = tempSql + " and b.NumScore>=0 and b.NumScore<@iNumA";
                        sql = string.Format(sql, "E", classes);
                        iNumA = valueD;
                        bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode, iNumA = iNumA });
                    }
                }

                //三总排名
                sql = " select academicYear,'1' Semester,'1' TestType,TestNo,'30000' CourseCode,srid,"
                        + "yw+sx+wy as Score,0 as OrderNO from s_tb_scorerep"
                        + " where Academicyear={0}"
                        + " and testno={1}";
                sql = string.Format(sql, micYear, testLogin.TestLoginNo);
                gf_ScoreOrderA(sql, "", "s_tb_scorerep", "YSWM", 0);

                //六总排名
                sql = "select academicYear,'1' Semester,'1' TestType,TestNo,'30000' CourseCode,srid,"
                         + "yw+sx+wy+zz+wl+hx as Score,0 as OrderNO from s_tb_scorerep"
                         + " where Academicyear={0}"
                         + " and testno={1}";
                sql = string.Format(sql, micYear, testLogin.TestLoginNo);
                gf_ScoreOrderA(sql, "", "s_tb_scorerep", "lzm", 0);

                //总分排名
                sql = "select academicYear,'1' Semester,'1' TestType,TestNo,'30000' CourseCode,srid,"
                        + "yw+sx+wy+zz+wl+hx+ls+dl+sw as Score,0 as OrderNO from s_tb_scorerep "
                        + " where Academicyear={0}"
                        + " and testno={1}";

                sql = string.Format(sql, micYear, testLogin.TestLoginNo);
                gf_ScoreOrderA(sql, "", "s_tb_scorerep", "bzm", 0);

                //获得学校编号
                sql = "update A set a.SchoolID=b.SchoolID"
                       + " from s_tb_scorerep as A,s_tb_schoolID as B"
                       + " where A.SRID = B.SRID";
                bll.ExecuteNonQueryByText(sql);

                //总分完成
                sql = "update s_tb_scorerep set "
                    + " ysw=yw+sx+wy,"
                    + " lz=yw+sx+wy+zz+wl+hx,"
                    + " bz=yw+sx+wy+zz+wl+hx+dl+ls+sw";
                bll.ExecuteNonQueryByText(sql);

                sql = "update s_tb_scorerep set wl=null where wl=0 ";
                bll.ExecuteNonQueryByText(sql);

                sql = "update s_tb_scorerep set hx=null where hx=0 ";
                bll.ExecuteNonQueryByText(sql);

                sql = "update s_tb_scorerep set sw=null where sw=0 ";
                bll.ExecuteNonQueryByText(sql);

                sql = "update s_tb_scorerep set zzx=null where zzx=0 ";
                bll.ExecuteNonQueryByText(sql);

                if (outItem == 1)
                {
                    sql = " Select Academicyear,testno,classcode,ClassSN,SchoolID,stdName,ysw,yswm,lz,lzm,bz,bzm ";
                    var length1 = gradeCourses.Count();
                    for (int i = 0; i < length1; i++)
                    {
                        string str_kc = "";
                        GradeCourse gradeCourse = gradeCourses[i];
                        switch (gradeCourse.CourseCode)
                        {
                            case "21001": str_kc = "yw"; break;
                            case "21002": str_kc = "sx"; break;
                            case "21003": str_kc = "wy"; break;
                            case "21004": str_kc = "zz"; break;
                            case "21005": str_kc = "wl"; break;
                            case "21006": str_kc = "hx"; break;
                            case "21007": str_kc = "dl"; break;
                            case "21008": str_kc = "ls"; break;
                            case "21009": str_kc = "sw"; break;
                            case "21010": str_kc = "jsj"; break;
                            case "21011": str_kc = "yy"; break;
                            case "21012": str_kc = "ms"; break;
                            case "21013": str_kc = "ty"; break;
                            case "31017": str_kc = "zzx"; break;
                            default: break;
                        }
                        if (str_kc == "jsj" || str_kc == "yy" || str_kc == "ms" || str_kc == "ty")
                        {
                            sql += "," + str_kc + " ";
                        }
                        else
                        {
                            sql += "," + str_kc + "," + str_kc + "m ";
                        }
                        mpClear(gradeCourse);
                    }
                    sql += " from s_tb_scorerep order by ClassCode,ClassSN ";
                    DataTable table = bll.FillDataTableByText(sql);
                    ResultEntry entry = new ResultEntry() { Code = 0, Message = Newtonsoft.Json.JsonConvert.SerializeObject(table) };
                    results.Add(entry);
                }
                else
                {
                    sql = " Select Academicyear,testno,classcode,ClassSN,SchoolID,stdName,ysw,yswm,lz,lzm,bz,bzm ";
                    var length1 = gradeCourses.Count();
                    for (int i = 0; i < length1; i++)
                    {
                        string str_kc = "";
                        GradeCourse gradeCourse = gradeCourses[i];
                        switch (gradeCourse.CourseCode)
                        {
                            case "21001": str_kc = "yw"; break;
                            case "21002": str_kc = "sx"; break;
                            case "21003": str_kc = "wy"; break;
                            case "21004": str_kc = "zz"; break;
                            case "21005": str_kc = "wl"; break;
                            case "21006": str_kc = "hx"; break;
                            case "21007": str_kc = "dl"; break;
                            case "21008": str_kc = "ls"; break;
                            case "21009": str_kc = "sw"; break;
                            default: break;
                        }
                        var tempSql = " update A set {0}= normalscore"
                                                   + " from s_tb_scorerep a,s_tb_normalscore b"
                                                   + " where a.Academicyear=b.Academicyear"
                                                   + " and a.testno=b.testno"
                                                   + " and a.srid=b.srid"
                                                   + " and b.coursecode='{1}'";
                        tempSql = string.Format(tempSql, str_kc, gradeCourse.CourseCode);

                        bll.ExecuteNonQueryByText(tempSql);
                        sql += "," + str_kc + "," + str_kc + "m ";
                        mpClear(gradeCourse);
                    }
                    sql += " from s_tb_scorerep order by ClassCode,ClassSN ";
                    DataTable table = bll.FillDataTableByText(sql);
                    ResultEntry entry = new ResultEntry() { Code = 0, Message = Newtonsoft.Json.JsonConvert.SerializeObject(table) };
                    results.Add(entry);
                }
            }
            return results;
        }

        private static void mpClear(GradeCourse gradeCourse)
        {
            string str_kc = "";
            if (int.Parse(gradeCourse.CourseCode) > 21009) return;
            switch (gradeCourse.CourseCode)
            {
                case "21001": str_kc = "yw"; break;
                case "21002": str_kc = "sx"; break;
                case "21003": str_kc = "wy"; break;
                case "21004": str_kc = "zz"; break;
                case "21005": str_kc = "wl"; break;
                case "21006": str_kc = "hx"; break;
                case "21007": str_kc = "dl"; break;
                case "21008": str_kc = "ls"; break;
                case "21009": str_kc = "sw"; break;
                default: break;
            }
            using (AppBLL bll = new AppBLL())
            {
                var sql = " update s_tb_scorerep set " + str_kc + "= null where " + str_kc + "=0";
                bll.ExecuteNonQueryByText(sql);

                sql = " update s_tb_scorerep set " + str_kc + "M = null where " + str_kc + "=0";
                bll.ExecuteNonQueryByText(sql);
            }
        }

        [WebMethod]
        public static string GetTestLoginByYear(int micYear)
        {
            DataTable table = App.Score.Db.UtilBLL.GetTestLoginByYear(micYear);
            return Newtonsoft.Json.JsonConvert.SerializeObject(table);
        }
        [WebMethod]
        public static GradeCode GetGradeByTestNo(int micYear, TestLogin testLogin)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = "Select * from s_tb_testLogin where Academicyear=@micYear and testno=@testNo";
                IList<GradeCode> grades = bll.FillListByText<GradeCode>(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo });
                return grades.Any() ? grades.First() : null;
            }
        }
        private static int gf_ScoreOrder(string OrderSQL, string WriteTableName, string FieldName, int WriteBack)
        {
            using (AppBLL bll = new AppBLL())
            {
                var TempTableName = App.Score.Db.UtilBLL.mf_getTable();
                try
                {
                    var sql = "create table " + TempTableName + "(academicYear char(4),Semester char(2),TestType char(1),TestNo char(5),CourseCode char(5),srid char(19),Score numeric(5,1),OrderNO integer)";
                    bll.ExecuteNonQueryByText(sql);

                    sql = "insert into " + TempTableName + "(academicYear,Semester,TestType,TestNo,CourseCode,srid,Score,OrderNO) " + OrderSQL;
                    bll.ExecuteNonQueryByText(sql);

                    ///开始排名
                    sql = "select distinct isnull(Score, 0) Score from " + TempTableName + " order by Score DESC";
                    DataTable table = bll.FillDataTableByText(sql);
                    var orderNo = 1;
                    var length = table.Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        sql = "update " + TempTableName + " set OrderNO={0} where score={1}";
                        sql = string.Format(sql, orderNo++, float.Parse(table.Rows[i]["Score"].ToString()));
                        bll.ExecuteNonQueryByText(sql);
                    }
                    if (WriteBack == 0)
                    {
                        //写回原表
                        sql = "UPDATE " + WriteTableName
                                    + " SET " + FieldName + " = a.OrderNo"
                                    + " FROM " + TempTableName + " as a INNER JOIN s_tb_normalscore as b "
                                    + " ON a.SRID = b.SRID "
                                    + " and a.Academicyear= b.Academicyear"
                                    + " and a.TestNo=b.testno"
                                    + " and a.coursecode=b.coursecode ";
                        bll.ExecuteNonQueryByText(sql);
                    }
                    return 0;
                }
                finally
                {
                    //删除临时表tempScore
                    var sql = "Drop table " + TempTableName;
                    bll.ExecuteNonQueryByText(sql);
                }
            }
        }
        private static void gf_GetStdScoreA(int micYear, string TestNo, string CourseCode, string GradeNo)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = " select avg(Numscore) as AvgScore from s_vw_ClassScoreNum"
               + " where Academicyear='{0}'"
               + " and TestNo='{1}'"
               + " and CourseCode='{2}'"
               + " and Gradeno='{3}'"
               + " and State is null"
               + " and Numscore<200"
               + " and Numscore is not Null ";
                DataTable table = bll.FillDataTableByText(string.Format(sql, micYear, TestNo, CourseCode, GradeNo));
                var avgScore = float.Parse(table.Rows[0]["AvgScore"].ToString());

                sql = " select stdevp(numscore) as stdScore from s_vw_ClassScoreNum"
                         + " where Academicyear='{0}'"
                         + " and TestNo='{1}'"
                         + " and CourseCode='{2}'"
                         + " and Gradeno='{3}'"
                         + " and state is null"
                         + " and Numscore<200"
                         + " and Numscore is not Null ";
                table = bll.FillDataTableByText(string.Format(sql, micYear, TestNo, CourseCode, GradeNo));
                var s = float.Parse(table.Rows[0]["stdScore"].ToString());
                //计算标准分
                if (s == 0)
                {
                    //方差为零，标准分为零
                    sql = " Update s_tb_Normalscore Set NormalScore = 0"
                           + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                           + " ON a.SRID = b.SRID"
                           + " and a.Academicyear= b.Academicyear"
                           + " and a.TestNo=b.testno"
                           + " and a.coursecode=b.coursecode"
                           + " where a.gradeno='{0}'"
                           + " and b.Academicyear='{1}'"
                           + " and b.TestNo='{2}'"
                           + " and b.CourseCode='{3}'"
                           + " and b.Numscore is not Null";
                    bll.ExecuteNonQueryByText(string.Format(sql, GradeNo, micYear, TestNo, CourseCode));
                }
                else
                {
                    sql = "Select * from s_tb_normalscore"
                            + " where Academicyear='{0}"
                            + " and TestNo='{1}'"
                            + " and CourseCode='{2}'"
                            + " and Numscore<200"
                            + " and State is null"
                            + " and Numscore is not Null";
                    table = bll.FillDataTableByText(string.Format(sql, micYear, TestNo, CourseCode));
                    var length = table.Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        string mScore = table.Rows[i]["NumScore"].ToString();
                        string mYear = table.Rows[i]["Academicyear"].ToString();
                        string mTestNo = table.Rows[i]["TestNo"].ToString();
                        string mCourseCode = table.Rows[i]["CourseCode"].ToString();
                        string mSRID = table.Rows[i]["SRID"].ToString();

                        var stdScore = (float.Parse(mScore) - avgScore) / s;
                        sql = " Update s_tb_Normalscore Set NormalScore ={4}"
                               + " where Academicyear='{0}'"
                               + " and TestNo ='{1}'"
                               + " and CourseCode='{2}'"
                               + " and SRID='{3}'";
                        bll.ExecuteNonQueryByText(string.Format(sql, mYear, mTestNo, mCourseCode, mSRID, stdScore));
                    }
                }

            }
        }

        private static void gf_GetStdScore(int Academicyear, string TestNo, string CourseCode, string GradeNo)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = "select avg(Numscore) as AvgScore from s_vw_ClassScoreNum"
                              + " where Academicyear='{0}'"
                              + " and TestNo='{1}'"
                              + " and CourseCode='{2}'"
                              + " and Gradeno='{3}'"
                              + " and Numscore<200"
                              + " and Numscore is not Null";
                DataTable table = bll.FillDataTableByText(string.Format(sql, Academicyear, TestNo, CourseCode, GradeNo));
                var avgScore = float.Parse(table.Rows[0]["AvgScore"].ToString());

                sql = "select stdevp(numscore) as stdScore from s_vw_ClassScoreNum"
                              + " where Academicyear='{0}'"
                              + " and TestNo='{1}'"
                              + " and CourseCode='{2}'"
                              + " and Gradeno='{3}'"
                              + " and Numscore<200"
                              + " and Numscore is not Null";
                table = bll.FillDataTableByText(string.Format(sql, Academicyear, TestNo, CourseCode, GradeNo));
                var s = float.Parse(table.Rows[0]["stdScore"].ToString());
                if (s == 0)
                {
                    sql = " Update s_tb_Normalscore Set NormalScore = 0"
                           + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                           + " ON a.SRID = b.SRID"
                           + " and a.Academicyear= b.Academicyear"
                           + " and a.TestNo=b.testno"
                           + " and a.coursecode=b.coursecode"
                           + " where a.gradeno='{1}'"
                           + " and b.Academicyear='{0}'"
                           + " and b.TestNo='{2}'"
                           + " and b.CourseCode='{3}'"
                           + " and b.Numscore is not Null";
                    bll.ExecuteNonQueryByText(string.Format(sql, Academicyear, GradeNo, TestNo, CourseCode));
                }
                else
                {
                    sql = "Select * from s_tb_normalscore "
                              + " where Academicyear='{0}'"
                              + " and TestNo='{1}'"
                              + " and CourseCode='{2}'"
                              + " and Numscore<200"
                              + " and Numscore is not Null";
                    table = bll.FillDataTableByText(string.Format(sql, Academicyear, TestNo, CourseCode));
                    var length = table.Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        string mScore = table.Rows[i]["NumScore"].ToString();
                        string mYear = table.Rows[i]["Academicyear"].ToString();
                        string mTestNo = table.Rows[i]["TestNo"].ToString();
                        string mCourseCode = table.Rows[i]["CourseCode"].ToString();
                        string mSRID = table.Rows[i]["SRID"].ToString();

                        var stdScore = (float.Parse(mScore) - avgScore) / s;
                        sql = " Update s_tb_Normalscore Set NormalScore ={4}"
                               + " where Academicyear='{0}'"
                               + " and TestNo ='{1}'"
                               + " and CourseCode='{2}'"
                               + " and SRID='{3}'";
                        bll.ExecuteNonQueryByText(string.Format(sql, mYear, mTestNo, mCourseCode, mSRID, stdScore));
                    }
                }
            }
        }
        private static void gp_GetTScoreA(int Academicyear, string TestNo, string CourseCode, string GradeNo)
        {
            using (AppBLL bll = new AppBLL())
            {
                if (string.IsNullOrEmpty(CourseCode) || CourseCode == "00000") return;
                //先计K
                var sql = "select max(normalscore) maxBZScore,min(normalscore) minBZScore"
                              + " from s_vw_ClassScoreNum"
                              + " where Academicyear='{0}'"
                              + " and TestNo='{1}'"
                              + " and CourseCode='{2}'"
                              + " and Gradeno='{3}'"
                              + " and State is null ";
                DataTable table = bll.FillDataTableByText(string.Format(sql, Academicyear, TestNo, CourseCode, GradeNo));
                if (table.Rows.Count == 0) return;
                var XT = float.Parse(table.Rows[0]["maxBZScore"].ToString());
                var YT = float.Parse(table.Rows[0]["minBZScore"].ToString());
                if (XT == 0 || YT == 0) return;

                int k = (int)Math.Floor(25 / XT);
                if (k > Math.Abs(Math.Floor(75 / YT))) k = (int)Math.Abs(Math.Floor(75 / YT));


                sql = " UPDATE b "
                      + " SET b.standardScore = (75+b.Normalscore*({0}))"
                      + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                      + " ON a.SRID = b.SRID"
                      + " and a.Academicyear= b.Academicyear"
                      + " and a.TestNo=b.testno"
                      + " and a.coursecode=b.coursecode"
                      + " where a.Academicyear=@micYear"
                      + " and a.testno=@testNo"
                      + " and a.GradeNo=@gradeNo"
                      + " and a.State is null"
                      + " and a.CourseCode=@courseCode";
                sql = string.Format(sql, k);
                bll.ExecuteNonQueryByText(sql, new { micYear = Academicyear, testNo = TestNo, courseCode = CourseCode, gradeNo = GradeNo });
            }
        }

        private static void gp_GetTScore(int Academicyear, string TestNo, string CourseCode, string GradeNo)
        {
            using (AppBLL bll = new AppBLL())
            {
                if (string.IsNullOrEmpty(CourseCode) || CourseCode == "00000") return;
                //先计K
                var sql = "select max(normalscore) maxBZScore,min(normalscore) minBZScore"
                              + " from s_vw_ClassScoreNum"
                              + " where Academicyear='{0}'"
                              + " and TestNo='{1}'"
                              + " and CourseCode='{2}'"
                              + " and Gradeno='{3}'"
                              + " and State is null ";
                DataTable table = bll.FillDataTableByText(string.Format(sql, Academicyear, TestNo, CourseCode, GradeNo));
                if (table.Rows.Count == 0) return;
                var XT = float.Parse(table.Rows[0]["maxBZScore"].ToString());
                var YT = float.Parse(table.Rows[0]["minBZScore"].ToString());
                if (XT == 0 || YT == 0) return;

                int k = (int)Math.Floor(25 / XT);
                if (k > Math.Abs(Math.Floor(75 / YT))) k = (int)Math.Abs(Math.Floor(75 / YT));


                sql = " UPDATE b "
                      + " SET b.standardScore = (75+b.Normalscore*({0}))"
                      + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                      + " ON a.SRID = b.SRID"
                      + " and a.Academicyear= b.Academicyear"
                      + " and a.TestNo=b.testno"
                      + " and a.coursecode=b.coursecode"
                      + " where a.Academicyear=@micYear"
                      + " and a.testno=@testNo"
                      + " and a.GradeNo=@gradeNo"
                      + " and a.CourseCode=@courseCode";
                sql = string.Format(sql, k);
                bll.ExecuteNonQueryByText(sql, new { micYear = Academicyear, testNo = TestNo, courseCode = CourseCode, gradeNo = GradeNo });
            }
        }

        [WebMethod]
        public static IList<ResultEntry> AnalyzeCommon(int micYear, GradeCode gradeCode, TestLogin testLogin, IList<GradeCourse> courses, int ckSR, int cbDC, string strLevel, string strLevel1)
        {
            using (AppBLL bll = new AppBLL())
            {
                string mGradeNo1 = gradeCode == null ? null : gradeCode.GradeNo;
                IList<ResultEntry> results = new List<ResultEntry>();
                ResultEntry entry = null;

                var sql = "Select * from s_tb_levelStd where LevelNo = 'A'";
                DataTable table = bll.FillDataTableByText(sql);
                var bLevel = !string.IsNullOrEmpty(table.Rows[0]["Level_bl"].ToString());

                sql = "select * from s_tb_levelStd";
                table = bll.FillDataTableByText(sql);
                if (table.Rows.Count == 0)
                {
                    entry = new ResultEntry() { Code = -1, Message = "无数据" };
                    results.Add(entry);
                    return results;
                }
                var floatA = bLevel ? float.Parse(table.Rows[0]["Level_bl"].ToString()) : float.Parse(table.Rows[0]["Level_Score"].ToString());
                var floatB = bLevel ? float.Parse(table.Rows[1]["Level_bl"].ToString()) : float.Parse(table.Rows[1]["Level_Score"].ToString());
                var floatC = bLevel ? float.Parse(table.Rows[2]["Level_bl"].ToString()) : float.Parse(table.Rows[2]["Level_Score"].ToString());
                var floatD = bLevel ? float.Parse(table.Rows[3]["Level_bl"].ToString()) : float.Parse(table.Rows[3]["Level_Score"].ToString());

                foreach (var course in courses)
                {
                    if (!string.IsNullOrEmpty(mGradeNo1) && mGradeNo1 == "00")
                    {
                        sql = "select * from tdGradeCode";
                        table = bll.FillDataTableByText(sql);
                        var length = table.Rows.Count;
                        for (int i = 0; i < length; i++)
                        {
                            var mGradeNo = table.Rows[i]["GradeNo"].ToString();
                            var mGradeName = table.Rows[i]["GradeBriefName"].ToString();
                            sql = string.Format("Select count(*) AS p from s_vw_ClassScoreNum where Gradeno='{0}'", mGradeNo);
                            DataTable tempTable = bll.FillDataTableByText(sql);
                            var pp = int.Parse(tempTable.Rows[0][0].ToString());
                            if (pp > 0)
                            {
                                if (ckSR == 1)
                                {
                                    gp_ScoreTjA(micYear, "", testLogin.TestType.ToString(), testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo, 0);
                                }
                                else
                                {
                                    App.Score.Db.UtilBLL.gp_ScoreTj(micYear, "", testLogin.TestType.ToString(), testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo, 0);
                                }
                                //开始进行年级排名
                                sql = " select academicYear,Semester,TestType,TestNo,CourseCode,srid,"
                                   + " Numscore,0 as OrderNO from s_vw_ClassScoreNum "
                                   + " where Academicyear={0}"
                                   + " and testno={1}"
                                   + " and GradeNo={2}"
                                   + " and CourseCode={3}";
                                if (ckSR == 1) sql += " and STATE is null";
                                sql = string.Format(sql, micYear, testLogin.TestLoginNo, mGradeNo, course.CourseCode);
                                gf_ScoreOrder(sql, "s_tb_normalscore", "GradeOrder", 0);
                                //11-11修改
                                gf_ScoreOrder(sql, "s_tb_normalscore", "GradeOrder", 0);
                                //开始计算标准分
                                //11-11修改
                                if (ckSR == 1)
                                    gf_GetStdScoreA(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo);
                                else
                                    gf_GetStdScore(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo);
                                //计算T分
                                //11-11修改
                                if (ckSR == 1)
                                    gp_GetTScoreA(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo);
                                else
                                    gp_GetTScore(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo);
                                //开始计算等第分

                                if (bLevel)
                                {
                                    sql = " Select count(*) as RS from s_vw_ClassScoreNum"
                                             + " where Academicyear=@micYear"
                                             + " and testno=@testNo"
                                             + " and GradeNo=@gradeNo"
                                             + " and CourseCode=@courseCode";
                                    if (ckSR == 1) sql += " and state is null";
                                    tempTable = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, gradeNo = mGradeNo, courseCode = course.CourseCode });
                                    var renshu = int.Parse(tempTable.Rows[0]["RS"].ToString());

                                    sql = "UPDATE s_tb_normalscore "
                                              + " SET levelscore = 'A'"
                                              + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                              + " ON a.SRID = b.SRID"
                                              + " and a.Academicyear= b.Academicyear"
                                              + " and a.TestNo=b.testno"
                                              + " and a.coursecode=b.coursecode"
                                              + " where a.gradeno=@gradeNo"
                                              + " and b.Academicyear=@micYear"
                                              + " and b.TestNo=@testNo"
                                              + " and b.CourseCode=@courseCode"
                                              + " and b.GradeOrder<=@iNum";
                                    bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, gradeNo = mGradeNo, courseCode = course.CourseCode, iNum = renshu * floatA });

                                    var tempSql = "update s_tb_Normalscore set LevelScore='{0}'"
                                               + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                               + " ON a.SRID = b.SRID"
                                               + " and a.Academicyear= b.Academicyear"
                                               + " and a.TestNo=b.testno"
                                               + " and a.coursecode=b.coursecode"
                                               + " where a.gradeno=@gradeNo"
                                               + " and b.Academicyear=@micYear"
                                               + " and b.TestNo=@testNo"
                                               + " and b.CourseCode=@courseCode"
                                               + " and b.GradeOrder<=@iNumB"
                                               + " and b.GradeOrder>@iNumA";
                                    sql = string.Format(tempSql, "B");
                                    var iNumA = renshu * floatA;
                                    var iNumB = renshu * floatB;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });

                                    sql = string.Format(tempSql, "C");
                                    iNumA = renshu * floatB;
                                    iNumB = renshu * floatC;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });

                                    sql = string.Format(tempSql, "D");
                                    iNumA = renshu * floatC;
                                    iNumB = renshu * floatD;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });

                                    sql = string.Format(tempSql, "E");
                                    iNumA = renshu * floatD;
                                    iNumB = renshu;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });

                                    if (cbDC == 1)
                                    {
                                        sql = "update s_tb_Normalscore set LevelScore='" + strLevel + "'"
                                                   + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b "
                                                   + " ON a.SRID = b.SRID"
                                                   + " and a.Academicyear= b.Academicyear "
                                                   + " and a.TestNo=b.testno"
                                                   + " and a.coursecode=b.coursecode"
                                                   + " where a.gradeno=@gradeNo"
                                                   + " and b.Academicyear=@micYear"
                                                   + " and b.TestNo=@testNo"
                                                   + " and b.CourseCode=@courseCode"
                                                   + " and b.NumScore>= cast(right(b.markcode,3) as int)*0.6"
                                                   + " and b.levelScore='" + strLevel1 + "'";
                                        bll.ExecuteNonQueryByText(sql, new
                                        {
                                            micYear = micYear,
                                            testNo = testLogin.TestLoginNo,
                                            gradeNo = mGradeNo,
                                            courseCode = course.CourseCode
                                        });
                                    }

                                }
                                else
                                {
                                    //根据分数
                                    sql = "UPDATE s_tb_normalscore "
                                            + " SET levelscore = 'A'"
                                            + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                            + " ON a.SRID = b.SRID"
                                            + " and a.Academicyear= b.Academicyear"
                                            + " and a.TestNo=b.testno"
                                            + " and a.coursecode=b.coursecode"
                                            + " where a.gradeno=@gradeNo"
                                            + " and b.Academicyear=@micYear"
                                            + " and b.TestNo=@testNo"
                                            + " and b.CourseCode=@courseCode"
                                            + " and b.GradeOrder<=@iNum";
                                    bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, gradeNo = mGradeNo, courseCode = course.CourseCode, iNum = floatA });

                                    var tempSql = "update s_tb_Normalscore set LevelScore='{0}'"
                                               + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                               + " ON a.SRID = b.SRID"
                                               + " and a.Academicyear= b.Academicyear"
                                               + " and a.TestNo=b.testno"
                                               + " and a.coursecode=b.coursecode"
                                               + " where a.gradeno=@gradeNo"
                                               + " and b.Academicyear=@micYear"
                                               + " and b.TestNo=@testNo"
                                               + " and b.CourseCode=@courseCode"
                                               + " and b.GradeOrder>=@iNumB"
                                               + " and b.GradeOrder<@iNumA";
                                    sql = string.Format(tempSql, "B");
                                    var iNumA = floatA;
                                    var iNumB = floatB;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });

                                    sql = string.Format(tempSql, "C");
                                    iNumA = floatB;
                                    iNumB = floatC;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });

                                    sql = string.Format(tempSql, "D");
                                    iNumA = floatC;
                                    iNumB = floatD;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA,
                                        iNumB = iNumB
                                    });
                                    tempSql = "update s_tb_Normalscore set LevelScore='{0}'"
                                               + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                               + " ON a.SRID = b.SRID"
                                               + " and a.Academicyear= b.Academicyear"
                                               + " and a.TestNo=b.testno"
                                               + " and a.coursecode=b.coursecode"
                                               + " where a.gradeno=@gradeNo"
                                               + " and b.Academicyear=@micYear"
                                               + " and b.TestNo=@testNo"
                                               + " and b.CourseCode=@courseCode"
                                               + " and b.NumScore>=0 "
                                               + " and b.NumScore<@iNumA ";
                                    sql = string.Format(tempSql, "E");
                                    iNumA = floatD;
                                    bll.ExecuteNonQueryByText(sql, new
                                    {
                                        micYear = micYear,
                                        testNo = testLogin.TestLoginNo,
                                        gradeNo = mGradeNo,
                                        courseCode = course.CourseCode,
                                        iNumA = iNumA
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        //***************
                        sql = "select * from tdGradeCode where GradeNo=@gradeNo";
                        DataTable tempTable = bll.FillDataTableByText(sql, new { gradeNo = mGradeNo1 });
                        if (ckSR == 1)
                        {
                            gp_ScoreTjA(micYear, "", testLogin.TestType.ToString(), testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo1, 0);
                        }
                        else
                        {
                            App.Score.Db.UtilBLL.gp_ScoreTj(micYear, "", testLogin.TestType.ToString(), testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo1, 0);
                        }

                        //开始进行年级排名
                        //11-11修改
                        sql = "select academicYear,Semester,TestType,TestNo,CourseCode,srid,"
                               + " Numscore,0 as OrderNO from s_vw_ClassScoreNum"
                               + " where Academicyear='{0}'"
                               + " and testno='{1}'"
                               + " and GradeNo='{2}'"
                               + " and CourseCode='{3}'";
                        sql = string.Format(sql, micYear, testLogin.TestLoginNo, mGradeNo1, course.CourseCode);
                        if (ckSR == 1) sql += " and STate is null ";

                        gf_ScoreOrder(sql, " s_tb_normalscore", "GradeOrder", 0);

                        //开始计算标准分
                        if (ckSR == 1)
                            gf_GetStdScore(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo1);
                        else
                            gf_GetStdScore(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo1);
                        //计算T分
                        if (ckSR == 1)
                            gp_GetTScoreA(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo1);
                        else
                            gp_GetTScore(micYear, testLogin.TestLoginNo.ToString(), course.CourseCode, mGradeNo1);

                        //开始计算等第分
                        if (bLevel)
                        {
                            //根据排名
                            sql = " Select count(*) as RS from s_vw_ClassScoreNum "
                                  + " where Academicyear=@micYear"
                                  + " and testno=@testNo"
                                  + " and GradeNo=@gradeNo"
                                  + " and CourseCode=@courseCode";
                            tempTable = bll.FillDataTableByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, gradeNo = mGradeNo1, courseCode = course.CourseCode });
                            var renshu = int.Parse(tempTable.Rows[0]["RS"].ToString());

                            sql = "UPDATE s_tb_normalscore "
                                            + " SET levelscore = 'A'"
                                            + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                            + " ON a.SRID = b.SRID"
                                            + " and a.Academicyear= b.Academicyear"
                                            + " and a.TestNo=b.testno"
                                            + " and a.coursecode=b.coursecode"
                                            + " where a.gradeno=@gradeNo"
                                            + " and b.Academicyear=@micYear"
                                            + " and b.TestNo=@testNo"
                                            + " and b.CourseCode=@courseCode"
                                            + " and b.GradeOrder<=@iNum";
                            bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, gradeNo = mGradeNo1, courseCode = course.CourseCode, iNum = renshu * floatA });

                            var tempSql = "update s_tb_Normalscore set LevelScore='{0}'"
                                       + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                       + " ON a.SRID = b.SRID"
                                       + " and a.Academicyear= b.Academicyear"
                                       + " and a.TestNo=b.testno"
                                       + " and a.coursecode=b.coursecode"
                                       + " where a.gradeno=@gradeNo"
                                       + " and b.Academicyear=@micYear"
                                       + " and b.TestNo=@testNo"
                                       + " and b.CourseCode=@courseCode"
                                       + " and b.GradeOrder<=@iNumB"
                                       + " and b.GradeOrder>@iNumA";
                            sql = string.Format(tempSql, "B");
                            var iNumA = renshu * floatA;
                            var iNumB = renshu * floatB;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });

                            sql = string.Format(tempSql, "C");
                            iNumA = renshu * floatB;
                            iNumB = renshu * floatC;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });

                            sql = string.Format(tempSql, "D");
                            iNumA = renshu * floatC;
                            iNumB = renshu * floatD;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });

                            sql = string.Format(tempSql, "E");
                            iNumA = renshu * floatD;
                            iNumB = renshu;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });

                            if (cbDC == 1)
                            {
                                sql = "update s_tb_Normalscore set LevelScore='" + strLevel + "'"
                                           + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b "
                                           + " ON a.SRID = b.SRID"
                                           + " and a.Academicyear= b.Academicyear "
                                           + " and a.TestNo=b.testno"
                                           + " and a.coursecode=b.coursecode"
                                           + " where a.gradeno=@gradeNo"
                                           + " and b.Academicyear=@micYear"
                                           + " and b.TestNo=@testNo"
                                           + " and b.CourseCode=@courseCode"
                                           + " and b.NumScore>= cast(right(b.markcode,3) as int)*0.6"
                                           + " and b.levelScore='" + strLevel1 + "'";
                                bll.ExecuteNonQueryByText(sql, new
                                {
                                    micYear = micYear,
                                    testNo = testLogin.TestLoginNo,
                                    gradeNo = mGradeNo1,
                                    courseCode = course.CourseCode
                                });
                            }
                        }
                        else
                        {
                            //根据分数
                            sql = "UPDATE s_tb_normalscore "
                                    + " SET levelscore = 'A'"
                                    + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                    + " ON a.SRID = b.SRID"
                                    + " and a.Academicyear= b.Academicyear"
                                    + " and a.TestNo=b.testno"
                                    + " and a.coursecode=b.coursecode"
                                    + " where a.gradeno=@gradeNo"
                                    + " and b.Academicyear=@micYear"
                                    + " and b.TestNo=@testNo"
                                    + " and b.CourseCode=@courseCode"
                                    + " and b.GradeOrder<=@iNum";
                            bll.ExecuteNonQueryByText(sql, new { micYear = micYear, testNo = testLogin.TestLoginNo, gradeNo = mGradeNo1, courseCode = course.CourseCode, iNum = floatA });

                            var tempSql = "update s_tb_Normalscore set LevelScore='{0}'"
                                       + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                       + " ON a.SRID = b.SRID"
                                       + " and a.Academicyear= b.Academicyear"
                                       + " and a.TestNo=b.testno"
                                       + " and a.coursecode=b.coursecode"
                                       + " where a.gradeno=@gradeNo"
                                       + " and b.Academicyear=@micYear"
                                       + " and b.TestNo=@testNo"
                                       + " and b.CourseCode=@courseCode"
                                       + " and b.GradeOrder>=@iNumB"
                                       + " and b.GradeOrder<@iNumA";
                            sql = string.Format(tempSql, "B");
                            var iNumA = floatA;
                            var iNumB = floatB;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });

                            sql = string.Format(tempSql, "C");
                            iNumA = floatB;
                            iNumB = floatC;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });

                            sql = string.Format(tempSql, "D");
                            iNumA = floatC;
                            iNumB = floatD;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA,
                                iNumB = iNumB
                            });
                            tempSql = "update s_tb_Normalscore set LevelScore='{0}'"
                                       + " FROM s_vw_ClassScoreNum as a INNER JOIN s_tb_normalscore as b"
                                       + " ON a.SRID = b.SRID"
                                       + " and a.Academicyear= b.Academicyear"
                                       + " and a.TestNo=b.testno"
                                       + " and a.coursecode=b.coursecode"
                                       + " where a.gradeno=@gradeNo"
                                       + " and b.Academicyear=@micYear"
                                       + " and b.TestNo=@testNo"
                                       + " and b.CourseCode=@courseCode"
                                       + " and b.NumScore>=0 "
                                       + " and b.NumScore<@iNumA ";
                            sql = string.Format(tempSql, "E");
                            iNumA = floatD;
                            bll.ExecuteNonQueryByText(sql, new
                            {
                                micYear = micYear,
                                testNo = testLogin.TestLoginNo,
                                gradeNo = mGradeNo1,
                                courseCode = course.CourseCode,
                                iNumA = iNumA
                            });
                        }
                    }
                }

                return results;
            }
        }

        [WebMethod]
        public static string GetMinutiaData(int micYear,
            GradeCode gradeCode,
            GradeCourse gradeCourse,
            TestLogin testLogin)
        {
            using (AppBLL bll = new AppBLL())
            {
                var sql = " select * from s_vw_minutiaScore"
                           + " where Academicyear=@micYear"
                           + " and TestNo=@testNo"
                           + " and CourseCode=@courseCode"
                           + " and substring(ClassCode,1,2)=@gradeNo"
                           + " order by Minutiacode,ClassSN";
                DataTable table = bll.FillDataTableByText(sql, new
                        {
                            micYear = micYear,
                            testNo = testLogin.TestLoginNo,
                            courseCode = gradeCourse.CourseCode,
                            gradeNo = gradeCode.GradeNo
                        });
                return Newtonsoft.Json.JsonConvert.SerializeObject(table);
            }
        }
        [WebMethod]
        public static IList<ChartOption> GetMinutiaCharts(
            int micYear,
            int chartType,
            GradeCode gradeCode,
            GradeClass gradeClass,
            Student student,
            GradeCourse gradeCourse,
            TestType testType,
            TestLogin testLogin)
        {
            using (AppBLL bll = new AppBLL())
            {
                IList<ChartOption> options = new List<ChartOption>();
                ChartOption option1 = new ChartOption() { legend = new Legend(), xAxis = new XAxis() };
                options.Add(option1);
                string title = chartType == 1 ? "得分" : "得分率";
                option1.legend.data.Add(string.Format("{0}{1}", student.StdName, title));
                option1.legend.data.Add(string.Format("年级{0}", student.StdName, title));
                option1.legend.data.Add(string.Format("班级{0}", student.StdName, title));

                var studentSeries = new SeriesItem() { type = "line", name = student.StdName };
                var gradeSeries = new SeriesItem() { type = "line", name = "年级总分平均" };
                var classSeries = new SeriesItem() { type = "line", name = "班级总分平均" };
                option1.series.Add(studentSeries);
                option1.series.Add(gradeSeries);
                option1.series.Add(classSeries);

                var sql = "";

                if (chartType == 1)
                {
                    sql = " SELECT a.AcademicYear,a.TestNo, a.MinutiaCode,"
                               + " c.GradeNo, d.MinutiaName,avg(a.MinutiaScore) GradeAvg"
                               + " FROM  s_tb_minutiaScore a INNER JOIN"
                               + " tbStudentClass b ON a.SRID = b.SRID AND"
                               + " a.AcademicYear = b.AcademicYear INNER JOIN"
                               + " tbGradeClass c ON b.AcademicYear = c.AcadEmicYear AND"
                               + " b.ClassCode = c.ClassNo INNER JOIN"
                               + " s_tb_minutia d ON a.MinutiaCode = d.MinutiaCode"
                               + " where a.academicyear=@micYear"
                               + " and c.GradeNo =@gradeNo"
                               + " and a.Testno=@testNo"
                               + " and a.CourseCode=@courseCode"
                               + " group by a.AcademicYear,a.TestNo, a.MinutiaCode,c.GradeNo,d.MinutiaName"
                               + " order by a.MinutiaCode";
                }
                else
                {
                    sql = " SELECT a.AcademicYear, a.TestNo, a.MinutiaCode, c.GradeNo,"
                              + " d.MinutiaName, AVG(a.MinutiaScore) AS GradeAvg, e.FullMark"
                              + " FROM  s_tb_minutiaScore a INNER JOIN"
                              + " tbStudentClass b ON a.SRID = b.SRID AND "
                              + " a.AcademicYear = b.AcademicYear INNER JOIN"
                              + " tbGradeClass c ON b.AcademicYear = c.AcadEmicYear AND"
                              + " b.ClassCode = c.ClassNo INNER JOIN"
                              + " s_tb_minutia d ON a.MinutiaCode = d.MinutiaCode INNER JOIN"
                              + " s_tb_minutiamark e ON a.AcademicYear = e.AcademicYear AND"
                              + " a.TestNo = e.TestNo AND a.MinutiaCode = e.MinutiaCode"
                              + " WHERE a.AcademicYear =@micYear"
                              + " and c.GradeNo =@gradeNo"
                              + " and a.Testno=@testNo"
                              + " and a.CourseCode=@courseCode"
                              + " group by a.AcademicYear,a.TestNo, a.MinutiaCode,c.GradeNo,d.MinutiaName,e.FullMark"
                              + " order by a.MinutiaCode";
                }
                DataTable table = bll.FillDataTableByText(sql, new { micYear = micYear, gradeNo = gradeCode.GradeNo, testNo = testLogin.TestLoginNo, courseCode = gradeCourse.CourseCode });
                if (table.Rows.Count == 0)
                {
                    return options;
                }
                DataTable tempTable = null;
                var length = table.Rows.Count;
                for (int i = 0; i < length; i++)
                {
                    if (chartType == 1)
                    {
                        gradeSeries.data.Add(table.Rows[0]["GradeAvg"].ToString());
                        sql = " SELECT a.MinutiaCode,d.MinutiaName,a.MinutiaScore "
                                 + " FROM  s_tb_minutiaScore a INNER JOIN"
                                 + " tbStudentClass b ON a.SRID = b.SRID AND"
                                 + " a.AcademicYear = b.AcademicYear INNER JOIN"
                                 + " tbGradeClass c ON b.AcademicYear = c.AcadEmicYear AND"
                                 + " b.ClassCode = c.ClassNo INNER JOIN"
                                 + " s_tb_minutia d ON a.MinutiaCode = d.MinutiaCode"
                                 + " where a.Academicyear=@micYear"
                                 + " and a.SRID = @srid"
                                 + " and a.testno=@testNo"
                                 + " and a.CourseCode=@courseCode"
                                 + " and a.MinutiaCode=@minutiaCode";
                        tempTable = bll.FillDataTableByText(sql, new
                        {
                            micYear = micYear,
                            srid = student.StudentId,
                            testNo = testLogin.TestLoginNo,
                            courseCode = gradeCourse.CourseCode,
                            minutiaCode = table.Rows[i]["MinutiaCode"].ToString()
                        });
                        if (tempTable.Rows.Count > 0)
                            studentSeries.data.Add(tempTable.Rows[0]["MinutiaScore"].ToString());

                        //教师
                        sql = " SELECT a.AcademicYear,a.TestNo, a.MinutiaCode,"
                               + " c.classNo, d.MinutiaName,avg(a.MinutiaScore) ClassAvg"
                               + " FROM  s_tb_minutiaScore a INNER JOIN"
                               + "  tbStudentClass b ON a.SRID = b.SRID AND"
                               + " a.AcademicYear = b.AcademicYear INNER JOIN"
                               + "  tbGradeClass c ON b.AcademicYear = c.AcadEmicYear AND"
                               + " b.ClassCode = c.ClassNo INNER JOIN"
                               + "  s_tb_minutia d ON a.MinutiaCode = d.MinutiaCode"
                               + " where a.academicyear=@micYear"
                               + " and c.classNo =@classCode"
                               + " and Testno=@testNo"
                               + " and a.CourseCode=@courseCode"
                               + " and a.MinutiaCode=@minutiaCode"
                               + " group by a.AcademicYear,a.TestNo, a.MinutiaCode,c.classNo,d.MinutiaName ";

                        tempTable = bll.FillDataTableByText(sql, new
                        {
                            micYear = micYear,
                            testNo = testLogin.TestLoginNo,
                            classCode = gradeClass.ClassNo,
                            courseCode = gradeCourse.CourseCode,
                            minutiaCode = table.Rows[i]["MinutiaCode"].ToString()
                        });
                        if (tempTable.Rows.Count > 0)
                            classSeries.data.Add(tempTable.Rows[0]["ClassAvg"].ToString());
                    }
                    else
                    {
                        var fullMark = float.Parse(table.Rows[i]["FullMark"].ToString());
                        var jj = float.Parse(table.Rows[i]["GradeAvg"].ToString()) / fullMark;
                        gradeSeries.data.Add(jj.ToString());

                        //加学生和班级
                        sql = " SELECT a.MinutiaCode,d.MinutiaName,a.MinutiaScore,e.FullMark"
                               + " FROM  s_tb_minutiaScore a INNER JOIN"
                               + " tbStudentClass b ON a.SRID = b.SRID AND "
                               + " a.AcademicYear = b.AcademicYear INNER JOIN"
                               + " tbGradeClass c ON b.AcademicYear = c.AcadEmicYear AND"
                               + " b.ClassCode = c.ClassNo INNER JOIN"
                               + " s_tb_minutia d ON a.MinutiaCode = d.MinutiaCode INNER JOIN"
                               + " s_tb_minutiamark e ON a.AcademicYear = e.AcademicYear AND"
                               + " a.TestNo = e.TestNo AND a.MinutiaCode = e.MinutiaCode"
                               + " where a.Academicyear=@micYear"
                               + " and a.SRID =@srid"
                               + " and a.testno=@testNo"
                               + " and a.CourseCode=@courseCode"
                               + " and a.MinutiaCode=@minutiaCode";
                        tempTable = bll.FillDataTableByText(sql, new
                        {
                            micYear = micYear,
                            srid = student.StudentId,
                            testNo = testLogin.TestLoginNo,
                            courseCode = gradeCourse.CourseCode,
                            minutiaCode = table.Rows[i]["MinutiaCode"].ToString()
                        });
                        if (tempTable.Rows.Count > 0)
                        {
                            jj = float.Parse(tempTable.Rows[i]["MinutiaScore"].ToString()) / fullMark;
                            studentSeries.data.Add(jj.ToString());
                        }
                        //教师
                        sql = " SELECT  a.AcademicYear, a.TestNo, a.MinutiaCode, c.classNo,"
                               + " d.MinutiaName, AVG(a.MinutiaScore) AS ClassAvg, e.FullMark"
                               + " FROM  s_tb_minutiaScore a INNER JOIN"
                               + " tbStudentClass b ON a.SRID = b.SRID AND "
                               + " a.AcademicYear = b.AcademicYear INNER JOIN"
                               + " tbGradeClass c ON b.AcademicYear = c.AcadEmicYear AND"
                               + " b.ClassCode = c.ClassNo INNER JOIN"
                               + " s_tb_minutia d ON a.MinutiaCode = d.MinutiaCode INNER JOIN"
                               + " s_tb_minutiamark e ON a.AcademicYear = e.AcademicYear AND"
                               + " a.TestNo = e.TestNo AND a.MinutiaCode = e.MinutiaCode"
                               + " WHERE a.AcademicYear =@micYear"
                               + " and c.classNo =@classCode"
                               + " and a.Testno=@testNo"
                               + " and a.CourseCode=@courseCode"
                               + " and a.MinutiaCode=@minutiaCode"
                               + " group by a.AcademicYear,a.TestNo, a.MinutiaCode,c.classNo,d.MinutiaName,e.FullMark ";
                        tempTable = bll.FillDataTableByText(sql, new
                        {
                            micYear = micYear,
                            testNo = testLogin.TestLoginNo,
                            classCode = gradeClass.ClassNo,
                            courseCode = gradeCourse.CourseCode,
                            minutiaCode = table.Rows[i]["MinutiaCode"].ToString()
                        });
                        if (tempTable.Rows.Count > 0)
                        {
                            jj = float.Parse(tempTable.Rows[i]["ClassAvg"].ToString()) / fullMark;
                            classSeries.data.Add(jj.ToString());
                        }
                    }
                }
                return options;
            }
        }
    }
}