using AgileHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Data
{
    // Gösterim ve testler için örnek proje ortamı. Yalnızca geliştirme ortamında (bkz. Program.cs)
    // ve veritabanında hiç proje yokken çalışır; elle girilen veriye dokunmaz.
    // Projeler ve sprintler mobildeki SampleData ile aynı sırada eklenir (temiz veritabanında id'ler de aynı olur);
    // tarihler bugüne göre hesaplanır.
    public static class DemoVerisi
    {
        // Görev tanımı. Atanan: kullanıcı anahtarı (bkz. kullanicilar), null ise atanmamış.
        private record GorevTanimi(
            string Baslik,
            GorevOnceligi Oncelik,
            int? StoryPoint,
            string? Atanan,
            GorevDurumu Durum = GorevDurumu.Yapilacak,
            string? Aciklama = null,
            params string[] Etiketler);

        private record SprintTanimi(
            string Ad,
            string? Hedef,
            int BaslangicGun,
            int BitisGun,
            SprintDurumu Durum,
            GorevTanimi[] Gorevler);

        private record ProjeTanimi(
            string Ad,
            string Aciklama,
            int OlusturmaGun,
            string[] Uyeler,
            SprintTanimi[] Sprintler,
            GorevTanimi[] Backlog);

        private static readonly (string Anahtar, string Email, string Ad, string Soyad)[] EkipUyeleri =
        [
            ("esra", "esra@agilehub.local", "Esra Meriç", "Topaktaş"),
            ("erkim", "erkim@agilehub.local", "Erkim Berk", "Ünsal"),
            ("gozde", "gozde@agilehub.local", "Gözde", "Sarı"),
            ("burcu", "burcu@agilehub.local", "Burcu", "Soyuğur")
        ];

        private static readonly string[] Ekip = ["admin", "esra", "erkim", "gozde", "burcu"];

        // Gün değerleri DateTime.Today'e eklenir: aktif sprintler bugünü kapsar,
        // tamamlananlar geçmişte, planlananlar gelecektedir.
        private static readonly ProjeTanimi[] Projeler =
        [
            new("AgileHub Mobil", "Çevik proje takip uygulamasının Flutter istemcisi", -36, Ekip,
            [
                new("Sprint 1", "Proje iskeleti, tema ve yönlendirme", -33, -20, SprintDurumu.Tamamlandı,
                [
                    new("Flutter proje iskeletini oluştur", GorevOnceligi.Yuksek, 5, "erkim", GorevDurumu.Bitti),
                    new("Uygulama temasını ve renk paletini belirle", GorevOnceligi.Orta, 3, "burcu", GorevDurumu.Bitti),
                    new("Sayfa yönlendirmesini kur", GorevOnceligi.Orta, 5, "gozde", GorevDurumu.Bitti)
                ]),
                new("Sprint 2", "Giriş, kayıt ve oturum yönetimi", -19, -6, SprintDurumu.Tamamlandı,
                [
                    new("Giriş ekranını tasarla", GorevOnceligi.Yuksek, 5, "esra", GorevDurumu.Bitti),
                    new("Kayıt ekranında form doğrulaması", GorevOnceligi.Orta, 5, "burcu", GorevDurumu.Bitti),
                    new("JWT token'ını güvenli depolamada sakla", GorevOnceligi.Kritik, 8, "erkim", GorevDurumu.Bitti, null, "Özellik")
                ]),
                new("Sprint 3", "Proje ekranları ve servis katmanı", -5, 9, SprintDurumu.Aktif,
                [
                    new("Projelerim ekranını API'ye bağla", GorevOnceligi.Yuksek, 5, "admin", GorevDurumu.Yapiliyor,
                        "Sahte servis yerine ApiProjectService kullanılsın; yükleniyor ve hata durumları gösterilsin.", "Özellik"),
                    new("Proje detay ekranı", GorevOnceligi.Orta, 5, "esra", GorevDurumu.Bitti),
                    new("Sprint listesinde ilerleme çubuğu", GorevOnceligi.Orta, 3, "gozde", GorevDurumu.Bitti),
                    new("Giriş ekranında şifre doğrulaması", GorevOnceligi.Yuksek, 5, "admin", GorevDurumu.Test,
                        "Şifre en az 6 karakter ve bir rakam içermeli; hata mesajı alanın altında Türkçe görünsün.", "Hata"),
                    new("API hata mesajlarını kullanıcıya göster", GorevOnceligi.Orta, 3, "burcu", GorevDurumu.Yapilacak, null, "İyileştirme")
                ]),
                new("Sprint 4", "Ekranların gerçek API'ye bağlanması", 10, 23, SprintDurumu.Planlanıyor,
                [
                    new("Görev panosu (Kanban) ekranı", GorevOnceligi.Yuksek, 8, "erkim"),
                    new("Görev detay ekranı", GorevOnceligi.Orta, 5, "gozde")
                ])
            ],
            [
                new("Çevrimdışı modda son verileri göster", GorevOnceligi.Yuksek, 8, "admin"),
                new("Karanlık tema desteği", GorevOnceligi.Dusuk, 3, null, GorevDurumu.Yapilacak, null, "İyileştirme"),
                new("Bildirim izni ekranı", GorevOnceligi.Dusuk, null, "esra")
            ]),

            new("AgileHub Web", "ASP.NET Core ile proje yönetim portalı ve ortak Web API", -38, Ekip,
            [
                new("Sprint 1", "Veritabanı, kimlik doğrulama ve projeler", -26, -6, SprintDurumu.Tamamlandı,
                [
                    new("Veritabanı modelleri ve ilk migration", GorevOnceligi.Kritik, 8, "erkim", GorevDurumu.Bitti),
                    new("Identity ile kayıt ve giriş", GorevOnceligi.Yuksek, 5, "esra", GorevDurumu.Bitti),
                    new("Proje listeleme ve detay sayfaları", GorevOnceligi.Orta, 5, "gozde", GorevDurumu.Bitti),
                    new("Menüye Projelerim bağlantısı ekle", GorevOnceligi.Dusuk, 2, "burcu", GorevDurumu.Bitti)
                ]),
                new("Sprint 2", "Backlog ve sprintler", -5, 9, SprintDurumu.Aktif,
                [
                    new("Backlog sayfası ve görev filtreleri", GorevOnceligi.Yuksek, 8, "esra", GorevDurumu.Bitti),
                    new("Sprint başlatma ve tamamlama", GorevOnceligi.Yuksek, 3, "erkim", GorevDurumu.Bitti),
                    new("Sprint planlama ekranı", GorevOnceligi.Yuksek, 5, "gozde", GorevDurumu.Test, null, "Özellik"),
                    new("Ana sayfa panosu", GorevOnceligi.Orta, 5, "admin", GorevDurumu.Yapiliyor,
                        "Giriş yapan kullanıcıya özet kartlar, aktif sprintleri ve kendisine atanan görevler gösterilsin."),
                    new("Sprint burndown grafiği", GorevOnceligi.Orta, 5, "burcu", GorevDurumu.Yapilacak, null, "Özellik")
                ]),
                // İptal edilen sprintin görevleri backlog'a dönmüştür.
                new("Raporlama denemesi", null, -16, -13, SprintDurumu.İptalEdildi, [])
            ],
            [
                new("Görev yorumlarını detay sayfasında göster", GorevOnceligi.Orta, 3, "admin", GorevDurumu.Yapilacak, null, "Özellik"),
                new("Proje silinirken üyelere e-posta gönder", GorevOnceligi.Dusuk, 2, null)
            ]),

            // Sprinti olmayan proje: boş durum ekranlarını göstermek için.
            new("Kütüphane Otomasyonu", "Ödünç alma ve iade takibi için örnek proje", -11, ["admin", "test"],
            [],
            [
                new("Kitap ekleme formu", GorevOnceligi.Yuksek, 5, "test"),
                new("Ödünç alma kaydı", GorevOnceligi.Orta, 3, "admin"),
                new("Gecikme bildirimi", GorevOnceligi.Dusuk, 2, null, GorevDurumu.Yapilacak, null, "Özellik")
            ])
        ];

        // Aktif sprint görevlerine farklı kişilerden yorumlar: (görev başlığı, yazan, metin, kaç gün önce).
        private static readonly (string Gorev, string Yazan, string Metin, int GunOnce)[] Yorumlar =
        [
            ("Projelerim ekranını API'ye bağla", "erkim", "Listeleme uçları main'de, /api/projeler en yeni projeyi önce döndürüyor.", 3),
            ("Projelerim ekranını API'ye bağla", "admin", "Teşekkürler, yarın gerçek veriyle deniyorum.", 2),
            ("Giriş ekranında şifre doğrulaması", "burcu", "Boş şifrede hata mesajı iki kez görünüyor, kontrol edebilir misin?", 1),
            ("Sprint planlama ekranı", "esra", "Toplu ekleme çalışıyor; tamamlanmış sprinte eklemeyi engellediğini de denedim.", 2),
            ("Ana sayfa panosu", "gozde", "Aktif sprint kartına kalan gün sayısını da ekleyelim mi?", 1)
        ];

        public static async Task EkleAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DemoVerisi");

            // Veritabanında proje varsa (önceki açılış ya da elle girilen veri) hiçbir şey eklenmez.
            if (await db.Projeler.AnyAsync())
            {
                return;
            }

            var sifre = config["SeedAdmin:Password"];

            if (string.IsNullOrEmpty(sifre))
            {
                logger.LogWarning("SeedAdmin:Password ayarlanmamış, demo verisi eklenmedi. README'ye bakın.");
                return;
            }

            var adminEmail = config["SeedAdmin:Email"] ?? "admin@agilehub.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            var test = await userManager.FindByEmailAsync("test@agilehub.local");

            if (admin is null || test is null)
            {
                logger.LogWarning("Yönetici veya test kullanıcısı bulunamadı, demo verisi eklenmedi.");
                return;
            }

            var kullanicilar = new Dictionary<string, ApplicationUser>
            {
                ["admin"] = admin,
                ["test"] = test
            };

            foreach (var (anahtar, email, ad, soyad) in EkipUyeleri)
            {
                var kullanici = await userManager.FindByEmailAsync(email);

                if (kullanici is null)
                {
                    kullanici = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        Ad = ad,
                        Soyad = soyad,
                        EmailConfirmed = true
                    };

                    var sonuc = await userManager.CreateAsync(kullanici, sifre);
                    if (!sonuc.Succeeded)
                    {
                        logger.LogError("Demo kullanıcısı {Email} oluşturulamadı: {Hatalar}",
                            email, string.Join(", ", sonuc.Errors.Select(e => e.Description)));
                        return;
                    }

                    await userManager.AddToRoleAsync(kullanici, Roller.Kullanici);
                }

                kullanicilar[anahtar] = kullanici;
            }

            var etiketler = await db.Etiketler.ToDictionaryAsync(e => e.Ad);
            var bugun = DateTime.Today;
            var gorevler = new Dictionary<string, Gorev>();

            foreach (var tanim in Projeler)
            {
                var proje = new Proje
                {
                    Ad = tanim.Ad,
                    Aciklama = tanim.Aciklama,
                    OlusturmaTarihi = bugun.AddDays(tanim.OlusturmaGun).AddHours(10),
                    OlusturanKullaniciId = admin.Id
                };

                foreach (var uye in tanim.Uyeler)
                {
                    proje.ProjeUyeleri.Add(new ProjeUyesi
                    {
                        KullaniciId = kullanicilar[uye].Id,
                        KatilmaTarihi = proje.OlusturmaTarihi
                    });
                }

                foreach (var sprintTanimi in tanim.Sprintler)
                {
                    var sprint = new Sprint
                    {
                        Ad = sprintTanimi.Ad,
                        Hedef = sprintTanimi.Hedef,
                        BaslangicTarihi = bugun.AddDays(sprintTanimi.BaslangicGun),
                        BitisTarihi = bugun.AddDays(sprintTanimi.BitisGun),
                        Durum = sprintTanimi.Durum
                    };
                    proje.Sprintler.Add(sprint);

                    // Görevler sprint başlamadan önceki gün eklenmiş sayılır.
                    var olusturma = sprint.BaslangicTarihi.AddDays(-1).AddHours(9);
                    foreach (var gorevTanimi in sprintTanimi.Gorevler)
                    {
                        var gorev = GorevOlustur(gorevTanimi, olusturma, kullanicilar, etiketler);
                        gorev.Sprint = sprint;
                        proje.Gorevler.Add(gorev);
                        gorevler[gorevTanimi.Baslik] = gorev;
                        olusturma = olusturma.AddMinutes(30);
                    }
                }

                var backlogOlusturma = bugun.AddDays(-3).AddHours(14);
                foreach (var gorevTanimi in tanim.Backlog)
                {
                    var gorev = GorevOlustur(gorevTanimi, backlogOlusturma, kullanicilar, etiketler);
                    proje.Gorevler.Add(gorev);
                    gorevler[gorevTanimi.Baslik] = gorev;
                    backlogOlusturma = backlogOlusturma.AddHours(5);
                }

                db.Projeler.Add(proje);
            }

            foreach (var (gorevBasligi, yazan, metin, gunOnce) in Yorumlar)
            {
                gorevler[gorevBasligi].Yorumlar.Add(new Yorum
                {
                    Metin = metin,
                    YazanKullaniciId = kullanicilar[yazan].Id,
                    OlusturmaTarihi = bugun.AddDays(-gunOnce).AddHours(11)
                });
            }

            await db.SaveChangesAsync();

            logger.LogInformation("Demo verisi eklendi: {Proje} proje, {Gorev} görev.", Projeler.Length, gorevler.Count);
        }

        private static Gorev GorevOlustur(
            GorevTanimi tanim,
            DateTime olusturma,
            Dictionary<string, ApplicationUser> kullanicilar,
            Dictionary<string, Etiket> etiketler)
        {
            var gorev = new Gorev
            {
                Baslik = tanim.Baslik,
                Aciklama = tanim.Aciklama,
                Oncelik = tanim.Oncelik,
                Durum = tanim.Durum,
                StoryPoint = tanim.StoryPoint,
                AtananKullaniciId = tanim.Atanan is null ? null : kullanicilar[tanim.Atanan].Id,
                OlusturmaTarihi = olusturma
            };

            // Seed'de olmayan bir etiket adı sessizce atlanır.
            foreach (var etiketAdi in tanim.Etiketler)
            {
                if (etiketler.TryGetValue(etiketAdi, out var etiket))
                {
                    gorev.GorevEtiketler.Add(new GorevEtiket { Etiket = etiket });
                }
            }

            return gorev;
        }
    }
}
