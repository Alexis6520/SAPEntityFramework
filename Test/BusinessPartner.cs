using System.ComponentModel.DataAnnotations;

namespace Test
{
    internal class BusinessPartner
    {
        [Key]
        public string CardCode { get; set; }
        public string CardName { get; set; }
    }
}
