namespace ScriptGen
{
    internal class ColumnDefinition
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public string SampleData { get; set; }
    }
}
