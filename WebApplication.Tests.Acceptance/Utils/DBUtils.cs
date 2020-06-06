using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace WebApplication.Tests.Acceptance.Utils
{
    public static class DBUtils
    {

        private static void ExecuteSqlScript(string scriptText)
        {
            if (string.IsNullOrEmpty(scriptText))
                return;

            var _sqlScriptSplitRegEx = new Regex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            var scripts = _sqlScriptSplitRegEx.Split(scriptText);

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WebApplication_ConnectionString"].ConnectionString))
            {
                conn.Open();

                using (var ts = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 10, 0)))
                {
                    foreach (var scriptLet in scripts)
                    {
                        if (scriptLet.Trim().Length == 0)
                            continue;

                        using (var cmd = new SqlCommand(scriptLet, conn))
                        {
                            //cmd.CommandTimeout = this.CommandTimeout;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ts.Complete();
                }
            }
        }

        public static void ExecutePreTestsScripts()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("\\bin\\Debug", "");
            string data = System.IO.File.ReadAllText(new Uri(path + "\\Data\\PRE_SCRIPTS_DB.sql").LocalPath);
            DBUtils.ExecuteSqlScript(data);
        }

        public static void ExecutePostTestsScripts()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("\\bin\\Debug", "");
            string data = System.IO.File.ReadAllText(new Uri(path + "\\Data\\POST_SCRIPTS_DB.sql").LocalPath);
            DBUtils.ExecuteSqlScript(data);
        }

    }

}
