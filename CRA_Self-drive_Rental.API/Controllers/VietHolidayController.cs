using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VietHolidayController : ControllerBase
    {
        [HttpGet("Up to 2028")]
        public IActionResult GetVietHolidaysUpTo2028()
        {
            var holidays = new HashSet<DateTime>();
            for (int i =2026; i <= 2028; i++)
            {
                // New Year's Day (Jan 1)
                holidays.Add(new DateTime(i, 1, 1));

                // Liberation Day (April 30) & Labor Day (May 1) (2 days)
                holidays.Add(new DateTime(i, 4, 30));
                holidays.Add(new DateTime(i, 5, 1));

                // National Day (September 2)
                // Vietnam usually takes Sep 2 plus one adjacent day (Sep 1 or Sep 3). 
                // We include the official 2nd, and the adjacent 1st for a typical 2-day break.
                holidays.Add(new DateTime(i, 9, 2));
                holidays.Add(new DateTime(i, 9, 1));
                // Dynamic Holidays
                switch (i)
                {
                    case 2026:
                        // Hung Kings' Temple Festival (10th day of 3rd lunar month) - Sat, April 26 + Mon, April 27 for compensation
                        holidays.Add(new DateTime(2026, 4, 26));
                        holidays.Add(new DateTime(2026, 4, 27)); // Compensation day for Sunday
                        // Tết (Lunar New Year) - Approx. 9-day break (Feb 16 to Feb 24)
                        for (var date = new DateTime(2026, 2, 16); date <= new DateTime(2026, 2, 24); date = date.AddDays(1))
                        {
                            holidays.Add(date);
                        }
                        break;
                    case 2027:
                        // Hung Kings' Temple Festival - Friday, April 16
                        holidays.Add(new DateTime(2027, 4, 16));
                        // Tết - Approx. 9-day break (Feb 5 to Feb 13)
                        for (var date = new DateTime(2027, 2, 5); date <= new DateTime(2027, 2, 13); date = date.AddDays(1))
                        {
                            holidays.Add(date);
                        }
                        break;
                    case 2028:
                        // Hung Kings' Temple Festival - Tuesday, April 4
                        holidays.Add(new DateTime(2028, 4, 4));
                        // Tết - Approx. 9-day break (Jan 25 to Feb 2)
                        for (var date = new DateTime(2028, 1, 25); date <= new DateTime(2028, 2, 2); date = date.AddDays(1))
                        {
                            holidays.Add(date);
                        }
                        break;
                }
            }

            return Ok(holidays);
        }
    }
}
