using DatabaseModel.Model;

namespace DatabaseModel
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class ABBDataContext : DbContext
    {
        public ABBDataContext()
            : base("name=ABBDataContext")
        {
        }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<ProductDataItem> ProductDataItems { get; set; }

        public virtual DbSet<ProductDataItemCategory> ProductDataItemCategories { get; set; }
    }
}