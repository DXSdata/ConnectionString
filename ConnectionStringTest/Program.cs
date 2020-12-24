using DXSdata.ConnectionString;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConnectionStringTest
{
    class Program
    {
        static void Main()
        {
            ConnectionString.PartsDefaultGlobal.Add(ConnectionStringPart.Server, "mysqltestserver");

            var instanceDefaultParts = new Dictionary<ConnectionStringPart, string>();
            instanceDefaultParts.Add(ConnectionStringPart.Database, "mydbname");

            var cs = new ConnectionString();


            cs = new ConnectionString(instanceDefaultParts);


            Debug.Assert(cs.IsTestMode);

            cs.Database = "mydb2";
            Console.WriteLine(cs.Result);

            cs.Password = "mypw";
            Console.WriteLine(cs.ResultSafe);

            cs.Other = "Use Compression=false;SslMode=none;";
            Console.WriteLine(cs.ResultSafe);

            cs = new ConnectionString(raw: "server=myserver");
            Debug.Assert(!cs.IsTestMode);


        }
    }
}
