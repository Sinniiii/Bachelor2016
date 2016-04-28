using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseModel.Model
{
    public class SmartCardDataItemData
    {
        public int Id { get; private set; }

        public byte[] Data { get; private set; }

        [Required]
        public SmartCardDataItem SmartCardDataItem {get; private set;}

        public void SetData(byte[] data)
        {
            Data = data;
        }

        public SmartCardDataItemData(byte[] data)
        {
            Data = data;
        }

        private SmartCardDataItemData() { }
    }
}
