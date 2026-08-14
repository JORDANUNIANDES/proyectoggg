using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuscleHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddActivoToPlanAndEjercicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Planes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Ejercicios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Planes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Ejercicios");
        }
    }
}
