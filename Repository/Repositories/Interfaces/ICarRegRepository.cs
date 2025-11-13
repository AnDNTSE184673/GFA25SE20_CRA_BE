using Microsoft.AspNetCore.Http;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarRegRepository
    {
        Task<(string status, CarRegistration regData)> UploadCarRegistration(CarRegistration data);
    }
}
