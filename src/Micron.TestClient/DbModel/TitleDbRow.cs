namespace Micron.TestClient.DbModel
{
    using System;
    using System.Collections.Generic;
    using Micron.TestClient.DataModel;

    public class TitleBasicsDbRow
    {
        public string TitleId { get; set; } = "";
        public string? TitleType { get; set; }
        public string? PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }
        public int IsAdult { get; set; } = 0;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }
        public string? GenresCsv { get; set; }

        public override string ToString() =>
            string.Join("\t",
                this.TitleId, this.TitleType, this.PrimaryTitle,
                this.OriginalTitle, this.IsAdult, this.StartYear,
                this.EndYear, this.RuntimeMinutes, this.GenresCsv);

        public static TitleBasicsDbRow From(TitleTsvRow tsvRow)
        {
            var dbRow = new TitleBasicsDbRow
            {
                TitleId = tsvRow.TitleId,
                TitleType = tsvRow.TitleType,
                PrimaryTitle = tsvRow.PrimaryTitle,
                OriginalTitle = tsvRow.OriginalTitle,
                IsAdult = tsvRow.IsAdult == "1" ? 1 : 0,
                StartYear = ImdbNull.IsImdbNull(tsvRow.StartYear)
                          ? (int?)null
                          : Convert.ToInt32(tsvRow.StartYear),
                EndYear = ImdbNull.IsImdbNull(tsvRow.EndYear)
                          ? (int?)null
                          : Convert.ToInt32(tsvRow.EndYear),
                RuntimeMinutes = ImdbNull.IsImdbNull(tsvRow.RuntimeMinutes)
                          ? (int?)null
                          : Convert.ToInt32(tsvRow.RuntimeMinutes),
                GenresCsv = tsvRow.GenresArray
            };

            return dbRow;
        }

        public static IEnumerable<TableColumn> TableColumns()
        {
            yield return new TableColumn
            {
                IsPrimaryKey = true,
                Name = StringFns.ToSnakeCase(nameof(TitleId)),
                RuntimeType = typeof(string)
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(TitleType)),
                RuntimeType = typeof(string),
                IsNullable = true
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(PrimaryTitle)),
                RuntimeType = typeof(string),
                IsNullable = true
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(OriginalTitle)),
                RuntimeType = typeof(string),
                IsNullable = true
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(IsAdult)),
                RuntimeType = typeof(bool)
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(StartYear)),
                RuntimeType = typeof(int),
                IsNullable = true
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(EndYear)),
                RuntimeType = typeof(int),
                IsNullable = true
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(RuntimeMinutes)),
                RuntimeType = typeof(int),
                IsNullable = false
            };
            yield return new TableColumn
            {
                Name = StringFns.ToSnakeCase(nameof(GenresCsv)),
                RuntimeType = typeof(string)
            };
        }
    }

    public class TableColumn
    {
        public bool IsPrimaryKey { get; set; }
        public string Name { get; set; } = "";
        public Type RuntimeType { get; set; } = typeof(object);
        public bool IsNullable { get; set; }
    }
}
