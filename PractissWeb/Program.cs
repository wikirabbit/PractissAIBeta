using PractissWeb.Services;
using PractissWeb.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

// Register the Markdown service
builder.Services.AddSingleton<MarkdownService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(240);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure Api Url
if (app.Environment.IsDevelopment())
{
    PractissApiClientLibrary.ApiUrl = "https://localhost:7228/api/";
}
else
{  
	PractissApiClientLibrary.ApiUrl = Environment.GetEnvironmentVariable("PractissApiUrl");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
  endpoints.MapRazorPages(); // Map Razor Pages
  endpoints.MapControllerRoute( // Map MVC controllers
      name: "default",
      pattern: "{controller=Home}/{action=Index}/{id?}");
});

// DataAccess.CosmosDbService.Init();

app.Run();

