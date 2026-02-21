using TUTSportApp.Application.Features.Auth.Commands;

namespace TUTSportApp.UnitTest.Builders
{
    public class LoginCommandBuilder
    {
        private string _username = "user";
        private string _password = "password123";

        public LoginCommandBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public LoginCommandBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public LoginCommand Build() => new LoginCommand { Username = _username, Password = _password };
    }
}
