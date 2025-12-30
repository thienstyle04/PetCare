using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PetCare.Data;
using PetCare.Repositories;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Text.Json.Serialization;

// KHỞI TẠO SERILOG SỚM (EARLY INITIALIZATION)
// Mục đích: Bắt được log lỗi xảy ra trong quá trình khởi tạo Host/Builder.
Console.OutputEncoding = System.Text.Encoding.UTF8; // Đảm bảo console hiển thị tiếng Việt
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Mức log thấp nhất được ghi nhận
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Giảm thiểu log hệ thống
    .Enrich.FromLogContext() // Bổ sung ngữ cảnh (Controller Name, Action Name, v.v.)
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/application_log-.txt", 
        rollingInterval: RollingInterval.Day,
        encoding: System.Text.Encoding.UTF8) // Ghi log tiếng Việt ra file
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

   
    //TÍCH HỢP SERILOG VÀO HOST VÀ CẤU HÌNH SERVICES

    // Tích hợp Serilog vào hệ thống Logging của Builder
    builder.Logging.ClearProviders(); // Xóa các provider logging mặc định
    builder.Host.UseSerilog(Log.Logger); // Đăng ký Serilog với cấu hình đã tạo
    
    // Cấu hình Controllers với JSON Options để xử lý circular reference
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Giải quyết vấn đề circular reference
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            // Cho phép null values (optional)
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    // cors
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("https://localhost:7179") // <-- THAY XXXX bằng cổng của dự án Petcare_web
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
    // Cấu hình Database Context
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Cấu hình Identity
    builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // Đăng ký TẤT CẢ REPOSITORIES (Đảm bảo tất cả services tồn tại)
    builder.Services.AddScoped<ITokenRepositories, NguoiDungRepositories>();
    builder.Services.AddScoped<IDichVuRepositories, DichVuRepositories>();
    builder.Services.AddScoped<IKhachHangRepositories, KhachHangRepositories>();
    builder.Services.AddScoped<IThuCungRepositories, ThuCungRepositories>();
    builder.Services.AddScoped<ILichHenRepository, SQLLichHenRepository>();
    builder.Services.AddScoped<IThanhToanRepository, SQLThanhToanRepository>();
    builder.Services.AddScoped<IDanhGiaRepository, SQLDanhGiaRepository>();
    builder.Services.AddScoped<IImageRepository, LocalImageRepository>();
    builder.Services.AddScoped<INhanVienRepositories, NhanVienRepositories>();
    builder.Services.AddScoped<IHoSoDichVuRepositories, HoSoDichVuRepositories>();
    builder.Services.AddHttpContextAccessor();


    // ... (Các services khác) ...

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PetCare API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
    });

    // Cấu hình JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

    // Xây dựng App
    var app = builder.Build();

    // 3. TẠO ROLE THỦ CÔNG (Chạy một lần khi khởi động)
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        string[] roleNames = { "KHACHHANG", "NHANVIEN", "QUANTRI" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        //// ✅ Tạo tài khoản admin mặc định
        //string adminEmail = "admin@petcare.com";
        //string adminPassword = "Admin@123";

        //var adminUser = await userManager.FindByEmailAsync(adminEmail);
        //if (adminUser == null)
        //{
        //    var newAdmin = new IdentityUser
        //    {
        //        UserName = adminEmail,
        //        Email = adminEmail,
        //        EmailConfirmed = true
        //    };

        //    var result = await userManager.CreateAsync(newAdmin, adminPassword);
        //    if (result.Succeeded)
        //    {
        //        await userManager.AddToRoleAsync(newAdmin, "QUANTRI");

        //        try
        //        {
        //            var nguoiDung = new PetCare.Models.Domain.NguoiDung
        //            {
        //                IdentityUserId = newAdmin.Id,
        //                TenDangNhap = adminEmail,
        //                Email = adminEmail,
        //                VaiTro = "QUANTRI",
        //                TrangThai = true,
        //                NgayTao = DateTime.Now,
        //                NgayCapNhat = DateTime.Now,
        //                MatKhauHash = newAdmin.PasswordHash
        //            };

        //            dbContext.NguoiDung.Add(nguoiDung);
        //            await dbContext.SaveChangesAsync();

        //            Console.WriteLine("✅ Đã tạo bản ghi NguoiDung cho admin.");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"❌ Lỗi khi lưu NguoiDung: {ex.Message}");
        //        }

        //        Console.WriteLine("✅ Admin mặc định đã được tạo thành công!");
        //    }
        //    else
        //    {
        //        Console.WriteLine("❌ Không thể tạo admin: " +
        //            string.Join(", ", result.Errors.Select(e => e.Description)));
        //    }
        //}
        //else
        //{
        //    Console.WriteLine("ℹ️ Admin đã tồn tại, bỏ qua việc tạo mới.");
        //}
    }

    //CẤU HÌNH MIDDLEWARE VÀ CHẠY APP

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // ✅ CẤU HÌNH STATIC FILES CHO THƯ MỤC IMAGES
    var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");
    if (!Directory.Exists(imagesPath))
    {
        Directory.CreateDirectory(imagesPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesPath),
        RequestPath = "/Images"
    });

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}