using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingOptimiserDemo
{
    public class GlassCuttingContext : DbContext
    {
        public DbSet<Panel> Panels { get; set; }
        public DbSet<StockSheet> StockSheets { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<CutSegment> CutSegments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=glasscutting.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockSheet>().HasData(
            new StockSheet { Id = 1, Width = 1862, Height = 1219 },
            new StockSheet { Id = 2, Width = 3210, Height = 2440 },
            new StockSheet { Id = 3, Width = 3210, Height = 2250 },
            new StockSheet { Id = 4, Width = 2440, Height = 1830 }
            );

            modelBuilder.Entity<Panel>().HasData(
                new Panel { Id = 1, Width = 700, Height = 484, Quantity = 8 },
                new Panel { Id = 2, Width = 501, Height = 249, Quantity = 4 },
                new Panel { Id = 3, Width = 1132, Height = 675, Quantity = 2 },
                new Panel { Id = 4, Width = 485, Height = 433, Quantity = 2 },
                new Panel { Id = 5, Width = 485, Height = 433, Quantity = 1 },
                new Panel { Id = 6, Width = 522, Height = 466, Quantity = 5 },
                new Panel { Id = 7, Width = 362, Height = 1756, Quantity = 2 },
                new Panel { Id = 8, Width = 1726, Height = 926, Quantity = 2 }
            );
        }
    }
}
