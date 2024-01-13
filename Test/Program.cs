using SAPEntityFramework;
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
var item = slContext.Items.Where(x => x.ItemCode == "B-T203-T/O").ToList();
var partner = slContext.BusinessPartners.Where(x => x.CardCode == "00110").First();
stopWatch.Stop();
Console.Write($"Prueba terminada en {stopWatch.Elapsed.TotalSeconds} segundos");
Console.ReadLine();