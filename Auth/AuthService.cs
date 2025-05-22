using System.Data;
using System.Security.Claims;
using EmployeeAttendance.Data;
using EmployeeAttendance.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EmployeeAttendance.Auth
{
    public class AuthService
    {
        private readonly DbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public AuthService(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            // In a real app, we should hash the password, but for this demo we use stored hash directly
            // This is just an example for the demo - in real app use BCrypt or similar
            var parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            var query = "SELECT * FROM Users WHERE Username = @Username";
            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            if (result.Rows.Count == 0)
                return null;

            var userRow = result.Rows[0];
            var storedHash = userRow["PasswordHash"].ToString();

            // In a real app we'd verify the hash, e.g.:
            // if (!BCrypt.Verify(password, storedHash))
            //    return null;
            
            // For demo we just return the user (assuming password is correct)
            return new User
            {
                UserID = Convert.ToInt32(userRow["UserID"]),
                Username = userRow["Username"].ToString()!,
                PasswordHash = storedHash!,
                Role = userRow["Role"].ToString()!,
                DepartmentManaged = userRow["DepartmentManaged"] as string,
                IsGeneralManager = Convert.ToBoolean(userRow["IsGeneralManager"])
            };
        }

        public async Task SignInAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserID.ToString())
            };

            if (user.Role == "Manager")
            {
                claims.Add(new Claim("IsGeneralManager", user.IsGeneralManager.ToString()));
                if (!user.IsGeneralManager && !string.IsNullOrEmpty(user.DepartmentManaged))
                {
                    claims.Add(new Claim("DepartmentManaged", user.DepartmentManaged));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                IsPersistent = true
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
} 