namespace AuthService.Core.Exceptions
{
    public class UserOPasswordException : Exception
    {
        public UserOPasswordException()
            : base("Email o Password Incorrectos.") { }
    }
}
