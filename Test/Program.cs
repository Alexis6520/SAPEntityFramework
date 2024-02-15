using SAPSLFramework;
using Test;

var options = new SLContextOptions()
{
    Url = "https://10.10.50.112:50000/b1s/v1",
    UserName = "manager",
    Password = "nueva2017",
    Language = 25,
    CompanyDB = "SAP_PRUEBAS_48"
};

using var context = new AppSLContext(options);

var items = await context.Items.Top(10).ToListAsync();

var item = new Item
{
    ItemName = "PRUEBA DE SERVICE LAYER",
    ItemType = "itItems",
    U_SATCLAVEARTICULO = "15121501",
    U_SATCLAVEUNIDADARTI = "H87"
};

bool exists;
int i = 0;
var itemCode = string.Empty;

do
{
    i++;
    itemCode = $"PRUEBA{i}";
    exists = await context.Items.Where(x => x.ItemCode == itemCode).AnyAsync();
} while (exists);

item.ItemCode = itemCode;
item.BarCode = itemCode;
await context.Items.AddAsync(item);
var requestedItem = await context.Items.Where(x => x.ItemCode == item.ItemCode).FirstAsync();
requestedItem.BarCode = "SeCambio";
await context.Items.UpdateAsync(requestedItem);
await context.Items.ExecuteActionAsync(item, "Cancel");
await context.Items.DeleteAsync(requestedItem);
var a = await context.ExecuteActionAsync<List<Activity>>("ActivitiesService_GetActivityList");
var startsWithItems = await context.Items.Where(x => x.ItemCode.StartsWith("PRUEBA") || x.ItemName == "Llanta").Select(x => new Activity(x.ItemCode)).ToListAsync();
var endsWithItems = await context.Items.Where(x => x.ItemName.EndsWith("layer")).ToListAsync();
var containsItems = await context.Items.Where(x => x.ItemName.Contains("ba")).Select(x => new Activity { DocNum = x.ItemCode }).FirstAsync();
var endsAndSelectItems = await context.Items.Where(x => x.ItemName.EndsWith("layer")).Select(x => new Activity { DocNum = x.ItemName }).ToListAsync();
var itemNames = await context.Items.Where(x => x.ItemName.EndsWith("layer")).Select(x => x.ItemName).ToListAsync();
var dates = await context.Items.Where(x => x.CreateDate < DateTime.Now).Select(x => x.CreateDate).FirstAsync();
var orderByItems = await context.Items.Where(x => x.ItemCode.StartsWith("PRUEBA")).OrderBy(x => new { x.ItemCode, x.ItemName }).ToListAsync();
var orderSkip = await context.Items.Where(x => x.ItemCode.StartsWith("PRUEBA"))
    .OrderBy(x => new { x.ItemCode, x.ItemName })
    .Skip(7)
    .Top(10)
    .ToListAsync();

var countItems = await context.Items.Where(x => x.ItemName.Contains("ba")).CountAsync();
Console.WriteLine();