namespace AgileHub.Services
{
    public enum IslemDurumu
    {
        Basarili,
        Bulunamadi,
        Yetkisiz,
        Gecersiz
    }

    // Servisler HTTP'ye bağlı değildir; sonucu bu tiple döndürür.
    // MVC controller'ı NotFound/Forbid/TempData'ya, API controller'ı 404/403/400'e çevirir.
    public record IslemSonucu(IslemDurumu Durum, string? Hata = null)
    {
        public bool BasariliMi => Durum == IslemDurumu.Basarili;

        public static IslemSonucu Basarili() => new(IslemDurumu.Basarili);
        public static IslemSonucu Bulunamadi() => new(IslemDurumu.Bulunamadi);
        public static IslemSonucu Yetkisiz() => new(IslemDurumu.Yetkisiz);
        public static IslemSonucu Gecersiz(string hata) => new(IslemDurumu.Gecersiz, hata);
    }

    // Başarılı işlem bir değer de döndürecekse (ör. yeni oluşturulan kayıt) kullanılır.
    public record IslemSonucu<T>(IslemDurumu Durum, T? Deger = default, string? Hata = null)
    {
        public bool BasariliMi => Durum == IslemDurumu.Basarili;

        public static IslemSonucu<T> Basarili(T deger) => new(IslemDurumu.Basarili, deger);

        // Hata sonuçları her iki tipte de aynı yazılsın diye:
        // IslemSonucu<Sprint> dönen metotta "return IslemSonucu.Bulunamadi();" yazılabilir.
        public static implicit operator IslemSonucu<T>(IslemSonucu sonuc) => new(sonuc.Durum, default, sonuc.Hata);
    }
}
