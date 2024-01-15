using SAPEntityFramework;
using SAPEntityFramework.Extensions.Queryable;
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
var item = await slContext.Items.Where(x => x.ItemCode == "B-T203-T/O" && x.ItemName!="a").ToListAsync();
var partner =await slContext.BusinessPartners.Where(x => x.CardCode == "00110").FirsttOrDefaultAsync();
stopWatch.Stop();
Console.Write($"Prueba terminada en {stopWatch.Elapsed.TotalSeconds} segundos");
Console.ReadLine();