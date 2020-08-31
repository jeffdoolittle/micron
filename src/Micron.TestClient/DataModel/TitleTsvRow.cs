namespace Micron.TestClient.DataModel
{
    public class TitleTsvRow
    {
        public string TitleId { get; set; } = "";
        public string? TitleType { get; set; }
        public string? PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }
        public string? IsAdult { get; set; }
        public string? StartYear { get; set; }
        public string? EndYear { get; set; }
        public string? RuntimeMinutes { get; set; }
        public string? GenresArray { get; set; }

        public static TitleTsvRow FromLine(string tsvLine)
        {
            var row = new TitleTsvRow();
            var parts = tsvLine.Split("\t");
            var p = 0;
            row.TitleId = parts[p++];
            row.TitleType = parts[p++];
            row.PrimaryTitle = parts[p++];
            row.OriginalTitle = parts[p++];
            row.IsAdult = parts[p++];
            row.StartYear = parts[p++];
            row.EndYear = parts[p++];
            row.RuntimeMinutes = parts[p++];
            row.GenresArray = parts[p++];
            return row;
        }

        public override string ToString() =>
            string.Join("\t",
                this.TitleId, this.TitleType, this.PrimaryTitle,
                this.OriginalTitle, this.IsAdult, this.StartYear,
                this.EndYear, this.RuntimeMinutes, this.GenresArray);
    }
}
