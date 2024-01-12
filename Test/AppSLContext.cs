using SAPEntityFramework;

namespace Test
{
    internal class AppSLContext : SLContext
    {
        public AppSLContext(SLContextOptions options) : base(options) { }
    }
}
