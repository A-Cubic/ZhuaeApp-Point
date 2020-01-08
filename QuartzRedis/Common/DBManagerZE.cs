using System;
using System.Collections.Generic;
using System.Text;
using Com.ACBC.Framework.Database;

namespace QuartzRedis.Common
{
    class DBManagerZE : IType
    {
        private DBType dbt;
        private string str = "";

        public DBManagerZE()
        {
            this.str = "Server=" + Global.ZEDBUrl
                     + ";Port=" + Global.ZEDBPort
                     + ";Database=dolldb;Uid=" + Global.ZEDBUser
                     + ";Pwd=" + Global.ZEDBPassword
                     + ";CharSet=utf8mb4; SslMode =none;";
            this.dbt = DBType.Mysql;
        }

        public DBManagerZE(DBType d, string s)
        {
            this.dbt = d;
            this.str = s;
        }

        public DBType getDBType()
        {
            return dbt;
        }

        public string getConnString()
        {
            return str;
        }

        public void setConnString(string s)
        {
            this.str = s;
        }
    }
}
