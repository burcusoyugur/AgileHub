namespace AgileHub.Models
{
    // Görev enum'larının ekranda gösterilen Türkçe karşılıkları.
    // Enum adları API ve mobil tarafından string olarak eşlendiği için değiştirilmez;
    // yalnızca görünen metin buradan gelir.
    public static class GorevMetinleri
    {
        public static string Metin(this GorevDurumu durum)
        {
            return durum switch
            {
                GorevDurumu.Yapilacak => "Yapılacak",
                GorevDurumu.Yapiliyor => "Yapılıyor",
                GorevDurumu.Test => "Test",
                GorevDurumu.Bitti => "Bitti",
                _ => durum.ToString()
            };
        }

        public static string Metin(this GorevOnceligi oncelik)
        {
            return oncelik switch
            {
                GorevOnceligi.Dusuk => "Düşük",
                GorevOnceligi.Orta => "Orta",
                GorevOnceligi.Yuksek => "Yüksek",
                GorevOnceligi.Kritik => "Kritik",
                _ => oncelik.ToString()
            };
        }
    }
}
