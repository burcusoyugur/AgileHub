using AgileHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Proje> Projeler { get; set; }
        public DbSet<ProjeUyesi> ProjeUyeleri { get; set; }
        public DbSet<Sprint> Sprintler { get; set; }
        public DbSet<Gorev> Gorevler { get; set; }
        public DbSet<Yorum> Yorumlar { get; set; }
        public DbSet<Etiket> Etiketler { get; set; }
        public DbSet<GorevEtiket> GorevEtiketler { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GorevEtiket>()
                .HasKey(ge => new { ge.GorevId, ge.EtiketId });

            builder.Entity<GorevEtiket>()
                .HasOne(ge => ge.Gorev)
                .WithMany(g => g.GorevEtiketler)
                .HasForeignKey(ge => ge.GorevId);

            builder.Entity<GorevEtiket>()
                .HasOne(ge => ge.Etiket)
                .WithMany(e => e.GorevEtiketler)
                .HasForeignKey(ge => ge.EtiketId);

            builder.Entity<Proje>()
                .HasOne(p => p.OlusturanKullanici)
                .WithMany(u => u.OlusturulanProjeler)
                .HasForeignKey(p => p.OlusturanKullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProjeUyesi>()
                .HasOne(pu => pu.Kullanici)
                .WithMany(u => u.ProjeUyelikleri)
                .HasForeignKey(pu => pu.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Gorev>()
                .HasOne(g => g.AtananKullanici)
                .WithMany(u => u.AtananGorevler)
                .HasForeignKey(g => g.AtananKullaniciId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Yorum>()
                .HasOne(y => y.YazanKullanici)
                .WithMany(u => u.Yorumlar)
                .HasForeignKey(y => y.YazanKullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Gorev>()
                .HasOne(g => g.Sprint)
                .WithMany(s => s.Gorevler)
                .HasForeignKey(g => g.SprintId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Gorev>()
                .HasOne(g => g.Proje)
                .WithMany(p => p.Gorevler)
                .HasForeignKey(g => g.ProjeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
