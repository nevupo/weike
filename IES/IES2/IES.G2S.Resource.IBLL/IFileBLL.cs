﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IES.Resource.Model;

namespace IES.G2S.Resource.IBLL
{

    public interface  IFileBLL
    {

        #region 文件夹


        #region  列表
        /// <summary>
        /// 文件夹列表
        /// </summary>
        /// <param name="model">文件夹对象</param>
        /// <returns></returns>
        List<Folder> Folder_List(Folder model);

        /// <summary>
        /// 文件夹树列表
        /// </summary>
        /// <param name="model">文件夹对象</param>
        /// <returns></returns>
        List<Folder> Folder_Tree(Folder model);

        /// <summary>
        /// 获取用户所有课程的文件夹
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<Folder> Folder_ALL_Tree(int userId);

        #endregion


        #region 详细信息

        #endregion


        #region  新增


        Folder Folder_ADD(Folder model);


        #endregion


        #region 对象更新

        bool Folder_ShareRange_Upd(Folder model);
        bool Folder_Batch_ShareRange(string folderIds, int shareRange);

        bool Folder_Upd(Folder model);

        #endregion


        #region 单个批量更新


        bool Folder_Move(Folder source, Folder Target);



        #endregion


        #region 属性批量操作

        /// <summary>
        /// 文件夹批量删除
        /// </summary>
        /// <param name="folderlist"></param>
        /// <returns></returns>
        bool Folder_Batch_Del(string FolderIDS);


        #endregion


        #region 删除

        bool Folder_Del(Folder model);
        bool File_Batch_Del(string FileIDS);
        #endregion


        #endregion 


        #region 文件

        #region  列表

        /// <summary>
        /// 文件查询
        /// </summary>
        /// <param name="model">文件对象</param>
        /// <param name="PageSize"></param>
        /// <param name="PageIndex"></param>
        /// <returns></returns>
        List<File> File_Search(File model, int PageSize, int PageIndex);

        #endregion


        #region 详细信息

        #endregion


        #region  新增

        File File_ADD(File model);

        #endregion

        #region 对象更新



        #endregion


        #region 单个批量更新


        bool File_ShareRange(File model);

        bool File_Attribute(File model, List<Chapter> chapterlist, List<Ken> kenlist);

        bool File_Keys(File model, List<Key> keylist);

        bool File_Move(File source, Folder Target);


        #endregion


        #region 属性批量操作

        bool File_Batch_ShareRange(string FileIDS, int ShareRange);

        #endregion

        #region 删除



        string File_Del(File model);


        #endregion

        #endregion

    }
}
