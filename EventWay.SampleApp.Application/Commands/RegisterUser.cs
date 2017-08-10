namespace EventWay.SampleApp.Application.Commands
{
    public class RegisterUser
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
