using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarRegister
{
    public class ApproveLicenseView
    {
        public List<SingleLicenseData> Document { get; set; } = new List<SingleLicenseData>();

        public UserView Owner { get; set; }
    }

    public class SingleLicenseData
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }
    }
}
