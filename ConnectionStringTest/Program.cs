using DXSdata.ConnectionString;
using System;
using System.Diagnostics;

namespace ConnectionStringTest
{
    class Program
    {
        static void Main()
        {
            ConnectionString.PartsDefault.Add(ConnectionStringPart.Server, "mysqltestserver");

            var cs = new ConnectionString();
            Debug.Assert(cs.IsTestMode);

            cs.Database = "mydb";
            Console.WriteLine(cs.Result);

            cs.Password = "mypw";
            Console.WriteLine(cs.ResultSafe);

            cs.Other = "Use Compression=false;SslMode=none;";
            Console.WriteLine(cs.ResultSafe);

            cs = new ConnectionString("server=myserver");
            Debug.Assert(!cs.IsTestMode);


        }
    }
}
