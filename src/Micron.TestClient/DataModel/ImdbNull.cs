namespace Micron.TestClient.DataModel
{
    public class ImdbNull
    {
        public static readonly ImdbNull Instance = new ImdbNull();

        public static readonly string NullString = @"\N";

        private ImdbNull() { }

        public override bool Equals(object? obj) =>
            ReferenceEquals(obj, this) || (obj is ImdbNull);

        public static bool IsImdbNull(string? value) =>
            value != null && value == @"\N";

        public override int GetHashCode() => 0;
        public override string? ToString() => NullString;

        public static implicit operator string(ImdbNull value) =>
            value?.ToString() ?? NullString;
    }
}
