using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgileHub.Models
{
    // Görev oluşturma ve düzenleme formu. ProjeId ve SprintId formdan değiştirilemez;
    // yeni görev her zaman Yapılacak durumunda ve backlog'da başlar (bkz. GorevServisi).
    public class GorevFormViewModel
    {
        public int Id { get; set; }

        public int ProjeId { get; set; }

        [Required(ErrorMessage = "Görev başlığı zorunludur.")]
        [MaxLength(300, ErrorMessage = "Görev başlığı en fazla 300 karakter olabilir.")]
        [Display(Name = "Başlık")]
        public string Baslik { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Display(Name = "Öncelik")]
        public GorevOnceligi Oncelik { get; set; } = GorevOnceligi.Orta;

        // Yalnızca düzenleme formunda gösterilir.
        [Display(Name = "Durum")]
        public GorevDurumu Durum { get; set; } = GorevDurumu.Yapilacak;

        [Range(0, 100, ErrorMessage = "Story point 0 ile 100 arasında olmalıdır.")]
        [Display(Name = "Story point")]
        public int? StoryPoint { get; set; }

        [Display(Name = "Atanan kişi")]
        public string? AtananKullaniciId { get; set; }

        // "Atanan kişi" açılır listesi için projenin üyeleri; formdan geri gelmez.
        [ValidateNever]
        public IEnumerable<SelectListItem> AtanabilecekKisiler { get; set; } = [];

        // Görev, projeden çıkarılmış birine atanmışsa düzenleme formunda uyarı için adı; formdan geri gelmez.
        [ValidateNever]
        public string? UyeOlmayanAtananAdi { get; set; }
    }
}
