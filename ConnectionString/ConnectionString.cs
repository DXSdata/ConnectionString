using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DXSdata.ConnectionString
{
    /// <summary>
    /// Convenient DB connection string handling, specializing in MySQL / MariaDB
    /// </summary>
    public class ConnectionString
    {
        

        public Dictionary<ConnectionStringPart, string> Parts { get; set; }

        /// <summary>
        /// Presets, default values for new instances.
        /// Be careful if you are using e.g. multiple databases!
        /// In this case, for presets better use e.g. a static variable of this instance
        /// </summary>
        public static Dictionary<ConnectionStringPart, string> PartsDefaultGlobal { get; set; } = new Dictionary<ConnectionStringPart, string>();

        /// <summary>
        /// Conditions for IsTestMode being set to true
        /// </summary>
        /// <example>Criteria "ConnectionStringPart.Server, test" -> IsTestMode becomes true e.g. if server name is "mysqltestserver"</example>
        public static List<(ConnectionStringPart part, string containedValue)> TestModeCriteriaGlobal { get; set; } = new List<(ConnectionStringPart, string)>();        

        public ConnectionString() => Init();

        public ConnectionString(Dictionary<ConnectionStringPart, string> defaultParts = null, string raw = null, bool useEnvironmentVars = true) => Init(defaultParts, useEnvironmentVars, raw);

        public string UseEnvironmentVariablesPrefix { get; set; } = "DB_";


        /// <summary>
        /// Returns "mycomponentkey=myComponentValue"
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private string BuildRaw(ConnectionStringPart part, bool protectPw = false)
        {
            return
                (part != ConnectionStringPart.Other ? part.ToString().ToLower() + "=" : "")
                    +
                (Parts.ContainsKey(part) ? 
                    (protectPw && part == ConnectionStringPart.Password ? "********" : Parts[part] )
                    : "");
        }

        /// <summary>
        /// Raw ConnectionString
        /// </summary>
        public string Result
        {
            get => string.Join(";", Parts.Select(p => BuildRaw(p.Key))).Replace(";;", ";");
            set => Init(raw: value);
        }

        /// <summary>
        /// Password-safe db connection string
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <returns></returns>
        public string ResultSafe { get => string.Join(";", Parts.Select(p => BuildRaw(p.Key, true))).Replace(";;", ";"); }
        //public string ResultSafe { get => Regex.Replace(Result.ToLower(), "password=.*?;", "password=********;"); }

        /// <summary>
        /// Will be applied (overriden) in the parameter's order:
        /// Global defaults < instance defaults < environment vars < raw connection string
        /// </summary>
        /// <param name="defaultParts"></param>
        /// <param name="raw"></param>
        /// <param name="useEnvironmentVariables"></param>
        public void Init(Dictionary<ConnectionStringPart, string> defaultParts = null, bool useEnvironmentVariables = true, string raw = null)
        {
            //Set some sample criteria to distinguish between production and test mode
            if (TestModeCriteriaGlobal.Count == 0)
            {
                TestModeCriteriaGlobal.Add((ConnectionStringPart.Server, "test"));
                TestModeCriteriaGlobal.Add((ConnectionStringPart.Server, "dev"));
                //TestModeCriteria.Add(ConnectionStringPart.Server, "localhost"); //does not always mean "test mode", e.g. production server and DB could be on same server
            }


            Parts = PartsDefaultGlobal.Copy();


            if (defaultParts != null)
                foreach (var dp in defaultParts)
                    Set(dp.Key, dp.Value);


            if (useEnvironmentVariables)
                foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
                    if (envVar.Key.ToString().StartsWith(UseEnvironmentVariablesPrefix))
                        Set(envVar.Key.ToString().Replace(UseEnvironmentVariablesPrefix, ""), envVar.Value.ToString());


            var rawParts = raw?.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
            foreach (var rawPart in rawParts)
            {
                var split = rawPart.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var key = split[0]?.Trim()?.ToLower();
                var val = split[1]?.Trim();
                Set(key, val);
            }

        }



        public void Set(ConnectionStringPart part, string val)
        {
            if (Parts.ContainsKey(part))
                Parts[part] = val;
            else
                Parts.Add(part, val);
        }

        public void Set(string partStr, string val)
        {
            partStr = partStr.ToLower().Trim().FirstCharToUpper();
            if (Enum.TryParse<ConnectionStringPart>(partStr, out var part))
                Set(part, val);
            else
            {
                if (!Parts.ContainsKey(ConnectionStringPart.Other))
                    Parts.Add(ConnectionStringPart.Other, "");
                Parts[ConnectionStringPart.Other] += partStr.ToLower() + "=" + val + ";";
            }
        }

        public string Get(ConnectionStringPart part) => Parts.ContainsKey(part) ? Parts[part] : null;


        /// <summary>
        /// Checks connection string for "test" or "dev" tags to determine if currently in testing mode
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <returns></returns>
        public bool IsTestMode 
        {
            get => TestModeCriteriaGlobal.Any(c => Get(c.part)?.ToLower()?.Contains(c.containedValue.ToLower()) ?? false);
        }
                

        #region some shortcuts
        public string Server { get => Get(ConnectionStringPart.Server); set => Set(ConnectionStringPart.Server, value); }
        public string Port { get => Get(ConnectionStringPart.Port); set => Set(ConnectionStringPart.Port, value); }
        public string User { get => Get(ConnectionStringPart.User); set => Set(ConnectionStringPart.User, value); }
        public string Password { get => Get(ConnectionStringPart.Password); set => Set(ConnectionStringPart.Password, value); }
        public string Database { get => Get(ConnectionStringPart.Database); set => Set(ConnectionStringPart.Database, value); }
        public string Other { get => Get(ConnectionStringPart.Other); set => Set(ConnectionStringPart.Other, value); }

        #endregion

    }
}
