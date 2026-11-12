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
    }
}
