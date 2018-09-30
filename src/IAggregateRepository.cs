using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EventSourcing
{
    public interface IAggregateRepository
    {
        Task<T> GetAggregate<T>(string aggregateId)
            where T : AggregateRoot<T>, new();
        Task AddVersion(
            string entityId, 
            AggregateVersion @version, 
            IReadOnlyDictionary<string, object> snapshot
        );
    }
}
