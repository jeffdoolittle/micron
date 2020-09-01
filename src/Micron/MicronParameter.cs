namespace Micron
{
    using System.Collections.Generic;
    using System.Data;

    public class MicronParameter
    {
        public string Name { get; set; } = "";
        public MicronParameterDataType DataType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }

        public MicronParameter Clone() => new MicronParameter
        {
            Name = this.Name,
            DataType = this.DataType,
            Direction = this.Direction,
            IsNullable = this.IsNullable,
            Precision = this.Precision,
            Scale = this.Scale,
            Size = this.Size
        };

        internal IEnumerable<string> GetParameterValues()
        {
            yield return this.Name;
            yield return this.DataType.ToString();
            yield return this.Direction.ToString();
            yield return this.IsNullable.ToString();
            yield return this.Precision.ToString();
            yield return this.Scale.ToString();
            yield return this.Size.ToString();
        }
    }
}
