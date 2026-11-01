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
