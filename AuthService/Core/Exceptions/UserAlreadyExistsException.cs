namespace AuthService.Core.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string email, string ci)
            : base($"El usuario con este email: {email} o esta Cedula de Identidad:{ci} ya se encuentra registrados.") { }
    }
}
