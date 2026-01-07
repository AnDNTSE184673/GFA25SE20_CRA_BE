using Repository.DTO.ResponseDTO.CarToll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarWalletService
    {
        Task<List<CarWalletView>> GetAllCarWallets();
        Task<CarWalletView> GetbyId(Guid id);
        Task<CarWalletView?> GetCarWalletByCarId(Guid carId);
        Task<CarWalletView?> CreateCarWallet(Guid carId);
        Task<CarWalletView?> UpdateCarWalletBalance(Guid carId, decimal amount);
        Task<CarWalletView?> AddToCarWallet(Guid carId, decimal amount);
        Task<CarWalletView?> SubtractFromCarWallet(Guid carId, decimal amount);
        Task<(string PaymentUrl, CarWalletView)> AddToWalletPayOS(Guid carId, decimal amount);
        Task<bool> DeleteCarWallet(Guid carId);
    }
}
