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
    public class DevelopmentInitializer : DropCreateDatabaseAlways<ABBDataContext>
    {
        protected override void Seed(ABBDataContext context)
        {
            // seed database here
            byte[] bytes = File.ReadAllBytes("TestData\\Theory Of Random.pdf");

            var item1 = new SmartCardDataItem("Test1", SmartCardDataItemCategory.Document,
                    bytes);

            bytes = File.ReadAllBytes("TestData\\arkema-st-auban.png");

            var item2 = new SmartCardDataItem("Test2", SmartCardDataItemCategory.Image,
                    bytes);

            bytes = File.ReadAllBytes("TestData\\ABB_ Life_Cycle_Assessment_service.mp4");

            var item3 = new SmartCardDataItem("ABB_ Life_Cycle_Assessment_service.mp4", SmartCardDataItemCategory.Video,
                    bytes);
            
            var smartcard = new SmartCard();
            smartcard.Name = "Test card 1";
            smartcard.TagId = 1;
            smartcard.DataItems = new List<SmartCardDataItem>();

            smartcard.DataItems.Add(item1);
            smartcard.DataItems.Add(item2);
            smartcard.DataItems.Add(item3);

            var smartcard2 = new SmartCard();
            smartcard2.Name = "Test card 2";
            smartcard2.TagId = 2;
            smartcard2.DataItems = new List<SmartCardDataItem>();

            smartcard2.DataItems.Add(item1);

            context.SmartCards.Add(smartcard);
            context.SmartCards.Add(smartcard2);

            //legger til et par flere kort, uten innhold - Bård
            for (int i = 3; i < 15; i++)
            {
                var temp = new SmartCard();
                temp.Name = $"Empty Card {i}";
                temp.TagId = i;
                temp.DataItems = new List<SmartCardDataItem>();
                context.SmartCards.Add(temp);
            }

            context.SaveChanges();
        }
    }
}
