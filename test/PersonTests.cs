using System;
using System.Threading.Tasks;
using Xunit;

namespace EventSourcingTests
{
    public class PersonTests
    {
        [Fact]
        public async Task AgeSameDay()
        {
            var person = new PersonAggregate("dummyId");
            await person.SetBirthDate(DateTime.UtcNow.AddYears(-34));
            Assert.Equal(34, person.Age);
        }

        [Fact]
        public async Task AgeBirthdayTomorrow()
        {
            var person = new PersonAggregate("dummyId");
            await person.SetBirthDate(DateTime.UtcNow.AddYears(-34).AddDays(1));
            Assert.Equal(33, person.Age);
        }

        [Fact]
        public async Task AgeNotBorn()
        {
            var person = new PersonAggregate("dummyId");
            await person.SetBirthDate(DateTime.UtcNow.AddDays(1));
            Assert.Equal(-1, person.Age);
        }

        [Fact]
        public async Task AgeCurrentYear()
        {
            var person = new PersonAggregate("dummyId");
            await person.SetBirthDate(DateTime.UtcNow.AddDays(-1));
            Assert.Equal(0, person.Age);
        }
    }
}
