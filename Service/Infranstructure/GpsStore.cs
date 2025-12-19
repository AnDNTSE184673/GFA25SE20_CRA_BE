using Repository.DTO.ResponseDTO.GPS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Infranstructure
{
    public class GpsStore : IGpsStore
    {
        private readonly ConcurrentDictionary<Guid, GpsPayload> _store = new();

        public void Upsert(GpsPayload payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            _store.AddOrUpdate(payload.CarId, payload, (k, v) => payload);
        }

        public bool TryGet(Guid carId, out GpsPayload payload) => _store.TryGetValue(carId, out payload);

        public IReadOnlyCollection<GpsPayload> GetAll() => _store.Values as IReadOnlyCollection<GpsPayload> ?? new List<GpsPayload>(_store.Values);
    }
}
