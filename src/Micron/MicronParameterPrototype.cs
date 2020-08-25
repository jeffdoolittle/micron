namespace Micron
{
    using System.Data;

    public class MicronParameterPrototype
    {
        public string Name { get; set; } = "";
        public MicronParameterDataType DataType { get; set; } = MicronParameterDataType.String;
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
}
