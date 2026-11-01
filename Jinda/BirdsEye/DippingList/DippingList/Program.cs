using DippingList.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IDippingListStore>(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    string path = Path.Combine(environment.ContentRootPath,
        "App_Data", "shopping-list.json");
    return new JsonDippingListStore(path);
});

builder.Services.AddSingleton<DippingListService>(sp =>
    new DippingListService(
        sp.GetRequiredService<IDippingListStore>()));

var app = builder.Build();

_ = app.Services.GetRequiredService<DippingListService>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
