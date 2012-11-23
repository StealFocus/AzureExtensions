namespace StealFocus.AzureExtensions.HostedService
{
    using System;
    using System.Net;

    public struct OperationResult : IEquatable<OperationResult>
    {
        public Guid Id { get; set; }

        public OperationStatus Status { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public static bool operator ==(OperationResult operationResult1, OperationResult operationResult2)
        {
            return operationResult1.Equals(operationResult2);
        }

        public static bool operator !=(OperationResult operationResult1, OperationResult operationResult2)
        {
            return !operationResult1.Equals(operationResult2);
        }

        public bool Equals(OperationResult other)
        {
            return this.Id.Equals(other.Id) && this.Status == other.Status && this.HttpStatusCode == other.HttpStatusCode && string.Equals(this.Code, other.Code) && string.Equals(this.Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is OperationResult && Equals((OperationResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.Status;
                hashCode = (hashCode * 397) ^ (int)this.HttpStatusCode;
                hashCode = (hashCode * 397) ^ (this.Code != null ? this.Code.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Message != null ? this.Message.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
