using AgileHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Data
{
    // Rol adlarını tek bir yerde tutuyoruz.
    public static class Roller
    {
        public const string Yonetici = "Yonetici";
        public const string Kullanici = "Kullanici";
    }

    public static class SeedData
    {
        public static async Task BaslangicVerileriniEkleAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();
            var ortam = services.GetRequiredService<IHostEnvironment>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

            // 1) Roller
            foreach (var rol in new[] { Roller.Yonetici, Roller.Kullanici })
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2) Yönetici hesabı
            var email = config["SeedAdmin:Email"] ?? "admin@agilehub.local";
            var sifre = config["SeedAdmin:Password"];

            if (string.IsNullOrEmpty(sifre))
            {
                logger.LogWarning("SeedAdmin:Password ayarlanmamış, yönetici hesabı oluşturulmadı. README'ye bakın.");
            }
            else if (await userManager.FindByEmailAsync(email) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Ad = "Sistem",
                    Soyad = "Yöneticisi",
                    EmailConfirmed = true
                };

                var sonuc = await userManager.CreateAsync(admin, sifre);
                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roller.Yonetici);
                }
                else
                {
                    logger.LogError("Yönetici oluşturulamadı: {Hatalar}",
                        string.Join(", ", sonuc.Errors.Select(e => e.Description)));
                }
            }

            // 3) Test kullanıcısı: üye ekleme gibi ikinci kullanıcı gereken işleri denemek için,
            // yalnızca geliştirme ortamında ve yönetici şifresiyle oluşturulur.
            const string testEmail = "test@agilehub.local";

            if (ortam.IsDevelopment() && !string.IsNullOrEmpty(sifre)
                && await userManager.FindByEmailAsync(testEmail) is null)
            {
                var testKullanici = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    Ad = "Test",
                    Soyad = "Kullanıcı",
                    EmailConfirmed = true
                };

                var sonuc = await userManager.CreateAsync(testKullanici, sifre);
                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(testKullanici, Roller.Kullanici);
                }
                else
                {
                    logger.LogError("Test kullanıcısı oluşturulamadı: {Hatalar}",
                        string.Join(", ", sonuc.Errors.Select(e => e.Description)));
                }
            }

            // 4) Etiketler
            if (!await db.Etiketler.AnyAsync())
            {
                db.Etiketler.AddRange(
                    new Etiket { Ad = "Hata", Renk = "#dc3545" },
                    new Etiket { Ad = "Özellik", Renk = "#0d6efd" },
                    new Etiket { Ad = "İyileştirme", Renk = "#198754" });

                await db.SaveChangesAsync();
            }
        }
    }
}