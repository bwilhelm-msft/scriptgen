namespace ScriptGen
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Antlr4.StringTemplate;

    using CommandLine;

    using CommandLineParser = CommandLine.Parser;

    internal class Program
    {
        private static void Main(string[] args)
        {
            CommandLineParser.Default.ParseArguments<Options>(args).WithParsed(ProcessOptions);
        }

        private static IEnumerable<ColumnDefinition> GetColumnDefinitions(Options options)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = options.Server,
                InitialCatalog = options.Database,
                UserID = options.User,
                Password = options.Password,
                Encrypt = true
            };

            using var connection = new SqlConnection(csb.ConnectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @$"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{options.Table}' AND TABLE_SCHEMA = '{options.Schema}' AND IS_NULLABLE != 'YES' ORDER BY ORDINAL_POSITION";

            var reader = command.ExecuteReader();

            var list = new List<ColumnDefinition>();

            while (reader.Read())
            {
                var cd = new ColumnDefinition
                {
                    Schema = reader.GetString("TABLE_SCHEMA"),
                    Table = reader.GetString("TABLE_NAME"),
                    Name = reader.GetString("COLUMN_NAME"),
                    DataType = reader.GetString("DATA_TYPE"),
                    Length = int.TryParse(reader.GetValue("CHARACTER_MAXIMUM_LENGTH")?.ToString(), out var length) ? length : 0
                };

                list.Add(cd);
            }

            return list;
        }

        private static void ProcessOptions(Options options)
        {
            var columns = GetColumnDefinitions(options).Select(CreateSampleData).ToList();

            var templateText = GetResourceText("ScriptGen.template.txt");
            Console.WriteLine(templateText);
            var group = new TemplateGroupString(templateText);
            var template = group.GetInstanceOf("CreateInsert");
            template.Add("schema", options.Schema);
            template.Add("table", options.Table);
            template.Add("columns", columns);
            Console.WriteLine(template.Render());
        }

        private static string GetResourceText(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static ColumnDefinition CreateSampleData(ColumnDefinition column)
        {
            if (column.DataType.Contains("varchar", StringComparison.OrdinalIgnoreCase))
            {
                var sample = $"{Guid.NewGuid().ToString().ToUpper()}-{GetAbbreviation(column.Table)}-{column.Name}";
                
                if (column.Length > 0)
                {
                    sample = sample.Substring(0, Math.Min(sample.Length, column.Length));
                }

                column.SampleData = $"'{sample}'";
            }
            else if (column.DataType.Contains("date", StringComparison.OrdinalIgnoreCase))
            {
                column.SampleData = $"'{DateTime.Now:s}'";
            }
            else if (column.DataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
            {
                var rand = new Random();
                column.SampleData = $"{rand.Next() % 2}";
            }
            else
            {
                var rand = new Random();
                column.SampleData = $"{rand.Next(0, 255)}";
            }

            return column;
        }

        private static string GetAbbreviation(string name)
        {
            return new string(name.Where(char.IsUpper).ToArray());
        }
    }
}
