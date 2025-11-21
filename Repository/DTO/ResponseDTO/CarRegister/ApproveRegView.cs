using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarRegister
{
    public class ApproveRegView
    {
        public List<SingleRegData> Document { get; set; }

        public UserView Owner { get; set; }
        public CarView Car { get; set; }
    }

    public class SingleRegData
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }
    }
}
