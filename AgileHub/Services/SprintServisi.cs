using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Sprint kuralları burada; web SprintController'ı ve ileride API uçları bu servisi kullanır.
    public class SprintServisi
    {
        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public SprintServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Projenin sprintleri, en yeni başlangıç tarihi önce.
        // Görevler story point toplamları için yüklenir (bkz. DtoDonusumleri).
        public async Task<IslemSonucu<List<Sprint>>> ProjeSprintleriniGetirAsync(int projeId, string kullaniciId)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            var sprintler = await _db.Sprintler
                .Where(s => s.ProjeId == projeId)
                .Include(s => s.Gorevler)
                .OrderByDescending(s => s.BaslangicTarihi)
                .AsNoTracking()
                .ToListAsync();

            return IslemSonucu<List<Sprint>>.Basarili(sprintler);
        }

        public async Task<IslemSonucu<Sprint>> GetirAsync(int sprintId, string kullaniciId)
        {
            var sprint = await _db.Sprintler
                .Include(s => s.Proje)
                .Include(s => s.Gorevler).ThenInclude(g => g.AtananKullanici)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            return IslemSonucu<Sprint>.Basarili(sprint);
        }
    }
}
