using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgileHub.Migrations
{
    /// <inheritdoc />
    public partial class ProjeUyesiBenzersizIndeks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjeUyeleri_ProjeId",
                table: "ProjeUyeleri");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeUyeleri_ProjeId_KullaniciId",
                table: "ProjeUyeleri",
                columns: new[] { "ProjeId", "KullaniciId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjeUyeleri_ProjeId_KullaniciId",
                table: "ProjeUyeleri");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeUyeleri_ProjeId",
                table: "ProjeUyeleri",
                column: "ProjeId");
        }
    }
}
