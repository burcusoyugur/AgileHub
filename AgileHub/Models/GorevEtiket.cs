namespace AgileHub.Models
{
    public class GorevEtiket
    {
        public int GorevId { get; set; }
        public Gorev? Gorev { get; set; }

        public int EtiketId { get; set; }
        public Etiket? Etiket { get; set; }
    }
}
