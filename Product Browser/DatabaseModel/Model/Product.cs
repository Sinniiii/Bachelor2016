using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseModel.Model
{
    /// <summary>
    /// A product is a set of ProductDataItems with an associated name and TagId(for recognizing the product using smartcards)
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int TagId { get; set; }

        public virtual List<ProductDataItem> DataItems { get; set; }
    }
}
