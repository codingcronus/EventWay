using System;
using System.Threading.Tasks;
using EventWay.SampleApp.Application.Commands;

namespace EventWay.SampleApp.Application
{
    public interface IUserApplicationService
    {
        Task<Guid> RegisterUser(RegisterUser command);
    }
}