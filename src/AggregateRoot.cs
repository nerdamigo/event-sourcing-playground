using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;

namespace EventSourcing
{
    public sealed class AggregateVersion
    {
        public AggregateVersion(int version)
        {
            Events = new List<IEvent>();
        }

        public int VersionNumber { get; private set; }
        public IList<IEvent> Events { get; }
    }

    public abstract class AggregateRoot<T>
        where T : AggregateRoot<T>
    {
        private static IReadOnlyDictionary<Type, MethodInfo> ApplyMethodIndex;
        static AggregateRoot()
        {
            DiscoverApplyMethods();
        }

        protected AggregateRoot(string entityId)
        {
            EntityId = entityId;
            currentVersion = new AggregateVersion(0);
        }

        private static void DiscoverApplyMethods()
        {
            //look for a protected method with a signature matching:
            // private void Apply(EventType ev)
            // private Task Apply(EventType ev)

            var ourType = typeof(T);
            var methods = 
                from method in ourType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                where method.Name.Equals("ApplyEvent")
                let returnType = method.ReturnType
                where returnType == typeof(void) || returnType == typeof(Task)
                    && method.GetParameters().Length == 1
                let eventType = method.GetParameters()[0].ParameterType
                where typeof(IEvent).IsAssignableFrom(eventType)
                select Tuple.Create(eventType, method);
            
            ApplyMethodIndex = methods.ToImmutableDictionary(k => k.Item1, e => e.Item2);
        }

        public string EntityId { get; private set; }
        private AggregateVersion currentVersion;

        public Task<IReadOnlyDictionary<string, object>> CreateSnapshot()
        {
            //iterate properties decorated with [Snapshot] attribute and add to dictionary
            throw new NotImplementedException();
        }

        public async Task Persist(IAggregateRepository repository)
        {
            if(currentVersion.Events.Count == 0) {
                await Task.CompletedTask;
                return;
            }

            var newVersion = new AggregateVersion(currentVersion?.VersionNumber ?? 0);
            var saveVersion = Interlocked.Exchange(ref currentVersion, newVersion);
            
            try
            {
                var snap = await CreateSnapshot(); 
                await repository.AddVersion(EntityId, saveVersion, snap);
            }
            catch
            {
                //if we throw an exception, and a another thread had made
                // changes to the newer version, persistence will fail for the other
                // thread due to a concurrency exception on save
                throw;
            }
        }

        public async Task Load(IAggregateRepository repository)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        private void ApplySnapshot(AggregateVersion version, IReadOnlyDictionary<string, object> snapshot)
        {
            //iterate keys in snapshot and set the matching property with [Snapshot] attribute
            throw new NotImplementedException();
        }

        private async Task ApplyVersion(AggregateVersion version)
        {
            currentVersion = version;
            foreach(var @event in version.Events)
            {
                await AddEvent(@event);
            }
        }

        protected async Task AddEvent(IEvent @event)
        {
            MethodInfo applyMethod;
            if(!ApplyMethodIndex.TryGetValue(@event.GetType(), out applyMethod))
            {
                throw new ArgumentException($"Event Type {@event.GetType().Name} does not have an ApplyEvent Method");
            }

            var applyResult = applyMethod.Invoke(this, new[] { @event });

            if(applyResult != null && typeof(Task).IsAssignableFrom(applyResult.GetType())) {
                await (Task)applyResult;
            }

            currentVersion.Events.Add(@event);
            await Task.CompletedTask;
        }

        public override abstract string ToString();

        //identity for AggregateRoot(s) is defined entirely by identity
        // concepts like version don't make sense for comparing equality, you're
        // still talking about the same thing
        public override bool Equals(object obj)
        {
            if(Object.ReferenceEquals(obj, this)) return true;
            var other = obj as AggregateRoot<T>;
            if(other == null) { return false; }
            return this.EntityId.Equals(other.EntityId);
        }

        public static bool operator !=(AggregateRoot<T> obj1, AggregateRoot<T> obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator ==(AggregateRoot<T> obj1, AggregateRoot<T> obj2)
        {
            //if both objects are null, result is true
            if((obj1 ?? obj2) == null) return true;
            return obj1?.Equals(obj2) ?? false;
        }

        public override int GetHashCode() => this.EntityId.GetHashCode();

    } 
}