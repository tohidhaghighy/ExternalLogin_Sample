using ExternalLogin.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ExternalLogin.Context
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
