using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers.Api
{
    // Proje uçları. Projeyi yalnızca üyeleri görebilir.
    [Route("api/projeler")]
    public class ProjelerApiController : ApiTemelController
    {
        private readonly ProjeServisi _projeServisi;
        private readonly GorevServisi _gorevServisi;

        public ProjelerApiController(ProjeServisi projeServisi, GorevServisi gorevServisi)
        {
            _projeServisi = projeServisi;
            _gorevServisi = gorevServisi;
        }

        /// <summary>Giriş yapan kullanıcının üyesi olduğu projeleri listeler (en yeni önce).</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ProjeDto>>> Listele()
        {
            var projeler = await _projeServisi.UyesiOldugumProjeleriGetirAsync(KullaniciId);
            return projeler.Select(p => p.ToDto()).ToList();
        }

        /// <summary>Projeyi getirir.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjeDto>> Getir(int id)
        {
            var sonuc = await _projeServisi.GetirAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!.ToDto();
        }

        /// <summary>Projenin üyelerini listeler (ada göre). Görev formundaki "atanan kişi" seçimi için.</summary>
        [HttpGet("{projeId:int}/uyeler")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<KullaniciDto>>> Uyeler(int projeId)
        {
            // Görev yalnızca proje üyelerine atanabildiği için atanabilecek kişiler = üyeler.
            var sonuc = await _gorevServisi.AtanabilecekKisileriGetirAsync(projeId, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!
                .Select(k => new KullaniciDto(k.Id, k.Email ?? string.Empty, k.Ad, k.Soyad))
                .ToList();
        }
    }
}
