using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ChatClient.Services
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }
    }

    public class AuthService
    {
        private readonly Client _supabase;

        public AuthService()
        {
            var url = "https://drttmgajjlyhpwurbgsm.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRydHRtZ2Fqamx5aHB3dXJiZ3NtIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc2MzM0NTcyNSwiZXhwIjoyMDc4OTIxNzI1fQ.tNQKdokibFLz_7ri0CjyiS3Ej5lB4hGRtlQVu2BIjF0";
            _supabase = new Client(url, key);
        }

        public async Task<User> SignUpAsync(string username, string password)
        {
            var existingUser = await _supabase.From<User>().Filter("username", Supabase.Postgrest.Constants.Operator.Equals, username).Get();
            if (existingUser.Models.Any())
            {
                return null;
            }

            var passwordHash = HashPassword(password);
            var user = new User { Username = username, PasswordHash = passwordHash };

            var response = await _supabase.From<User>().Insert(user);
            return response.Models.FirstOrDefault();
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var response = await _supabase.From<User>().Filter("username", Supabase.Postgrest.Constants.Operator.Equals, username).Get();
            var user = response.Models.FirstOrDefault();

            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }
    }
}
