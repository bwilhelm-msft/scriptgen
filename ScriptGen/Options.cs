using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGen
{
    internal class Options
    {
        [Option('i', Required = true, HelpText = "The SQL Server instance name")]
        public string Server { get; set; }

        [Option('d', Required = true, HelpText = "The database name")]
        public string Database { get; set; }

        [Option('u', Required = true, HelpText = "The user name")]
        public string User { get; set; }

        [Option('p', Required = true, HelpText = "The user password")]
        public string Password { get; set; }

        [Option('s', Required = true, HelpText = "The schema name")]
        public string Schema { get; set; }

        [Option('t', Required = true, HelpText = "The table name")]
        public string Table { get; set; }
    }
}
