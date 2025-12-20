using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class GPSCoordinateDoulbe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Trim whitespace
            migrationBuilder.Sql(@"
UPDATE ""GPS"" SET ""Longitude"" = trim(""Longitude"") WHERE ""Longitude"" IS NOT NULL;
UPDATE ""GPS"" SET ""Latitude"" = trim(""Latitude"") WHERE ""Latitude"" IS NOT NULL;
");

            // 2) Replace empty / non-numeric values with a safe numeric default (0.0)
            //    Adjust the default if you prefer a different fallback (e.g. NULL then make column nullable).
            migrationBuilder.Sql(@"
UPDATE ""GPS""
SET ""Longitude"" = '0'
WHERE ""Longitude"" IS NULL
   OR ""Longitude"" = ''
   OR trim(""Longitude"") !~ '^[+-]?\d+(\.\d+)?$';

UPDATE ""GPS""
SET ""Latitude"" = '0'
WHERE ""Latitude"" IS NULL
   OR ""Latitude"" = ''
   OR trim(""Latitude"") !~ '^[+-]?\d+(\.\d+)?$';
");

            // 3) Alter types using explicit USING cast (safe because we've sanitized values)
            migrationBuilder.Sql(@"
ALTER TABLE ""GPS""
ALTER COLUMN ""Longitude"" TYPE double precision
USING trim(""Longitude"")::double precision;
");

            migrationBuilder.Sql(@"
ALTER TABLE ""GPS""
ALTER COLUMN ""Latitude"" TYPE double precision
USING trim(""Latitude"")::double precision;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to text explicitly
            migrationBuilder.Sql(@"ALTER TABLE ""GPS"" ALTER COLUMN ""Longitude"" TYPE text USING ""Longitude""::text;");
            migrationBuilder.Sql(@"ALTER TABLE ""GPS"" ALTER COLUMN ""Latitude"" TYPE text USING ""Latitude""::text;");
        }
    }
}
