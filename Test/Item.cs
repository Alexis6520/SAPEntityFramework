using System.ComponentModel.DataAnnotations;

namespace Test
{
    public class Item
    {
        [Key]
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
    }
}
