using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DXSdata.ConnectionString
{
    public static class ExtensionMethods
    {
        public static string FirstCharToUpper(this string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsLower(str[0]))
                str = char.ToUpper(str[0]) + str.Substring(1);
            return str;
        }

        /// <summary>
        /// Creates an independent copy of a class object, i.e. json-serializes, then deserializes it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceItem"></param>
        /// <returns></returns>
        public static T Copy<T>(this T sourceItem)
        {
            var serialized = JsonConvert.SerializeObject(sourceItem,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
