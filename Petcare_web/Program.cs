using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Thêm đường dẫn tìm kiếm View cho khách hàng
        options.ViewLocationFormats.Add("/Views/Khachhang/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Khachhang/Shared/{0}.cshtml");

        // Thêm đường dẫn tìm kiếm View cho admin
        options.ViewLocationFormats.Add("/Views/Admin/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Admin/Shared/{0}.cshtml");
    });
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6); // Thời gian session tự hết hạn
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index"; // Chuyển về trang chủ thay vì Login
        options.ExpireTimeSpan = TimeSpan.FromHours(6); // Thời gian cookie hết hạn
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticationHandler>();
builder.Services.AddHttpClient("PetCareApiClient", client =>
{
    // Cấu hình địa chỉ cơ sở của API backend
    client.BaseAddress = new Uri("https://localhost:7053/"); // API Project port
})
.AddHttpMessageHandler<AuthenticationHandler>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
