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
        /// Presets, default values for new instances
        /// </summary>
        public static Dictionary<ConnectionStringPart, string> PartsDefault { get; set; } = new Dictionary<ConnectionStringPart, string>();

        /// <summary>
        /// Conditions for IsTestMode being set to true
        /// </summary>
        /// <example>Criteria "ConnectionStringPart.Server, test" -> IsTestMode becomes true e.g. if server name is "mysqltestserver"</example>
        public static List<(ConnectionStringPart part, string containedValue)> TestModeCriteria { get; set; } = new List<(ConnectionStringPart, string)>();        

        public ConnectionString() => Init();

        public ConnectionString(string raw, bool useEnvironmentVars = true) => Init(raw, useEnvironmentVars);

        public string UseEnvironmentVariablesPrefix { get; set; } = "DB_";


        /// <summary>
        /// Returns "mycomponentkey=myComponentValue"
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private string BuildRaw(ConnectionStringPart part)
        {
            return
                (part != ConnectionStringPart.Other ? part.ToString().ToLower() + "=" : "")
                    +
                (Parts.ContainsKey(part) ? Parts[part] : "");
        }

        /// <summary>
        /// Raw ConnectionString
        /// </summary>
        public string Result
        {
            get => string.Join(";", Parts.Select(p => BuildRaw(p.Key))).Replace(";;", ";");
            set => Init(value);
        }

        /// <summary>
        /// Password-safe db connection string
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <returns></returns>
        public string ResultSafe { get => Regex.Replace(Result.ToLower(), "password=.*?;", "password=********;"); }

        public void Init(string raw = null, bool useEnvironmentVariables = true)
        {
            //Set some sample criteria to distinguish between production and test mode
            if (TestModeCriteria.Count == 0)
            {
                TestModeCriteria.Add((ConnectionStringPart.Server, "test"));
                TestModeCriteria.Add((ConnectionStringPart.Server, "dev"));
                //TestModeCriteria.Add(ConnectionStringPart.Server, "localhost"); //does not always mean "test mode", e.g. production server and DB could be on same server
            }

            Parts = PartsDefault.Copy();

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
            get => TestModeCriteria.Any(c => Get(c.part)?.ToLower()?.Contains(c.containedValue.ToLower()) ?? false);
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
