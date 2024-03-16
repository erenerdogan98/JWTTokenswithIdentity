namespace JWTTokens.Models.DTO
{
    public class LoginResponse : Status
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
    }
}
