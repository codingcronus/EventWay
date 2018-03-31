using EventWayCore.Core;

namespace EventWayCore.SampleApp.Core.Commands
{
    public class RegisterUser : IDomainCommand
    {
        public RegisterUser(
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
