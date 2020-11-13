using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Entities;

namespace UserManagement.Data
{
    public class UserDataContext: DbContext
    {
        protected readonly IConfiguration Configuration;

        public UserDataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("UserDbConnection"));
        }
        public DbSet<User> Users { get; set; }
    }
}
