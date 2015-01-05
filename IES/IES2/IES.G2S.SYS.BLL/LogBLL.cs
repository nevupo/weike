﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IES.G2S.SYS.DAL;
using IES.SYS.Model;
using IES.G2S.SYS.IBLL;

namespace IES.G2S.SYS.BLL
{
    public class LogBLL : ILogBLL
    {

        #region 列表
        public List<Log> Log_List(Log model, int PageIndex, int PageSize)
        {
            return LogDAL.Log_List(model, PageIndex, PageSize);
        }
        #endregion
    }
}
