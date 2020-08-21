namespace Micron.Core
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

        [SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Unit is always equal to itself.")]
        public static bool operator ==(Unit a, Unit b) => true;

        [SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Unit is never unequal to itself.")]
        public static bool operator !=(Unit a, Unit b) => false;
    }
}