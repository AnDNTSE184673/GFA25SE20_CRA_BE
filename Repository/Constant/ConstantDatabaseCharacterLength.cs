using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Constant
{
    public class ConstantDatabaseCharacterLength
    {
        public static class CharacterLength
        {
            // === Identifiers & Codes ===
            public const int ShortCode = 10;          // e.g., OTP, short codes
            public const int MediumCode = 20;         // e.g., staff codes, manual IDs
            public const int LicensePlate = 15;

            // === Person Info ===
            public const int FirstName = 50;
            public const int LastName = 50;
            public const int FullName = 100;

            // === Account & Auth ===
            public const int Username = 50;
            public const int Email = 254;             // RFC standard max email length
            public const int PhoneNumber = 20;
            public const int PasswordHash = 500;      // hashed passwords are long

            // === Address & Location ===
            public const int AddressLine = 200;
            public const int City = 100;
            public const int Country = 100;
            public const int PostalCode = 20;

            // === Business Entities ===
            public const int Title = 200;
            public const int ShortDescription = 500;
            public const int LongDescription = 2000;  // safe for long-form text
            public const int Notes = 2000;

            // === System/Meta Data ===
            public const int FileName = 255;          // OS filename max
            public const int ContentType = 100;
            public const int Url = 2048;              // browser-safe URL size

            // === Generic Sizes ===
            public const int Tiny = 20;
            public const int Small = 50;
            public const int Medium = 100;
            public const int Large = 500;
            public const int ExtraLarge = 1000;
        }

    }
}
