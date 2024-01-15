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
var item = await slContext.Items.Where(x => x.ItemCode == "TEST_0011").FirstOrDefaultAsync();
item.ItemName = "Prueba actualización";
await slContext.Items.UpdateAsync(item);
stopWatch.Stop();
Console.Write($"Prueba terminada en {stopWatch.Elapsed.TotalSeconds} segundos");
Console.ReadLine();