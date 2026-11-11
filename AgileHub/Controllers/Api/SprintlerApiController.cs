using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers.Api
{
    // Sprint uçları. Bir projenin sprintleri /api/projeler/{projeId}/sprintler altında,
    // tek sprint işlemleri /api/sprintler/{id} altındadır.
    [Route("api")]
    public class SprintlerApiController : ApiTemelController
    {
        private readonly SprintServisi _sprintServisi;

        public SprintlerApiController(SprintServisi sprintServisi)
        {
            _sprintServisi = sprintServisi;
        }

        /// <summary>Projenin sprintlerini listeler (başlangıç tarihi en yeni önce).</summary>
        [HttpGet("projeler/{projeId:int}/sprintler")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<SprintDto>>> ProjeSprintleri(int projeId)
        {
            var sonuc = await _sprintServisi.ProjeSprintleriniGetirAsync(projeId, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!.Select(s => s.ToDto()).ToList();
        }

        /// <summary>Sprinti story point toplamlarıyla birlikte getirir.</summary>
        [HttpGet("sprintler/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SprintDto>> Getir(int id)
        {
            var sonuc = await _sprintServisi.GetirAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc);
            }

            return sonuc.Deger!.ToDto();
        }
    }
}
