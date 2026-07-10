using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgenciaPublicidad.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    cliente_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_empresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contacto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.cliente_id);
                });

            migrationBuilder.CreateTable(
                name: "Disenadores",
                columns: table => new
                {
                    disenador_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    especialidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disenadores", x => x.disenador_id);
                });

            migrationBuilder.CreateTable(
                name: "Campanas",
                columns: table => new
                {
                    campana_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    presupuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campanas", x => x.campana_id);
                    table.ForeignKey(
                        name: "FK_Campanas_Clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entregables",
                columns: table => new
                {
                    entregable_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campana_id = table.Column<int>(type: "int", nullable: false),
                    disenador_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_entrega = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregables", x => x.entregable_id);
                    table.ForeignKey(
                        name: "FK_Entregables_Campanas_campana_id",
                        column: x => x.campana_id,
                        principalTable: "Campanas",
                        principalColumn: "campana_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entregables_Disenadores_disenador_id",
                        column: x => x.disenador_id,
                        principalTable: "Disenadores",
                        principalColumn: "disenador_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campanas_cliente_id",
                table: "Campanas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Entregables_campana_id",
                table: "Entregables",
                column: "campana_id");

            migrationBuilder.CreateIndex(
                name: "IX_Entregables_disenador_id",
                table: "Entregables",
                column: "disenador_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entregables");

            migrationBuilder.DropTable(
                name: "Campanas");

            migrationBuilder.DropTable(
                name: "Disenadores");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
