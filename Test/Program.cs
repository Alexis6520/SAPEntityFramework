﻿using SAPSLFramework;
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
await context.Items.DeleteAsync(requestedItem);
Console.WriteLine();