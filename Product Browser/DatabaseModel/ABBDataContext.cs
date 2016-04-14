using DatabaseModel.Model;

namespace DatabaseModel
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Model;

    public class ABBDataContext : DbContext
    {
        public ABBDataContext()
            : base("name=ABBDataContext")
        {
            this.Configuration.LazyLoadingEnabled = true;
        }

        public virtual DbSet<SmartCard> SmartCards { get; set; }

        public virtual DbSet<SmartCardDataItem> SmartCardDataItems { get; set; }
    }
}