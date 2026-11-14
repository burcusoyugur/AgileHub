# AgileHub

Yazılım ekiplerinin projelerini çevik (Agile / Scrum) yöntemle planlayıp takip edebilmesi için geliştirilen web tabanlı proje yönetim portalı. Ekipler projelerini, sprintlerini ve görevlerini tek yerden yönetir; görevler Kanban panosunda takip edilir.

BLM4581 Ağ Tabanlı Teknolojiler ve Uygulamaları dersi dönem projesidir (2026–2027 Güz).

## Ekip

| Ad Soyad | GitHub |
| --- | --- |
| Gözde Sarı | [@gozdesari](https://github.com/gozdesari) |
| Şahyan Burcu Soyuğur | [@burcusoyugur](https://github.com/burcusoyugur) |
| Esra Meriç Topaktaş | [@esrameric](https://github.com/esrameric) |
| Erkim Berk Ünsal | [@erkimberk](https://github.com/erkimberk) |

## Kullanılan teknolojiler

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core (Code First, migration)
- Microsoft SQL Server (geliştirmede LocalDB)
- ASP.NET Core Identity (giriş ve roller)
- Swagger (Web API testi)
- Bootstrap 5

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB: Visual Studio kurulumunda **"ASP.NET ve web geliştirme"** iş yüküyle birlikte gelir. Kurulu olduğunu `sqllocaldb info` komutuyla kontrol edebilirsiniz (`MSSQLLocalDB` yazmalı).
- EF Core komut satırı aracı:
  ```
  dotnet tool install --global dotnet-ef
  ```
- (İsteğe bağlı) Veritabanını incelemek için SQL Server Management Studio (SSMS). Bağlanırken sunucu adı: `(localdb)\MSSQLLocalDB`

## Kurulum ve çalıştırma

Aşağıdaki komutlar, projeyi ilk kez kuran biri için sırasıyla yapılacak adımlardır.

**1. Depoyu klonlayın ve proje klasörüne girin**

```
git clone https://github.com/burcusoyugur/AgileHub.git
cd AgileHub/AgileHub
```

> Depoda iç içe iki `AgileHub` klasörü vardır. `dotnet` komutları, `AgileHub.csproj` dosyasının bulunduğu **içteki** klasörde çalıştırılmalıdır.

**2. Veritabanını oluşturun (migration'ları uygulayın)**

```
dotnet ef database update
```

Bu komut `appsettings.json` içindeki bağlantı cümlesini kullanarak LocalDB'de `AgileHubDb` veritabanını ve tüm tabloları oluşturur.

**3. Yönetici hesabının şifresini ayarlayın (User Secrets)**

Şifreler depoya konmaz. Her geliştirici varsayılan yönetici hesabının şifresini kendi bilgisayarında ayarlar:

```
dotnet user-secrets set "SeedAdmin:Password" "BirSifre123"
```

Şifre en az 6 karakter olmalı ve en az bir rakam içermelidir. Bu ayar proje klasörünün dışında, kullanıcının kendi bilgisayarında saklanır.

JWT imza anahtarı da aynı şekilde ayarlanır. En az 32 karakter olmalıdır; ayarlanmazsa uygulama açılmaz:

```
dotnet user-secrets set "Jwt:Key" "en-az-32-karakterlik-rastgele-bir-anahtar"
```

**4. Uygulamayı çalıştırın**

```
dotnet run
```

Terminalde yazan adresi (ör. `http://localhost:5203`) tarayıcıda açın. API uç noktaları geliştirme ortamında `/swagger` adresinden test edilebilir.

## Mobil kimlik doğrulama API'si

Mobil uygulama JWT ile giriş yapar; web sayfaları cookie ile çalışmaya devam eder. Korunan uçlara `Authorization: Bearer <token>` başlığıyla istek atılır. Swagger'da **Authorize** düğmesine token'ı yapıştırmak yeterlidir.

| Uç | Başarılı | Hata |
| --- | --- | --- |
| `POST /api/auth/login` | `200`, token yanıtı | `401` hatalı giriş veya kilitli hesap, `400` geçersiz istek |
| `POST /api/auth/register` | `200`, token yanıtı | `409` e-posta kayıtlı, `400` geçersiz istek veya şifre kurala uymuyor |
| `GET /api/auth/me` | `200`, kullanıcı | `401` token yok, geçersiz veya süresi dolmuş |

Giriş isteği:

```json
{ "email": "test@agilehub.local", "password": "BirSifre123" }
```

Kayıt isteği (son kelime soyad olarak kaydedilir):

```json
{ "fullName": "Ayşe Nur Yılmaz", "email": "ayse@ornek.com", "password": "BirSifre123" }
```

Token yanıtı:

```json
{
  "token": "<jwt>",
  "expiresAt": "2026-11-02T10:30:00Z",
  "user": { "id": "<guid>", "email": "ayse@ornek.com", "ad": "Ayşe Nur", "soyad": "Yılmaz" }
}
```

## Web API ve Swagger

Mobil uygulama `/api/...` uçlarını kullanır. Bu uçlar REST kurallarına göre yazılmıştır: adresler kaynağı gösterir (`/api/projeler/5/sprintler`), yapılacak işi HTTP metodu söyler (GET okur, POST ekler, PUT günceller, PATCH kısmen günceller, DELETE siler), sonuç durum koduyla bildirilir. Web sayfaları (`/Proje`, `/Gorev`...) API değildir; onlar HTML döndüren MVC tarafıdır.

### Swagger ile deneme

1. `dotnet run --launch-profile http` ile uygulamayı başlatın ve `http://localhost:5203/swagger` adresini açın (yalnızca geliştirme ortamında açıktır).
2. `POST /api/auth/login` → **Try it out** → e-posta ve şifreyi yazıp **Execute**. Yanıttaki `token` değerini kopyalayın.
3. Sağ üstteki **Authorize** düğmesine token'ı yapıştırın. Sonraki isteklere `Authorization: Bearer <token>` başlığı eklenir.

Uç açıklamaları controller'lardaki `/// <summary>` yorumlarından gelir (`AgileHub.csproj` içindeki `GenerateDocumentationFile` ayarı ve `Program.cs` içindeki `IncludeXmlComments` satırı). Yeni bir uç eklerken üstüne `/// <summary>` ve olası durum kodları için `[ProducesResponseType]` yazın; örnek kalıp `AuthApiController` içindedir.

### Uçlar

| Uç | Başarılı | Hata |
| --- | --- | --- |
| `GET /api/projeler` | `200` üyesi olunan projeler | `401` |
| `GET /api/projeler/{id}` | `200` proje | `401`, `403` üye değil, `404` |
| `GET /api/projeler/{projeId}/uyeler` | `200` üyeler | `401`, `403`, `404` |
| `GET /api/projeler/{projeId}/sprintler` | `200` sprintler | `401`, `403`, `404` |
| `GET /api/sprintler/{id}` | `200` sprint ve story point toplamları | `401`, `403`, `404` |
| `GET /api/sprintler/{id}/gorevler` | `200` sprintin görevleri | `401`, `403`, `404` |
| `GET /api/projeler/{projeId}/gorevler` | `200` projenin görevleri | `400` geçersiz filtre, `401`, `403`, `404` |
| `GET /api/gorevler/{id}` | `200` görev | `401`, `403`, `404` |
| `POST /api/projeler/{projeId}/gorevler` | `201` + `Location` başlığı | `400`, `401`, `403`, `404` |
| `PUT /api/gorevler/{id}` | `200` güncel görev | `400`, `401`, `403`, `404` |
| `PATCH /api/gorevler/{id}/sprint` | `200` güncel görev (`sprintId: null` → backlog) | `400`, `401`, `403`, `404` |
| `DELETE /api/gorevler/{id}` | `204` | `401`, `403`, `404` |

Hata yanıtları ProblemDetails biçimindedir:

```json
{ "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4", "title": "Bu kayda erişim yetkin yok", "status": 403 }
```

Enum alanları sayı değil ad olarak gönderilir ve döner: `"oncelik": "Yuksek"`, `"durum": "Yapiliyor"`.

### `.http` dosyasıyla test

`AgileHub/AgileHub.http` bütün test isteklerini içerir. VS Code'da **REST Client** eklentisiyle (veya Visual Studio'da) açılır; her isteğin üstündeki **Send Request** ile gönderilir. Giriş isteklerinin döndürdüğü token'lar sonraki isteklere kendiliğinden aktarılır.

Şifre dosyaya yazılmaz. `AgileHub/AgileHub/.env` dosyası oluşturup içine `SEED_PASSWORD=<yönetici şifresi>` yazın; `.env` dosyaları `.gitignore`'dadır. `@projeId` değişkenine yöneticinin üyesi olduğu, test kullanıcısının üyesi olmadığı bir projenin kimliğini yazın.

### Test sonuçları

14 Kasım 2026, `main` (44a50f8) + `hafta4/swagger`, yerel veritabanı. Kullanıcılar: `admin@agilehub.local` (proje 2'nin üyesi), `test@agilehub.local` (hiçbir projenin üyesi değil).

| # | İstek | Senaryo | Beklenen | Sonuç |
| --- | --- | --- | --- | --- |
| 1.1 | `POST /api/auth/login` | Doğru bilgiler | 200 + token | ✅ 200 |
| 1.3 | `POST /api/auth/login` | Hatalı şifre | 401 | ✅ 401 |
| 1.4 | `POST /api/auth/login` | Geçersiz e-posta biçimi | 400 | ✅ 400 (mesaj İngilizce, aşağıya bakın) |
| 1.5 | `POST /api/auth/register` | Kayıtlı e-posta | 409 | ✅ 409 |
| 1.6 | `POST /api/auth/register` | Şifre 3 karakter | 400 + Türkçe hata | ✅ 400 "Şifre en az 6 karakter…" |
| 1.7 | `GET /api/auth/me` | Token ile | 200 + kullanıcı | ✅ 200 |
| 1.8 | `GET /api/auth/me` | Token yok | 401, yönlendirme yok | ✅ 401 |
| 1.9 | `GET /api/auth/me` | Bozuk token | 401 | ✅ 401 |
| 2.1 | `GET /api/projeler` | Admin | 200 + liste | ✅ 200, 1 proje |
| 2.1 | `GET /api/projeler` | Test kullanıcısı | 200 + boş liste | ✅ 200, `[]` |
| 2.2 | `GET /api/projeler` | Token yok | 401 | ✅ 401 |
| 2.3 | `GET /api/projeler/2` | Üye | 200 | ✅ 200 |
| 2.4 | `GET /api/projeler/2` | Üye değil | 403 | ✅ 403 |
| 2.5 | `GET /api/projeler/999999` | Olmayan proje | 404 | ✅ 404 |
| 2.6 | `GET /api/projeler/2/uyeler` | Üye | 200 | ✅ 200 |
| 2.7 | `GET /api/projeler/2/uyeler` | Üye değil | 403 | ✅ 403 |
| 3.1 | `GET /api/projeler/2/sprintler` | Üye | 200 | ✅ 200 |
| 3.2 | `GET /api/projeler/2/sprintler` | Üye değil | 403 | ✅ 403 |
| 3.3 | `GET /api/projeler/999999/sprintler` | Olmayan proje | 404 | ✅ 404 |
| 3.4 | `GET /api/sprintler/999999` | Olmayan sprint | 404 | ✅ 404 |
| 3.5 | `GET /api/sprintler/999999/gorevler` | Olmayan sprint | 404 | ✅ 404 |
| 4.1 | `GET /api/projeler/2/gorevler` | Üye | 200 | ✅ 200 |
| 4.2 | `GET /api/projeler/2/gorevler` | Üye değil | 403 | ✅ 403 |
| 4.3 | `GET /api/gorevler/999999` | Olmayan görev | 404 | ✅ 404 |
| 5.x | Görev ekle / güncelle / sprinte ata / sil | `hafta4/api-gorev-yazma` | 201 / 200 / 200 / 204, tekrar silme 404 | ⏳ dal birleşince (17 Kasım test turu) |

Swagger kontrolleri: yalnızca `/api` uçları listeleniyor ✅, enum'lar ad olarak görünüyor ✅, uç açıklamaları ve olası durum kodları görünüyor ✅, Authorize düğmesi çalışıyor ✅.

Bulunan küçük sorunlar (uçların sahiplerine iletildi):

- Token yokken veya geçersizken dönen `401` yanıtının gövdesi boş; Swagger'da ise ProblemDetails gövdesi gösteriliyor. Mobil taraf yalnızca durum koduna baktığı için çalışmayı bozmuyor.
- `POST /api/auth/login` doğrulama hataları İngilizce ("The Email field is not a valid e-mail address."); `GirisIstegi` ve `KayitIstegi` kayıtlarında Türkçe `ErrorMessage` yok.
- `401` (hatalı giriş) ve `409` (kayıtlı e-posta) yanıtlarının `title` alanı İngilizce ("Unauthorized", "Conflict"), `detail` yok. Mobil uygulama bu kodları kendi Türkçe mesajına çeviriyor.

## Başlangıç verileri (seed)

Uygulama her açılışta `Data/SeedData.cs` içindeki kodu çalıştırır ve eksik olan başlangıç verilerini ekler (var olanları tekrar eklemez):

| Veri | Açıklama |
| --- | --- |
| Roller | `Yonetici`, `Kullanici` |
| Yönetici hesabı | E-posta: `admin@agilehub.local`, şifre: 3. adımda ayarladığınız şifre. Yonetici rolüne atanır. |
| Test kullanıcısı | Yalnızca geliştirme ortamında. E-posta: `test@agilehub.local`, şifre: yönetici şifresiyle aynı. Kullanici rolüne atanır. |
| Etiketler | Hata, Özellik, İyileştirme |

Şifre ayarlanmamışsa yönetici hesabı oluşturulmaz ve terminalde bir uyarı görünür.

## Yeni bir migration eklendiğinde

Modelde değişiklik yapan kişi migration oluşturur:

```
dotnet ef migrations add <MigrationAdi>
```

Diğer üyeler `git pull` sonrasında `Migrations` klasörüne yeni dosya geldiyse veritabanlarını günceller:

```
dotnet ef database update
```

## Commit kuralları

Commit mesajları [Conventional Commits](https://www.conventionalcommits.org/tr/v1.0.0/) yapısını izler, açıklamalar Türkçe yazılır:

```
<tür>(haftaN/<kapsam>): <kısa açıklama>
```

| Tür | Ne zaman |
| --- | --- |
| `feat` | Yeni özellik |
| `fix` | Hata düzeltme |
| `refactor` | Davranışı değiştirmeden kod düzenleme |
| `docs` | README ve belgeler |
| `test` | Test ekleme veya düzeltme |
| `chore` | Paket, ayar, seed gibi yan işler |

- Kapsamlar: `hesap`, `proje`, `uye`, `sprint`, `gorev`, `kanban`, `api`, `yonetici`, `db`
- Hafta numarası plan belgesindeki haftadır. Kapsamı olmayan genel işlerde yalnızca hafta yazılır.
- Açıklama küçük harfle başlar, emir kipiyle yazılır ("eklendi" değil "ekle") ve sonunda nokta olmaz.
- İlk satır 72 karakteri geçmez.
- Her commit tek bir iş yapar ve proje o haliyle derlenir.

```
feat(hafta2/hesap): e-posta ve şifre ile girişi ekle
fix(hafta2/uye): aynı kullanıcının iki kez eklenmesini engelle
docs(hafta2): commit kurallarını README'ye ekle
```

## Proje yapısı

| Klasör / dosya | İçerik |
| --- | --- |
| `Models/` | Varlık sınıfları (Proje, Sprint, Gorev, Yorum, Etiket...) |
| `Data/` | `ApplicationDbContext` ve başlangıç verileri (`SeedData`) |
| `Migrations/` | EF Core migration dosyaları |
| `Controllers/` | MVC ve API controller'ları |
| `Services/` | Yardımcı servisler (`JwtServisi`) |
| `Views/` | Razor sayfaları |
| `appsettings.json` | Uygulama ayarları ve veritabanı bağlantı cümlesi |
| `Program.cs` | Uygulamanın başlangıç noktası ve servis ayarları |
| `AgileHub.http` | API test istekleri (VS Code REST Client) |
