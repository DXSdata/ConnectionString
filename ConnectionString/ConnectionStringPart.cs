using System;
using System.Collections.Generic;
using System.Text;

namespace DXSdata.ConnectionString
{
    /// <summary>
    /// Common connection string parts
    /// </summary>
    public enum ConnectionStringPart
    {
        Server,
        Port,
        Database,
        User,
        Password,

        /// <summary>
        /// Everything we don't know
        /// </summary>
        Other
    }
}
