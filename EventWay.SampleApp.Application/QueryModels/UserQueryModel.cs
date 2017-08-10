using System;
using EventWay.Query;

namespace EventWay.SampleApp.Application.QueryModels
{
    public class UserQueryModel : QueryModel
    {
        public UserQueryModel(Guid aggregateId) : base(aggregateId)
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
    }
}
