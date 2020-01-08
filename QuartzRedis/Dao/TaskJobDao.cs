using Com.ACBC.Framework.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QuartzRedis.Dao
{
    public class TaskJobDao
    {
        public List<AddMemberInfoParam> getAddMemberInfoParam()
        {
            List<AddMemberInfoParam> list = new List<AddMemberInfoParam>();
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ShipSqls.SELECT_USER);
            string sql = builder.ToString();
            DataTable dt = DatabaseOperationWeb.ExecuteSelectDS(sql, "T").Tables[0];
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    AddMemberInfoParam param = new AddMemberInfoParam
                    {
                        phone = dr["CHAT_USER_ID"].ToString(),
                        point = dr["LAVEPOINT"].ToString(),
                        cardCode = dr["USER_ID"].ToString(),
                    };
                    list.Add(param);
                }
            }
            return list;
        }


        public List<UserPointParam> getChange()
        {
            List<UserPointParam> list = new List<UserPointParam>();
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ShipSqls.SELECT_CHANGE);
            string sql = builder.ToString();
            DataTable dt = DatabaseOperationWeb.ExecuteSelectDS(sql, "T").Tables[0];
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    UserPointParam param = new UserPointParam
                    {
                        phone = dr["CHAT_USER_ID"].ToString(),
                        point = dr["POINT"].ToString(),
                        cardCode = dr["USER_ID"].ToString(),
                        totalPoint = dr["TOTALPOINT"].ToString(),
                    };
                    list.Add(param);
                }
            }
            return list;
        }


        public int getMemberTotalScore(string phone)
        {
            int score = 0;
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ShipSqls.SELECT_POINT_FROM_USER_BY_ME_MOBILENUM, phone);
            string sql = builder.ToString();
            DataTable dt = DatabaseOperationWeb.ExecuteSelectDS(sql, "T").Tables[0];
            if (dt != null)
            {
                Int32.TryParse(dt.Rows[0][0].ToString(), out score);
            }
            return score;
        }
        public void updateUserPoint(string userId, int point, int newPoint, int oldPoint)
        {
            ArrayList list = new ArrayList();
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ShipSqls.UPDATE_USER_BALANCE_POINT, userId, point);
            string sql = builder.ToString();
            list.Add(sql);
            StringBuilder builder1 = new StringBuilder();
            builder1.AppendFormat(ShipSqls.UPDATE_USER_CHANGE_POINT, userId, newPoint);
            string sql1 = builder1.ToString();
            list.Add(sql1);
            if (DatabaseOperationWeb.ExecuteDML(list))
            {
                InsertPointLog(userId, oldPoint, newPoint, point);
            }
        }

        public string getUserIdByPhone(string phone)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ShipSqls.SELECT_USERID_BY_PHONE, phone);
            string sql = builder.ToString();
            DataTable dt = DatabaseOperationWeb.ExecuteSelectDS(sql, "T").Tables[0];
            if (dt != null)
            {
                return dt.Rows[0][0].ToString();
            }
            return "";
        }


        public void InsertPointLog(string userId, int oldPoint, int newPoint, int point)
        {
            ArrayList list = new ArrayList();
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ShipSqls.INSERT_POINTLOG, userId, oldPoint, newPoint, point);
            string sql = builder.ToString();
            DatabaseOperationWeb.ExecuteDML(sql);
        }

        public class ShipSqls
        {
            public const string SELECT_USER = "" +
                                      "SELECT F.LAVEPOINT,U.CHAT_USER_ID,U.USER_ID " +
                                      "FROM F_BALANCE F,U_USER U " +
                                      "WHERE F.USER_ID = U.USER_ID " +
                                      "AND U.IF_ADD_MEMBER = 0 " +
                                      "AND LENGTH(U.CHAT_USER_ID) = 11";
            public const string UPDATE_USER_IF_ADD = "" +
                                      "UPDATE U_USER " +
                                      "SET IF_ADD_MEMBER=1 " +
                                      "WHERE  USER_ID = {0} ";

            public const string SELECT_USERID_BY_PHONE = "" +
                                      "SELECT USER_ID " +
                                      "FROM U_USER " +
                                      "WHERE  CHAT_USER_ID = {0} ";
            public const string SELECT_CHANGE = "" +
                                      "SELECT F.LAVEPOINT-U.LAVEPOINT AS POINT ,U.CHAT_USER_ID ,U.USER_ID,F.LAVEPOINT AS TOTALPOINT  " +
                                      "FROM F_BALANCE F,U_USER U " +
                                      "WHERE F.USER_ID = U.USER_ID " +
                                      "AND U.IF_ADD_MEMBER = 1 " +
                                      "AND U.LAVEPOINT <> F.LAVEPOINT";
            public const string UPDATE_USER_CHANGE_POINT = "" +
                                      "UPDATE U_USER " +
                                      "SET LAVEPOINT={1} " +
                                      "WHERE  USER_ID = {0} ";
            public const string SELECT_POINT_FROM_USER_BY_ME_MOBILENUM = "" +
                                      "SELECT F.LAVEPOINT  " +
                                      "FROM F_BALANCE F,U_USER U " +
                                      "WHERE F.USER_ID = U.USER_ID " +
                                      "AND U.CHAT_USER_ID = '{0}'";
            public const string UPDATE_USER_BALANCE_POINT = "" +
                                      "UPDATE F_BALANCE " +
                                      "SET LAVEPOINT=LAVEPOINT-{1} " +
                                      "WHERE  USER_ID = {0} ";
            public const string INSERT_POINTLOG = "" +
                                      "INSERT INTO POINTLOG(USER_ID,OLD_POINT,NEW_POINT,POINT,CREATETIME) " +
                                      "VALUES({0},{1},{2},{3},NOW()) ";
        }
    }
}
