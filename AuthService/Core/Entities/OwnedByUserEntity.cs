namespace AuthService.Core.Entities
{
    public class OwnedByUserEntity
    {
        public Guid IdUser { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
