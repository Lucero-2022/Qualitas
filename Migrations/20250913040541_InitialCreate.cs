using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qualitas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cobranza",
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
                    PrimaNeta = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<int>(type: "int", nullable: false),
                    Oficina = table.Column<int>(type: "int", nullable: false),
                    NombreOficina = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cobranza", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDAgente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDAgente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ramo = table.Column<int>(type: "int", nullable: false),
                    Ejercicio = table.Column<int>(type: "int", nullable: false),
                    Oficina = table.Column<int>(type: "int", nullable: false),
                    Cobertura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ajuste = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Poliza = table.Column<int>(type: "int", nullable: false),
                    NombreAsegurado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SumaAsegurada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Agente = table.Column<int>(type: "int", nullable: false),
                    NombreAgente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreOficina = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId",
                table: "Reservas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cobranza");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
