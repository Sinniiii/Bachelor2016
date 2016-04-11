using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseModel.Model
{
    /// <summary>
    /// A data item kept by Product. Could be a raw image, document or video.
    /// </summary>
    public class ProductDataItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ProductDataItemCategory Category { get; set; }

        public byte[] Data { get; set; }
    }
}
