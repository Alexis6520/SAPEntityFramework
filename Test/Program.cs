using SAPEntityFramework;
using SAPEntityFramework.Extensions;
using System.Diagnostics;
using Test;

var options = new SLContextOptions()
{
    Url = "https://10.10.50.112:50000/b1s/v2/",
    UserName = "manager",
    Password = "nueva2017",
    Language = 25,
    CompanyDB = "SAP_PRUEBAS"
};

using var slContext = new AppSLContext(options);
var stopWatch = new Stopwatch();
stopWatch.Start();
string a = "P0000000010";
var b = new BusinessPartner { CardCode = "P0000000010" };
var item = await slContext.BusinessPartners.Where(x => x.CardCode == "P0000000010").GetFirstAsync();
stopWatch.Stop();
Console.Write($"Prueba terminada en {stopWatch.Elapsed.TotalSeconds} segundos");