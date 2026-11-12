using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    // API'nin döndürdüğü veriler. Entity'ler doğrudan döndürülmez: ApplicationUser
    // şifre özeti gibi alanlar taşır, gezinme özellikleri de JSON'da döngü oluşturur.
    // Alan adları modellerle aynıdır; hesaplanan alanlar ek olarak yer alır.

    public record KullaniciDto(string Id, string Email, string Ad, string Soyad);

    public record GirisIstegi(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public record KayitIstegi(
        [Required, MinLength(3)] string FullName,
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public record AuthYaniti(string Token, DateTime ExpiresAt, KullaniciDto User);

    public record ProjeDto(
        int Id,
        string Ad,
        string? Aciklama,
        DateTime OlusturmaTarihi,
        string OlusturanKullaniciId,
        string? OlusturanKullaniciAdi);

    public record SprintDto(
        int Id,
        int ProjeId,
        string Ad,
        string? Hedef,
        DateTime BaslangicTarihi,
        DateTime BitisTarihi,
        SprintDurumu Durum,
        int ToplamStoryPoint,
        int TamamlananStoryPoint);

    public record GorevDto(
        int Id,
        int ProjeId,
        int? SprintId,
        string Baslik,
        string? Aciklama,
        GorevDurumu Durum,
        GorevOnceligi Oncelik,
        int? StoryPoint,
        string? AtananKullaniciId,
        string? AtananKullaniciAdi,
        DateTime OlusturmaTarihi);

    public record YorumDto(
        int Id,
        int GorevId,
        string YazanKullaniciId,
        string YazanKullaniciAdi,
        string Metin,
        DateTime OlusturmaTarihi);

    // Görev yazma uçlarının istek gövdeleri. Kurallar GorevServisi'ndekiyle aynıdır;
    // servis yine de son sözü söyler. Proje URL'den gelir, yeni görev hep backlog'da başlar.
    public record GorevOlusturIstegi(
        [Required(ErrorMessage = "Başlık zorunludur."), MaxLength(300, ErrorMessage = "Başlık en fazla 300 karakter olabilir.")] string Baslik,
        string? Aciklama,
        GorevOnceligi Oncelik,
        [Range(0, 100, ErrorMessage = "Story point 0 ile 100 arasında olmalıdır.")] int? StoryPoint,
        string? AtananKullaniciId);

    public record GorevGuncelleIstegi(
        [Required(ErrorMessage = "Başlık zorunludur."), MaxLength(300, ErrorMessage = "Başlık en fazla 300 karakter olabilir.")] string Baslik,
        string? Aciklama,
        GorevOnceligi Oncelik,
        GorevDurumu Durum,
        [Range(0, 100, ErrorMessage = "Story point 0 ile 100 arasında olmalıdır.")] int? StoryPoint,
        string? AtananKullaniciId);

    // SprintId null ise görev backlog'a alınır.
    public record GorevSprintIstegi(int? SprintId);
}
