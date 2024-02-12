using SAPSLFramework;
using SAPSLFramework.Extensions;
using Test;

var options = new SLContextOptions()
{
    Url = "https://10.10.50.112:50000/b1s/v2",
    UserName = "manager",
    Password = "nueva2017",
    Language = 25,
    CompanyDB = "SAP_PRUEBAS_48"
};

using var slContext = new AppSLContext(options);
var item =await slContext.Items.GetFirstAsync();
Console.WriteLine("");