using System;
using System.Collections.Generic;
using System.Linq;
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
            public const string CUSTOMER = "Customer";
        }
        
        public static class Statuses
        {
            public const string ACTIVE = "Active";
            public const string PENDING = "Pending";
            public const string INACTIVE = "Inactive";
            public const string PUBLIC = "Public";
            public const string PRIVATE = "Private";
        }

        public enum RoleID
        {
            ADMIN = 1,
            CUSTOMER = 2
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
            PENDING = 3,
            //1. SQL Server Identity Jump Behavior (Post-SQL Server 2012)
            //Starting from SQL Server 2012, the IDENTITY column may auto-increment in steps of 1000 under certain conditions due to performance optimization.
            PUBLIC = 1001,
            PRIVATE = 1002
        }
    }
}
