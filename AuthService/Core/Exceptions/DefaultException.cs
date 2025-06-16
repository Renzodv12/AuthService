namespace AuthService.Core.Exceptions
{
    public class DefaultException : Exception
    {
        public DefaultException(string Message)
            : base(Message) { }
    }
}
