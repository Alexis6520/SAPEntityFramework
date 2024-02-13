using SAPSLFramework;
using Test;

var options = new SLContextOptions()
{
    Url = "https://10.10.50.112:50000/b1s/v2",
    UserName = "manager",
    Password = "nueva2017",
    Language = 25,
    CompanyDB = "SAP_PRUEBAS_48"
};

using var context = new AppSLContext(options);
var items = await context.Items.Where(x => x.ItemCode == "AB-0464").ToListAsync();
var item = await context.Items.Where(x => x.ItemCode == "AB-0464").FirstAsync();
Console.WriteLine("");