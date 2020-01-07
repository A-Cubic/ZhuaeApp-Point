using Com.ACBC.Framework.Database;
using QuartzRedis.Buss;
using QuartzRedis.Common;
using System;

namespace QuartzRedis
{
    class Program
    {
        static void Main(string[] args)
        {
            if (DatabaseOperationWeb.TYPE == null)
            {
                DatabaseOperationWeb.TYPE = new DBManagerZE();
            }
            TaskJob.Worker();
            TaskJob.Subscribe();
            Console.ReadLine();
            //TaskJobBuss taskJobBuss = new TaskJobBuss();
            //taskJobBuss.updateUserInfo();
            //taskJobBuss.updateCommit();
            //taskJobBuss.getCommit();
        }
    }
}
