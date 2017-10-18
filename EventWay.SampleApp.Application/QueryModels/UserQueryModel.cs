using System;
using EventWay.Query;

namespace EventWay.SampleApp.Application.QueryModels
{
    public class UserQueryModel : QueryModel
    {
        public UserQueryModel(Guid id) : base(id)
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }

        public override string BaseType => string.Empty;
    }
}
