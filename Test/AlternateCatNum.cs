using System.ComponentModel.DataAnnotations;

namespace Test
{
    public class AlternateCatNum
    {
        [Key]
        public string ItemCode { get; set; }
        [Key]
        public string CardCode { get; set; }
        [Key]
        public string Substitute { get; set; }
    }
}
