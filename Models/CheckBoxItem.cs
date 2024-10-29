using System;
using System.Collections.Generic;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class CheckBoxItem
    {
        public int ID { get; set; }
        public string FeesName { get; set; } = string.Empty;
        public string TypeOfFees {  get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        
    }
}
