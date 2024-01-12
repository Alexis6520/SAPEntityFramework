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
var item = await slContext.Items.FindAsync("B-T203-T/O");
var partner = await slContext.BusinessPartners.FindAsync("B-T203-T/O");
Console.Write("Prueba terminada");
Console.ReadLine();