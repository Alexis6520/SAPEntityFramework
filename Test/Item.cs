using System.ComponentModel.DataAnnotations;

namespace Test
{
    public class Item
    {
        [Key]
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string BarCode { get; set; }
        public string U_SATCLAVEUNIDADARTI { get; set; }
        public string U_SATCLAVEARTICULO { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
