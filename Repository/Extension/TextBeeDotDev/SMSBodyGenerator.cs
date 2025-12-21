using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension.TextBeeDotDev
{
    public class SMSBodyGenerator
    {
        public static string SMSMessage(string serviceName, string otpCode)
        {
            return $"{otpCode} is your {serviceName} verification code.";
        }
    }
}
