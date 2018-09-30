using System;
using EventSourcing;

namespace EventSourcingTests
{
    internal class PersonBirthDateChangedEvent : IEvent
    {
        public DateTime NewBirthday { get; private set; }

        public PersonBirthDateChangedEvent(DateTime newBirthday)
        {
            this.NewBirthday = newBirthday;
        }
    }
}