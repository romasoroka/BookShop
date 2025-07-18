using Microsoft.EntityFrameworkCore;
using WebSite.Data;
using WebSite.DataAccess.Repository;
using WebSite.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using WebSite.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);
string BlobConnectionString = builder.Configuration.GetSection("AzureBlobSettings:ConnectionString").Value;
string BlobContainerName = builder.Configuration.GetSection("AzureBlobSettings:ContainerName").Value;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRazorPages();

builder.Services.AddSingleton(x =>
{
    var blobServiceClient = new BlobServiceClient(BlobConnectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);
    _ = containerClient.CreateIfNotExistsAsync();
    return containerClient;
});


builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for p roduction scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapStaticAssets();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
