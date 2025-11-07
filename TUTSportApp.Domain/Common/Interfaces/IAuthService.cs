
using TUTSportApp.Domain.Models;

namespace TUTSportApp.Domain.Common.Interfaces
{
    public interface IAuthService
    {
        Task<string> CreateTokenAsync(LoginModel request);
        string CreatePasswordHash(string password);
        bool VerifyPasswordHash(string password, string passwordHash);
    }
}