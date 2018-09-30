using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EventSourcing
{
    //we can solve most threading issues here
    // provided NO THREAD attempts to RETAIN A REFERENCE to a given
    // entity. each time an operation is to be performed, it should
    // be loaded and persisted (with a retry on failure) to allow
    // for concurrency checking
    public sealed class InMemoryAggregateRepository : IAggregateRepository
    {
        public Task<T> GetAggregate<T>(string aggregateId)
            where T : AggregateRoot<T>, new()
        {
            //read our dictionary of dictionary (entityId, versionList/snapshot)
            throw new NotImplementedException();
        }

        public Task AddVersion(
            string entityId, 
            AggregateVersion @version, 
            IReadOnlyDictionary<string, object> snapshot
        )
        {
            throw new NotImplementedException();
        }
    } 
}