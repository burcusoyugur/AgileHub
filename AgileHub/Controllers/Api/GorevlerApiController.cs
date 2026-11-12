using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers.Api
{
    // Görev uçları. Bir projenin görevleri /api/projeler/{projeId}/gorevler altında,
    // tek görev işlemleri /api/gorevler/{id} altındadır.
    [Route("api")]
    public class GorevlerApiController : ApiTemelController
    {
        private readonly GorevServisi _gorevServisi;

        public GorevlerApiController(GorevServisi gorevServisi)
        {
            _gorevServisi = gorevServisi;
        }

        /// <summary>Projenin görevlerini listeler (önce en yüksek öncelik, aynı öncelikte en yeni).</summary>
        /// <param name="projeId">Proje kimliği.</param>
        /// <param name="sadeceBacklog">true ise yalnızca sprinte atanmamış görevler gelir.</param>
        /// <param name="durum">İsteğe bağlı durum filtresi (Yapilacak, Yapiliyor, Test, Bitti).</param>
        /// <param name="atananKullaniciId">İsteğe bağlı atanan kişi filtresi.</param>
        [HttpGet("projeler/{projeId:int}/gorevler")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<GorevDto>>> ProjeGorevleri(
            int projeId,
            [FromQuery] bool sadeceBacklog = false,
            [FromQuery] GorevDurumu? durum = null,
            [FromQuery] string? atananKullaniciId = null)
        {
            var sonuc = await _gorevServisi.ProjeGorevleriniGetirAsync(
                projeId, KullaniciId, sadeceBacklog, durum, atananKullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!.Select(g => g.ToDto()).ToList();
        }

        /// <summary>Görevi getirir. Yalnızca projenin üyeleri görebilir.</summary>
        // Route adı, yeni görev oluşturulunca Location başlığı için kullanılır.
        [HttpGet("gorevler/{id:int}", Name = "GorevGetir")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GorevDto>> Getir(int id)
        {
            var sonuc = await _gorevServisi.GetirAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!.ToDto();
        }
    

        /// <summary>Projeye yeni görev ekler; görev her zaman backlog'da başlar.</summary>
        /// <param name="projeId">Görevin ekleneceği proje kimliği.</param>
        /// <param name="istek">Yeni görevin bilgileri.</param>
        [HttpPost("projeler/{projeId:int}/gorevler")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GorevDto>> Olustur(int projeId, [FromBody] GorevOlusturIstegi istek)
        {
            var sonuc = await _gorevServisi.OlusturAsync(
                projeId, istek.Baslik, istek.Aciklama, istek.Oncelik,
                istek.StoryPoint, istek.AtananKullaniciId, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            // Oluşan entity'de atanan kişi yüklü değil; DTO'da adı dolu olsun diye yeniden okunur.
            var olusan = await _gorevServisi.GetirAsync(sonuc.Deger!.Id, KullaniciId);

            if (!olusan.BasariliMi)
            {
                return HataSonucu(olusan);
            }

            var dto = olusan.Deger!.ToDto();
            return CreatedAtRoute("GorevGetir", new { id = dto.Id }, dto);
        }
    

        /// <summary>Görevin bilgilerini günceller; sprinti değiştirmez.</summary>
        /// <param name="id">Görev kimliği.</param>
        /// <param name="istek">Görevin yeni bilgileri.</param>
        [HttpPut("gorevler/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GorevDto>> Guncelle(int id, [FromBody] GorevGuncelleIstegi istek)
        {
            var sonuc = await _gorevServisi.GuncelleAsync(
                id, istek.Baslik, istek.Aciklama, istek.Oncelik, istek.Durum,
                istek.StoryPoint, istek.AtananKullaniciId, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return await GuncelGorevAsync(id);
        }
    

        /// <summary>Görevi siler.</summary>
        /// <param name="id">Görev kimliği.</param>
        [HttpDelete("gorevler/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _gorevServisi.SilAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return NoContent();
        }
    

        // Yazma işleminden sonra güncel görevi (atanan kişi dahil) okuyup DTO olarak döndürür.
        private async Task<ActionResult<GorevDto>> GuncelGorevAsync(int id)
        {
            var sonuc = await _gorevServisi.GetirAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!.ToDto();
        }
    }
}
