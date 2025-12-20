using Repository.DTO.ResponseDTO.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Infranstructure
{
    public interface IGpsStore
    {
        void Upsert(GpsPayload payload);
        bool TryGet(Guid carId, out GpsPayload payload);
        IReadOnlyCollection<GpsPayload> GetAll();
    }
}
