using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackEndAPI.Models;

namespace BackEndAPI.Data
{
    public class BackEndAPIContext : DbContext
    {
        public BackEndAPIContext (DbContextOptions<BackEndAPIContext> options)
            : base(options)
        {
        }

        public DbSet<BackEndAPI.Models.Client> Client { get; set; } = default!;
        public DbSet<BackEndAPI.Models.Seller> Seller { get; set; } = default!;
        public DbSet<BackEndAPI.Models.Product> Product { get; set; } = default!;
        public DbSet<BackEndAPI.Models.Order> Order { get; set; } = default!;
    }
}
