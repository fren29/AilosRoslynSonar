using Microsoft.EntityFrameworkCore;
using SonarQubeTest.Domain.Entities;

namespace SonarQubeTest.Infra.Context
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext (DbContextOptions<SqlServerContext> options) 
            : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
    }
}
