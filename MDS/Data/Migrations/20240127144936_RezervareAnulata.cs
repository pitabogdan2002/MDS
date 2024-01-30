using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RezervareAnulata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Anulata",
                table: "ListaRezervari",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anulata",
                table: "ListaRezervari");
        }
    }
}
