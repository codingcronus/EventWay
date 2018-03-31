using EventWayCore.Core;

namespace EventWayCore.SampleApp.Core.Events
{
    public class UserRegistered : DomainEvent
    {
        public UserRegistered(
            string firstName,
            string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }
    }
}
