using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace jpdb_cli
{
    internal class stringFunctions
    {
        public static string removeBContents(string query) //remove everything inside of <>
        {
            var pattern = @"<(.*?)\>";

            string output = Regex.Replace(query, pattern, string.Empty);

            return output;
        }

    }
}
