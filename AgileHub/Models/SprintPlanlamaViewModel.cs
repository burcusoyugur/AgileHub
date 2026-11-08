namespace AgileHub.Models
{
    // Sprint planlama sayfasının verisi: solda backlog, sağda seçili sprintin görevleri.
    public class SprintPlanlamaViewModel
    {
        public int ProjeId { get; set; }

        public string ProjeAdi { get; set; } = string.Empty;

        public List<Sprint> Sprintler { get; set; } = new();

        public Sprint? SeciliSprint { get; set; }

        public List<Gorev> Backlog { get; set; } = new();

        public List<Gorev> SprintGorevleri { get; set; } = new();

        // Seçili sprintin story point toplamı (kapasite fikri için). Boş story point 0 sayılır.
        public int SprintStoryPoint => SprintGorevleri.Sum(g => g.StoryPoint ?? 0);

        // Planlanıyor veya Aktif sprinte görev eklenip çıkarılabilir.
        public bool SprintDuzenlenebilir =>
            SeciliSprint is not null
            && SeciliSprint.Durum is SprintDurumu.Planlanıyor or SprintDurumu.Aktif;
    }
}
