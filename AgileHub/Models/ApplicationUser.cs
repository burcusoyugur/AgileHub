using Microsoft.AspNetCore.Identity;

namespace AgileHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;

        public ICollection<ProjeUyesi> ProjeUyelikleri { get; set; } = new List<ProjeUyesi>();
        public ICollection<Gorev> AtananGorevler { get; set; } = new List<Gorev>();
        public ICollection<Yorum> Yorumlar { get; set; } = new List<Yorum>();
        public ICollection<Proje> OlusturulanProjeler { get; set; } = new List<Proje>();
    }
}
