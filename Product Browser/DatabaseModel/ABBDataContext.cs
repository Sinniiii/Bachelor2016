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
            //Database.SetInitializer(new DevelopmentInitializer()); // This is only while developing, it drops database and reseeds it

            Configuration.LazyLoadingEnabled = true;
        }

        public virtual DbSet<SmartCard> SmartCards { get; set; }

        public virtual DbSet<SmartCardDataItem> SmartCardDataItems { get; set; }
    }
}