using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Modele : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nume",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prenume",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ListaTari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaTari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaTari_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ListaHoteluri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Locatie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Facilitati = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaraId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaHoteluri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaHoteluri_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ListaHoteluri_ListaTari_TaraId",
                        column: x => x.TaraId,
                        principalTable: "ListaTari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaCamere",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacitate = table.Column<int>(type: "int", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PretNoapte = table.Column<float>(type: "real", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaCamere", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaCamere_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ListaCamere_ListaHoteluri_HotelId",
                        column: x => x.HotelId,
                        principalTable: "ListaHoteluri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titlu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Continut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    HotelId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ListaReviews_ListaHoteluri_HotelId",
                        column: x => x.HotelId,
                        principalTable: "ListaHoteluri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ListaRezervari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListaClienti = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Suma = table.Column<float>(type: "real", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CameraId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaRezervari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaRezervari_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ListaRezervari_ListaCamere_CameraId",
                        column: x => x.CameraId,
                        principalTable: "ListaCamere",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListaCamere_HotelId",
                table: "ListaCamere",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaCamere_UserId",
                table: "ListaCamere",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaHoteluri_TaraId",
                table: "ListaHoteluri",
                column: "TaraId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaHoteluri_UserId",
                table: "ListaHoteluri",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaReviews_HotelId",
                table: "ListaReviews",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaReviews_UserId",
                table: "ListaReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaRezervari_CameraId",
                table: "ListaRezervari",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaRezervari_UserId",
                table: "ListaRezervari",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaTari_UserId",
                table: "ListaTari",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListaReviews");

            migrationBuilder.DropTable(
                name: "ListaRezervari");

            migrationBuilder.DropTable(
                name: "ListaCamere");

            migrationBuilder.DropTable(
                name: "ListaHoteluri");

            migrationBuilder.DropTable(
                name: "ListaTari");

            migrationBuilder.DropColumn(
                name: "Nume",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Prenume",
                table: "AspNetUsers");
        }
    }
}
