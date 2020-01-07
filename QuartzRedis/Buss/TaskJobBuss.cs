using Com.ACBC.Framework.Database;
using Newtonsoft.Json;
using QuartzRedis.Common;
using QuartzRedis.Dao;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static QuartzRedis.Dao.SqlDao;

namespace QuartzRedis.Buss
{
    public class TaskJobBuss
    {
        public void doWork(string ids)
        {
            updateUserInfo();
            updateCommit();
            getCommit();
        }

        /// <summary>
        /// 上传用户信息
        /// </summary>
        public void updateUserInfo()
        {
            try
            {
                SqlDao sqlDao = new SqlDao();
                List<AddMemberInfoParam> paramList = sqlDao.getAddMemberInfoParam();
                if (paramList.Count == 0)
                {
                    return;
                }
                ArrayList list = new ArrayList();
                foreach (var param in paramList)
                {
                    string st = getRemoteParam(param, "AddMemberInfo", "5");
                    string result = HttpHandle.PostHttps(Global.PostUrl, st, "application/json");
                    ReturnItem ri = JsonConvert.DeserializeObject<ReturnItem>(result);
                    if (ri.success)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendFormat(ShipSqls.UPDATE_USER_IF_ADD, param.cardCode);
                        list.Add(builder.ToString());
                    }
                }
                DatabaseOperationWeb.ExecuteDML(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// 上传积分变动信息
        /// </summary>
        public void updateCommit()
        {
            try
            {
                SqlDao sqlDao = new SqlDao();
                ArrayList list = new ArrayList();
                //处理玩偶兑换积分
                List<UserPointParam> gList = sqlDao.getChange();
                foreach (UserPointParam aParam in gList)
                {
                    AddPointRecordParam param = new AddPointRecordParam
                    {
                        phone = aParam.phone,
                        point = aParam.point,
                    };
                    string st = getRemoteParam(param, "AddPointRecord", "5");
                    string result = HttpHandle.PostHttps(Global.PostUrl, st, "application/json");
                    ReturnItem ri = JsonConvert.DeserializeObject<ReturnItem>(result);
                    if (ri.success)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendFormat(ShipSqls.UPDATE_USER_CHANGE_POINT, aParam.cardCode, aParam.totalPoint);
                        list.Add(builder.ToString());
                    }
                }
                DatabaseOperationWeb.ExecuteDML(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        /// <summary>
        /// 获取积分变动信息
        /// </summary>
        public void getCommit()
        {
            try
            {
                SqlDao sqlDao = new SqlDao();
                string st = getRemoteParam(new Param(), "GetPointCommitList", "5");
                string result = HttpHandle.PostHttps(Global.PostUrl, st, "application/json");
                ReturnItem ri = JsonConvert.DeserializeObject<ReturnItem>(result);
                if (ri.success)
                {
                    if (ri.data != null)
                    {
                        for (int i = 0; i < ri.data.Count; i++)
                        {
                            if (ri.data[i].storeId == "5")
                            {
                                try
                                {
                                    string userId = sqlDao.getUserIdByPhone(ri.data[i].phone);
                                    if (userId!=null)
                                    {
                                        int rPoint = Convert.ToInt32(ri.data[i].point);
                                        int oldPoint = sqlDao.getMemberTotalScore(ri.data[i].phone);
                                        int newPoint = oldPoint - rPoint;
                                        if (newPoint < 0)
                                        {
                                            continue;
                                        }
                                        sqlDao.updateUserPoint(userId, rPoint, newPoint, oldPoint);
                                        UpdatePointCommitParam param = new UpdatePointCommitParam
                                        {
                                            pointCommitId = ri.data[i].pointCommitId,
                                        };
                                        string st1 = getRemoteParam(param, "UpdatePointCommit", "5");
                                        string result1 = HttpHandle.PostHttps(Global.PostUrl, st1, "application/json");
                                        ReturnItem ri1 = JsonConvert.DeserializeObject<ReturnItem>(result1);
                                        if (ri1.success)
                                        {
                                        }
                                        else
                                        {
                                            Console.WriteLine("UpdatePointCommit:" + result1);
                                        }
                                    }
                                    
                                }
                                catch (Exception)
                                {
                                    
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }




        private string getRemoteParam(Param param, string name, string code)
        {
            string appId = Global.AppId;
            string appSecret = Global.AppSecret;
            //string code = "1";
            string placeHold = Global.PlaceHold;
            string nonceStr = DateTime.Now.ToString("MMddHHmmss");
            string paramS = Regex.Replace(JsonConvert.SerializeObject(param), "\"(.+?)\"",
                 new MatchEvaluator(
                    (s) =>
                    {
                        return s.ToString().Replace(" ", placeHold);
                    }))
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace(" ", "")
                    .Replace(placeHold, " ");
            string needMd5 = appId + nonceStr + appSecret + paramS;
            string md5S = "";
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(needMd5));
                var strResult = BitConverter.ToString(result);
                md5S = strResult.Replace("-", "");
            }

            PostParam postParam = new PostParam
            {
                sign = md5S,
                code = code,
                nonceStr = nonceStr,
                method = name,
                appId = appId,
                param = param,
            };

            return JsonConvert.SerializeObject(postParam);
        }


    }
}
