using SAPEntityFramework;

namespace Test
{
    internal class AppSLContext : SLContext
    {
        public AppSLContext(SLContextOptions options) : base(options) { }

        public SLSet<Item> Items { get; set; }
        public SLSet<BusinessPartner> BusinessPartners { get; set; }
    }
}
