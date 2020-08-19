namespace Micron.SqlClient
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public struct Unit : IEquatable<Unit>
    {
        public static Unit Default => default;

        public override bool Equals(object? obj) => obj is Unit;

        public bool Equals([AllowNull] Unit other) => true;

        public override int GetHashCode() => 0;

        public override string? ToString() => "()";

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "first", Justification = "Parameter required for operator overloading."), Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second", Justification = "Parameter required for operator overloading.")]
        public static bool operator ==(Unit a, Unit b) => true;

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "first", Justification = "Parameter required for operator overloading."), Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second", Justification = "Parameter required for operator overloading.")]
        public static bool operator !=(Unit a, Unit b) => false;
    }
}