using Repository.Data.Entities;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IJWTService
    {
        LoginResponse GenerateToken(User user);
    }
}
