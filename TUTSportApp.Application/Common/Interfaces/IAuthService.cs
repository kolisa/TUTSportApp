using TUTSportApp.Application.Common.Models;
using TUTSportApp.Application.Features.Auth.Commands;

namespace TUTSportApp.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<string> CreateTokenAsync(LoginCommand request);
        string CreatePasswordHash(string password);
        bool VerifyPasswordHash(string password, string passwordHash);
    }
}