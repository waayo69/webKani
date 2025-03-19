using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer;
using webKani.Models;

namespace webKani.Data
{
    public class MyAppContext:DbContext
    {
        public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
        {
        }
        public DbSet<Item> Items { get; set; }

    }
}
