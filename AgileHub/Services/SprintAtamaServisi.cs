using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Görevleri sprinte atama / sprintten çıkarma kuralları burada;
    // web SprintPlanlamaController'ı ve ileride API uçları bu servisi kullanır.
    // Bu servis yalnızca Gorev.SprintId'yi değiştirir.
    public class SprintAtamaServisi
    {
        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public SprintAtamaServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Tamamlanmış veya iptal edilmiş sprint artık değişmez.
        private static bool Kapali(Sprint sprint)
        {
            return sprint.Durum is SprintDurumu.Tamamlandı or SprintDurumu.İptalEdildi;
        }

        // Planlama sayfasının verisi: projenin sprintleri, seçili sprintin görevleri ve backlog.
        // sprintId verilmezse aktif sprint, o da yoksa ilk planlanan sprint seçilir.
        public async Task<IslemSonucu<SprintPlanlamaViewModel>> PlanlamaVerisiniGetirAsync(
            int projeId, int? sprintId, string kullaniciId)
        {
            var projeAdi = await _db.Projeler
                .Where(p => p.Id == projeId)
                .Select(p => p.Ad)
                .FirstOrDefaultAsync();

            if (projeAdi is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            var sprintler = await _db.Sprintler
                .Where(s => s.ProjeId == projeId)
                .OrderBy(s => s.Durum == SprintDurumu.Aktif ? 0 : s.Durum == SprintDurumu.Planlanıyor ? 1 : 2)
                .ThenBy(s => s.BaslangicTarihi)
                .AsNoTracking()
                .ToListAsync();

            Sprint? secili;

            if (sprintId is int istenen)
            {
                secili = sprintler.FirstOrDefault(s => s.Id == istenen);

                // Olmayan ya da başka projeye ait sprint.
                if (secili is null)
                {
                    return IslemSonucu.Bulunamadi();
                }
            }
            else
            {
                secili = sprintler.FirstOrDefault(s => !Kapali(s));
            }

            var backlog = await _db.Gorevler
                .Where(g => g.ProjeId == projeId && g.SprintId == null)
                .Include(g => g.AtananKullanici)
                .OrderByDescending(g => g.Oncelik)
                .ThenBy(g => g.OlusturmaTarihi)
                .AsNoTracking()
                .ToListAsync();

            var sprintGorevleri = secili is null
                ? new List<Gorev>()
                : await _db.Gorevler
                    .Where(g => g.SprintId == secili.Id)
                    .Include(g => g.AtananKullanici)
                    .OrderByDescending(g => g.Oncelik)
                    .ThenBy(g => g.OlusturmaTarihi)
                    .AsNoTracking()
                    .ToListAsync();

            return IslemSonucu<SprintPlanlamaViewModel>.Basarili(new SprintPlanlamaViewModel
            {
                ProjeId = projeId,
                ProjeAdi = projeAdi,
                Sprintler = sprintler,
                SeciliSprint = secili,
                Backlog = backlog,
                SprintGorevleri = sprintGorevleri
            });
        }

        public async Task<IslemSonucu> SpriteAtaAsync(int gorevId, int sprintId, string kullaniciId)
        {
            var gorev = await _db.Gorevler.FirstOrDefaultAsync(g => g.Id == gorevId);

            if (gorev is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            var sprint = await _db.Sprintler.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(gorev.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (gorev.ProjeId != sprint.ProjeId)
            {
                return IslemSonucu.Gecersiz("Görev yalnızca kendi projesindeki bir sprinte atanabilir.");
            }

            // Zaten o sprintte: sessizce başarılı.
            if (gorev.SprintId == sprintId)
            {
                return IslemSonucu.Basarili();
            }

            if (Kapali(sprint))
            {
                return IslemSonucu.Gecersiz("Tamamlanmış veya iptal edilmiş sprinte görev atanamaz.");
            }

            gorev.SprintId = sprintId;
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }

        // Görev backlog'a döner (SprintId = null). Zaten backlog'daysa sessizce başarılı.
        public async Task<IslemSonucu> SprinttenCikarAsync(int gorevId, string kullaniciId)
        {
            var gorev = await _db.Gorevler
                .Include(g => g.Sprint)
                .FirstOrDefaultAsync(g => g.Id == gorevId);

            if (gorev is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(gorev.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (gorev.Sprint is null)
            {
                return IslemSonucu.Basarili();
            }

            if (Kapali(gorev.Sprint))
            {
                return IslemSonucu.Gecersiz("Tamamlanmış veya iptal edilmiş sprintten görev çıkarılamaz.");
            }

            gorev.SprintId = null;
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }

        // Hepsi ya da hiçbiri: bir görev bile kurala uymazsa hiçbiri taşınmaz.
        // Değer olarak sprinte gerçekten taşınan (zaten orada olmayan) görev sayısı döner.
        public async Task<IslemSonucu<int>> TopluAtaAsync(int sprintId, IEnumerable<int> gorevIdleri, string kullaniciId)
        {
            var idler = gorevIdleri.Distinct().ToList();

            var sprint = await _db.Sprintler.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (idler.Count == 0)
            {
                return IslemSonucu.Gecersiz("Sprinte eklemek için en az bir görev seçin.");
            }

            var gorevler = await _db.Gorevler.Where(g => idler.Contains(g.Id)).ToListAsync();

            if (gorevler.Count != idler.Count)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (gorevler.Any(g => g.ProjeId != sprint.ProjeId))
            {
                return IslemSonucu.Gecersiz("Görevler yalnızca kendi projelerindeki bir sprinte atanabilir.");
            }

            var tasinacaklar = gorevler.Where(g => g.SprintId != sprintId).ToList();

            if (tasinacaklar.Count == 0)
            {
                return IslemSonucu<int>.Basarili(0);
            }

            if (Kapali(sprint))
            {
                return IslemSonucu.Gecersiz("Tamamlanmış veya iptal edilmiş sprinte görev atanamaz.");
            }

            foreach (var gorev in tasinacaklar)
            {
                gorev.SprintId = sprintId;
            }

            await _db.SaveChangesAsync();

            return IslemSonucu<int>.Basarili(tasinacaklar.Count);
        }
    }
}
