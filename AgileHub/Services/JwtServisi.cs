using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgileHub.Models;
using Microsoft.IdentityModel.Tokens;

namespace AgileHub.Services
{
    public class JwtServisi
    {
        private readonly IConfiguration _config;

        public JwtServisi(IConfiguration config)
        {
            _config = config;
        }

        public AuthYaniti YanitOlustur(ApplicationUser kullanici)
        {
            var bitis = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:SureDakika"));

            var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:
                [
                    new Claim(JwtRegisteredClaimNames.Sub, kullanici.Id),
                    new Claim(JwtRegisteredClaimNames.Email, kullanici.Email!)
                ],
                expires: bitis,
                signingCredentials: new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256));

            return new AuthYaniti(
                new JwtSecurityTokenHandler().WriteToken(token),
                bitis,
                new KullaniciDto(kullanici.Id, kullanici.Email!, kullanici.Ad, kullanici.Soyad));
        }
    }
}
