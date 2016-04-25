using DatabaseModel.Model;

namespace DatabaseModel
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Model;

    public class ABBDataContext : DbContext
    {
        public ABBDataContext()
            : base("name=ABBDataContext")
        {
            Database.SetInitializer(new DevelopmentInitializer()); // This is only while developing, it drops database and reseeds it
        }

        //protected override void OnModelCreating(DbModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    builder.Entity<SmartCard>().ToTable("SmartCards");

        //    builder.Entity<SmartCardDataItem>().
        //        dataItem.HasOne(item => item.SmartCard_Id).WithMany(smartcard => smartcard.DataItems);

        //    // Customize the ASP.NET Identity model and override the defaults if needed.
        //    // For example, you can rename the ASP.NET Identity table names and more.
        //    // Add your customizations after calling base.OnModelCreating(builder);
        //}

        public virtual DbSet<SmartCard> SmartCards { get; set; }

        public virtual DbSet<SmartCardDataItem> SmartCardDataItems { get; set; }
    }
}