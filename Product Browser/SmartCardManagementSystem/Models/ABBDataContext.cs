using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using DatabaseModel.Model;

namespace SmartCardManagementSystem.Models
{
    public class ABBDataContext : DbContext
    {
        public ABBDataContext()
            : base()
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SmartCard>().ToTable("SmartCards");

            builder.Entity<SmartCardDataItem>().ToTable("SmartCardDataItems");

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<SmartCard> SmartCards { get; set; }

        public DbSet<SmartCardDataItem> SmartCardDataItems { get; set; }
    }
}
