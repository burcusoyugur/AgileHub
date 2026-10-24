using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    public class Etiket
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(7)]
        public string Renk { get; set; } = "#6c757d";

        public ICollection<GorevEtiket> GorevEtiketler { get; set; } = new List<GorevEtiket>();
    }
}
