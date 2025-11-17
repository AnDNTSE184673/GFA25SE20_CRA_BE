using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Constant
{
    public static class ConstantEnum
    {
        public static class RepoStatus
        {
            public const string SUCCESS = "Success";
            public const string FAILURE = "Failure";
        }

        public static class Gender
        {
            public const string MALE = "Male";
            public const string FEMALE = "Female";
            public const string OTHER = "Other";
        }

        public static class Roles
        {
            public const string ADMIN = "Admin";
            public const string STAFF = "Staff";
            public const string CUSTOMER = "Customer";
        }
        
        public static class Statuses
        {
            public const string ACTIVE = "Active";
            public const string PENDING = "Pending";
            public const string INACTIVE = "Inactive";
            public const string PUBLIC = "Public";
            public const string PRIVATE = "Private";
            public const string COMPLETED = "Completed";
            public const string CANCELLED = "Cancelled";
            public const string ONGOING = "Ongoing";
            public const string PAYMENT_PENDING = "Waiting for Payment";
            public const string REFUNDED = "Refunded";
            public const string RESERVED = "Reserved";
            public const string APPROVED = "Approved";
            public const string DENIED = "Denied";
        }

        public static class SupabaseBucket
        {
            public const string CarRegistration = "CarRegistrationDocs";
            public const string DriverLicense = "DriverLicenseDocs";
            public const string FeedbackImages = "FeedbackImages";
        }

        public enum RoleID
        {
            ADMIN = 1001,
            STAFF = 1002,
            CUSTOMER = 1
        }

        public enum GenderID
        {
            MALE = 1,
            FEMALE = 2,
            OTHER = 3
        }

        public enum InternalStatusID
        {
            SUCCESS = 0,
            FAILURE = -1
        }

        public enum StatusID
        {
            ACTIVE = 1,
            INACTIVE = 2,
            PENDING = 3
            //1. SQL Server Identity Jump Behavior (Post-SQL Server 2012)
            //Starting from SQL Server 2012, the IDENTITY column may auto-increment in steps of 1000 under certain conditions due to performance optimization.
        }

        public enum Status
        {
            Active,
            Inactive,
            Pending,
            Public,
            Private,
            Completed,
            Cancelled,
            Ongoing,
            [EnumMember(Value ="Waiting for Payment")]
            PaymentPending,
            Refunded,
            Reserved,
            Approved,
            Denied
        }
    }
}
