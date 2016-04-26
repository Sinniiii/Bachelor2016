using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseModel.Model
{
    /// <summary>
    /// A virtual smart card. Has a name, a list of data items and an associated TagId, identifying the physical card
    /// to which it is connected.
    /// </summary>
    public class SmartCard
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int TagId { get; set; }

        public virtual List<SmartCardDataItem> DataItems { get; set; }
    }
}
