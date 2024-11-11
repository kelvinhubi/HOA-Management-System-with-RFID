namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class PayoutDetails
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public Attributes Attributes{get; set;}
    }

    public class Attributes
    {
        public int Amount { get; set; }
        public bool Archived { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public bool Livemode { get; set; }
        public int Fee { get; set; }
        public object Remarks { get; set; } // Can be null, so use object
        public string Status { get; set; }
        public object TaxAmount { get; set; } // Can be null, so use object
        public List<object> Taxes { get; set; } // Empty array, so use List<object>
        public string Checkout_url { get; set; }
        public string ReferenceNumber { get; set; }
        public long CreatedAt { get; set; } // Assuming CreatedAt is a Unix timestamp (long)
        public long UpdatedAt { get; set; } // Assuming UpdatedAt is a Unix timestamp (long)
        public List<object> Payments { get; set; } // Empty array, so use List<object>
    }
}
