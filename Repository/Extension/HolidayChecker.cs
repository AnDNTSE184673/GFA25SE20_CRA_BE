using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension
{
    public class HolidayChecker
    {
        public bool IsHolidayInDateRange(DateTime pickupTime, DateTime dropoffTime)
        {
            // 1. Ensure the range is valid (optional, but good practice)
            if (pickupTime > dropoffTime)
            {
                throw new ArgumentException("PickupTime must be before DropoffTime.");
            }

            // Normalize to Date only (ignore time components)
            DateTime start = pickupTime.Date;
            DateTime end = dropoffTime.Date;

            // 2. Get all relevant holidays for the year range
            var holidays = GetVietnameseHolidays(start.Year, end.Year);

            // 3. Iterate from start date to end date
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (holidays.Contains(date))
                {
                    return true; // Found a holiday!
                }
            }

            return false; // No holidays found in the range
        }

        // --- Holiday Definition Helper ---

        /// <summary>
        /// Returns a HashSet of all official Vietnamese public holidays 
        /// for the given year range.
        /// NOTE: This only includes official statutory holidays and does not 
        /// include official 'weekend compensation' days which are highly variable.
        /// </summary>
        private HashSet<DateTime> GetVietnameseHolidays(int startYear, int endYear)
        {
            var holidays = new HashSet<DateTime>();

            // Ensure we cover all years in the range
            for (int year = startYear; year <= endYear; year++)
            {
                // 1. Fixed-Date Holidays (Statutory Minimums)

                // New Year's Day (Jan 1)
                holidays.Add(new DateTime(year, 1, 1));

                // Liberation Day (April 30) & Labor Day (May 1) (2 days)
                holidays.Add(new DateTime(year, 4, 30));
                holidays.Add(new DateTime(year, 5, 1));

                // National Day (September 2)
                // Vietnam usually takes Sep 2 plus one adjacent day (Sep 1 or Sep 3). 
                // We include the official 2nd, and the adjacent 1st for a typical 2-day break.
                holidays.Add(new DateTime(year, 9, 2));
                holidays.Add(new DateTime(year, 9, 1)); // Include the adjacent day

                // 2. Lunar Calendar Holidays (Dynamic: Tết and Hung Kings)

                // Define dynamic holidays based on the official solar dates for the 10th day of the 3rd lunar month.
                // NOTE: These dates represent the *official* statutory non-working days.

                DateTime hungKingsDay = default;
                (DateTime tetStart, DateTime tetEnd) tetBreak = default;

                switch (year)
                {
                    case 2026:
                        // Hung Kings' Temple Festival (10th day of 3rd lunar month) - Sat, April 26 + Mon, April 27 for compensation
                        hungKingsDay = new DateTime(2026, 4, 26);
                        holidays.Add(new DateTime(2026, 4, 27)); // Compensation day for Sunday

                        // Tết (Lunar New Year) - Approx. 9-day break (Feb 16 to Feb 24)
                        tetBreak = (new DateTime(2026, 2, 16), new DateTime(2026, 2, 24));
                        break;

                    case 2027:
                        // Hung Kings' Temple Festival - Friday, April 16
                        hungKingsDay = new DateTime(2027, 4, 16);

                        // Tết - Approx. 9-day break (Feb 5 to Feb 13)
                        tetBreak = (new DateTime(2027, 2, 5), new DateTime(2027, 2, 13));
                        break;

                    case 2028:
                        // Hung Kings' Temple Festival - Tuesday, April 4
                        hungKingsDay = new DateTime(2028, 4, 4);

                        // Tết - Approx. 9-day break (Jan 25 to Feb 2)
                        tetBreak = (new DateTime(2028, 1, 25), new DateTime(2028, 2, 2));
                        break;

                    case 2029:
                        // Hung Kings' Temple Festival - Monday, April 23
                        hungKingsDay = new DateTime(2029, 4, 23);

                        // Tết - Approx. 9-day break (Feb 12 to Feb 20)
                        tetBreak = (new DateTime(2029, 2, 12), new DateTime(2029, 2, 20));
                        break;

                    case 2030:
                        // Hung Kings' Temple Festival - Friday, April 12
                        hungKingsDay = new DateTime(2030, 4, 12);

                        // Tết - Approx. 9-day break (Feb 2 to Feb 10)
                        tetBreak = (new DateTime(2030, 2, 2), new DateTime(2030, 2, 10));
                        break;
                }

                // Add Hung Kings' Day
                if (hungKingsDay != default)
                {
                    holidays.Add(hungKingsDay);
                }

                // Add Tết Break
                if (tetBreak.tetStart != default)
                {
                    for (DateTime date = tetBreak.tetStart; date <= tetBreak.tetEnd; date = date.AddDays(1))
                    {
                        holidays.Add(date);
                    }
                }
            }

            return holidays;
        }
    }
}
