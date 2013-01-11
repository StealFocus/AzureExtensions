namespace StealFocus.AzureExtensions.HostedService
{
    using System;

    public struct HorizontalScale : IEquatable<HorizontalScale>
    {
        public string RoleName { get; set; }

        public int InstanceCount { get; set; }

        public static bool operator ==(HorizontalScale horizontalScale1, HorizontalScale horizontalScale2)
        {
            return horizontalScale1.Equals(horizontalScale2);
        }

        public static bool operator !=(HorizontalScale horizontalScale1, HorizontalScale horizontalScale2)
        {
            return !horizontalScale1.Equals(horizontalScale2);
        }

        public bool Equals(HorizontalScale other)
        {
            return string.Equals(this.RoleName, other.RoleName) && this.InstanceCount == other.InstanceCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is HorizontalScale && Equals((HorizontalScale)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.RoleName != null ? this.RoleName.GetHashCode() : 0) * 397) ^ this.InstanceCount;
            }
        }
    }
}
