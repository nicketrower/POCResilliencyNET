using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RegistrationAPI.Models;

namespace RegisterUserAPI.DAL
{
    public class GabDbContext : DbContext
    {
        public GabDbContext(DbContextOptions<GabDbContext> options) : base(options)
        {
        }

        public DbSet<UserInfo> UserInfo { get; set; }
    }
}
