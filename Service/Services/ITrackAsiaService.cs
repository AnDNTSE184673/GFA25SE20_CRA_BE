using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ITrackAsiaService
    {
        Task<(string, string)?> GetPlaceCoordinate(string address);
        Task<int?> GetDistanceBetween(string scCoord, string desCoord);
    }
}
