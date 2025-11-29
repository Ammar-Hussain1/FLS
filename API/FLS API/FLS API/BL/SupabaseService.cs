using FLS_API.DL.Models;
using FLS_API.DL.DTOs;
using FLS_API.Models;
using Supabase;
using Supabase.Realtime;

namespace FLS_API.BL
{
    public class SupabaseService
    {
        private readonly Supabase.Client _client;

        public SupabaseService(Models.SupabaseOptions options)
        {
            if (string.IsNullOrEmpty(options.Url))
                throw new ArgumentException("Supabase URL is required", nameof(options));
            
            // Use AnonKey for PostgREST operations, ServiceRoleKey for admin operations
            var apiKey = !string.IsNullOrEmpty(options.AnonKey) ? options.AnonKey : options.ServiceRoleKey;
            
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Supabase API key (AnonKey or ServiceRoleKey) is required", nameof(options));

            _client = new Supabase.Client(
                options.Url,
                apiKey
            );
            
            _client.InitializeAsync().GetAwaiter().GetResult();
        }

        public Supabase.Client Client => _client;

        public async Task<User> SignUpUserAsync(string fullName, string email, string password)
        {
            var authResult = await _client.Auth.SignUp(email, password);

            if (authResult.User == null)
                throw new Exception("Unable to create Supabase Auth user.");

            // Insert into Users table
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            await _client.From<User>().Insert(user);
            return user;
        }

        public async Task<User?> SignInUserAsync(string email, string password)
        {
            var session = await _client.Auth.SignIn(email, password);
            if (session.User == null)
                return null;

            var user = await GetUserByEmailAsync(email);
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var response = await _client.From<User>()
                .Where(u => u.Email == email)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = await _client.From<User>().Get();
            return response.Models;
        }

        public async Task<User?> UpdateUserAsync(int id, UsersDTO dto)
        {
            var existing = await _client.From<User>().Where(u => u.Id == id).Get();
            var user = existing.Models.FirstOrDefault();
            if (user == null) return null;

            user.FullName = dto.FullName ?? user.FullName;
            user.Role = dto.Role ?? user.Role;

            await _client.From<User>().Update(user);
            return user;
        }
    }
}
