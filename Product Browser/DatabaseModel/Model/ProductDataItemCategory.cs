using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseModel.Model
{
    /// <summary>
    /// Defines the category of a ProductDataItem. Categories => Image, Document, Video ?
    /// </summary>
    public class ProductDataItemCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
