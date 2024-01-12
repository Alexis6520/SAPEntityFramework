using SAPEntityFramework;
using Test;

var options = new SLContextOptions()
{
    Url = "https://10.10.50.112:50000",
    UserName = "manager",
    Password = "nueva2017",
    Language = 25,
    CompanyDB = "SAP_PRUEBAS"
};

using var slContext = new AppSLContext(options);
await slContext.LoginAsync();
await slContext.LoginAsync();
Console.WriteLine("Fin");