using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qualitas.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Cobranza",
                table: "Cobranza");

            migrationBuilder.RenameTable(
                name: "Cobranza",
                newName: "Cobranzas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cobranzas",
                table: "Cobranzas",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Producciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAgente = table.Column<int>(type: "int", nullable: false),
                    Promotor = table.Column<int>(type: "int", nullable: false),
                    NombreAgente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoPoliza = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Poliza = table.Column<int>(type: "int", nullable: false),
                    NombreAsegurado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Primaneta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "date", nullable: false),
                    VigenciaDesde = table.Column<DateTime>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateTime>(type: "date", nullable: false),
                    Oficina = table.Column<int>(type: "int", nullable: false),
                    NombreOficina = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producciones", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Producciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cobranzas",
                table: "Cobranzas");

            migrationBuilder.RenameTable(
                name: "Cobranzas",
                newName: "Cobranza");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cobranza",
                table: "Cobranza",
                column: "Id");
        }
    }
}
