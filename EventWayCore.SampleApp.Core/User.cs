using System;
using EventWayCore.Core;
using EventWayCore.SampleApp.Core.Commands;
using EventWayCore.SampleApp.Core.Events;

namespace EventWayCore.SampleApp.Core
{
    public class User : Aggregate
    {
        //Internal state
        protected UserState State { get; private set; }

        public User(Guid id) : base(id)
        {
            // Events
            OnEvent<UserRegistered>(e => {
                Console.WriteLine("Got UserRegistered event");

                State = new UserState
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName
                };
            });

            // Commands
            OnCommand<RegisterUser>(c => {
                Console.WriteLine("Got RegisterUser command");

                if (State != null)
                    throw new Exception("User already exists");

                Publish(new UserRegistered(
                    c.FirstName,
                    c.LastName));
            });
        }

        // The internal state representation
        public class UserState
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
