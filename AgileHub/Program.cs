using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using AgileHub.Data;
using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddErrorDescriber<TurkceIdentityHataAciklayici>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Varsayılan şema web için Identity cookie'si olarak kalır; JWT yalnızca
// şemayı açıkça belirten API uçlarında kullanılır.
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<JwtServisi>();
builder.Services.AddScoped<ProjeYetkiServisi>();
builder.Services.AddScoped<GorevServisi>();
builder.Services.AddScoped<SprintServisi>();
builder.Services.AddScoped<SprintAtamaServisi>();
builder.Services.AddScoped<AnaSayfaServisi>();
builder.Services.AddScoped<ProjeServisi>();

// Enum'lar API'de sayı yerine ad olarak gönderilir (ör. "Yapiliyor").
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AgileHub API",
        Version = "v1",
        Description = "AgileHub mobil uygulamasının kullandığı REST API.\n\n" +
                      "1. `POST /api/auth/login` ile giriş yap ve yanıttaki `token` değerini kopyala.\n" +
                      "2. Sağ üstteki **Authorize** düğmesine token'ı yapıştır; sonraki isteklere `Authorization: Bearer` başlığı eklenir.\n\n" +
                      "Hatalar ProblemDetails biçiminde döner (`title`, `status`, `detail`). " +
                      "401: token yok veya süresi dolmuş, 403: projenin üyesi değilsin, 404: kayıt yok, 400: gönderilen bilgi geçersiz."
    });

    // Uçların üstündeki /// <summary> açıklamaları derlemenin ürettiği XML dosyasından okunur.
    var xmlDosyasi = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlDosyasi));

    // Gruplar controller adından "Api" eki atılarak adlandırılır: "ProjelerApi" yerine "Projeler".
    c.TagActionsBy(api => [api.ActionDescriptor.RouteValues["controller"]!.Replace("Api", string.Empty)]);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

var app = builder.Build();

// Kontrol burada yapılır: dotnet ef komutları Build'de durduğu için anahtar gerektirmez.
if (Encoding.UTF8.GetByteCount(builder.Configuration["Jwt:Key"] ?? string.Empty) < 32)
{
    throw new InvalidOperationException("Jwt:Key ayarlanmamış veya 32 bayttan kısa. README'ye bakın.");
}

using (var scope = app.Services.CreateScope())
{
    await SeedData.BaslangicVerileriniEkleAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgileHub API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
