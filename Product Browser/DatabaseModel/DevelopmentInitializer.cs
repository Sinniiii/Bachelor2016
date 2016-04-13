using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DatabaseModel.Model;

namespace DatabaseModel
{
    /// <summary>
    /// Development class to drop database on changes, then reseed with stuff
    /// </summary>
    public class DevelopmentInitializer : DropCreateDatabaseIfModelChanges<ABBDataContext>
    {

        public DevelopmentInitializer()
            : base()
        {
            Seed(new ABBDataContext());
        }
        protected override void Seed(ABBDataContext context)
        {
            // seed database here
            SmartCardDataItem item = null;
            byte[] bytes = File.ReadAllBytes("TestData\\Theory Of Random.pdf");

            item = new SmartCardDataItem("Test1", SmartCardDataItemCategory.Document,
                    bytes);

            context.SmartCardDataItems.Add(item);

            context.SaveChanges();
        }
    }
}
