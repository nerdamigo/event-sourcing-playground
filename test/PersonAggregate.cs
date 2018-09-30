using System;
using System.Threading.Tasks;
using EventSourcing;

namespace EventSourcingTests
{
    public sealed class PersonAggregate : AggregateRoot<PersonAggregate>
    {
        public PersonAggregate(string entityId) : base(entityId) {}

        public override string ToString()
        {
            return $"Person '{FullName}' is {Age} years old";
        }

        public DateTime? BirthDate { get; private set; }
        public string FullName { get; private set; }

        //returns age since birth, can be negative if BirthDate is in future
        public int? Age
        {
            get
            {
                if (!BirthDate.HasValue) return null;
                var birthdayPassed = DateTime.UtcNow.Month > BirthDate.Value.Month ||
                    (DateTime.UtcNow.Month == BirthDate.Value.Month && DateTime.UtcNow.Day >= BirthDate.Value.Day);
                return DateTime.UtcNow.Year - BirthDate.Value.Year - (birthdayPassed ? 0 : 1);
            }
        }

        public async Task SetBirthDate(DateTime newBirthday)
        {
            if (newBirthday.Kind != DateTimeKind.Utc) { throw new ArgumentException("Expected in UTC"); }
            if (!newBirthday.Equals(BirthDate))
            {
                await AddEvent(new PersonBirthDateChangedEvent(newBirthday));
            }
        }

        private void ApplyEvent(PersonBirthDateChangedEvent ev) => BirthDate = ev.NewBirthday;
        //private async Task ApplyEvent(PersonBirthDateChangedEvent ev) => BirthDate = ev.NewBirthday;
    }
}