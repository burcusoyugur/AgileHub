namespace AgileHub.Models
{
    public class ProjeUyesi
    {
        public int Id { get; set; }

        public int ProjeId { get; set; }
        public Proje? Proje { get; set; }

        public string KullaniciId { get; set; } = string.Empty;
        public ApplicationUser? Kullanici { get; set; }

        public DateTime KatilmaTarihi { get; set; } = DateTime.Now;
    }
}
