using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    // API'nin döndürdüğü veriler. Entity'ler doğrudan döndürülmez: ApplicationUser
    // şifre özeti gibi alanlar taşır, gezinme özellikleri de JSON'da döngü oluşturur.
    // Alan adları modellerle aynıdır; hesaplanan alanlar ek olarak yer alır.

    public record KullaniciDto(string Id, string Email, string Ad, string Soyad);

    public record GirisIstegi(
        [Required(ErrorMessage = "E-posta zorunludur."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi gir.")] string Email,
        [Required(ErrorMessage = "Şifre zorunludur.")] string Password);

    public record KayitIstegi(
        [Required(ErrorMessage = "Ad soyad zorunludur."), MinLength(3, ErrorMessage = "Ad soyad en az 3 karakter olmalıdır.")] string FullName,
        [Required(ErrorMessage = "E-posta zorunludur."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi gir.")] string Email,
        [Required(ErrorMessage = "Şifre zorunludur.")] string Password);

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
}
