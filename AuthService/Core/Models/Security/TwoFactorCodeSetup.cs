namespace AuthService.Core.Models.Security
{
    public class TwoFactorCodeSetup
    {
        public TwoFactorCodeSetup()
        {
            CustomValues = new Dictionary<string, string>();
        }
        public IDictionary<string, string> CustomValues { get; set; }
    }

}
