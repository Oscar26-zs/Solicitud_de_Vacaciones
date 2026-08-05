using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vacations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFKIntegridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_HistorialSolicitudes_SolicitudesVacaciones_SolicitudId",
                table: "HistorialSolicitudes",
                column: "SolicitudId",
                principalTable: "SolicitudesVacaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaldosEmpleados_Empleados_EmpleadoId",
                table: "SaldosEmpleados",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesVacaciones_Empleados_EmpleadoId",
                table: "SolicitudesVacaciones",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistorialSolicitudes_SolicitudesVacaciones_SolicitudId",
                table: "HistorialSolicitudes");

            migrationBuilder.DropForeignKey(
                name: "FK_SaldosEmpleados_Empleados_EmpleadoId",
                table: "SaldosEmpleados");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesVacaciones_Empleados_EmpleadoId",
                table: "SolicitudesVacaciones");
        }
    }
}
