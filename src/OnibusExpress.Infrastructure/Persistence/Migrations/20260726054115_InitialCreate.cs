using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnibusExpress.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "passageiros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cpf = table.Column<string>(type: "varchar(11)", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passageiros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Origem = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Destino = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DuracaoEstimada = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rotas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "viagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RotaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHoraPartida = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    PrecoBase = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalAssentos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_viagens_rotas_RotaId",
                        column: x => x.RotaId,
                        principalTable: "rotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViagemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassageiroId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroAssento = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Codigo = table.Column<string>(type: "varchar(9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservas_passageiros_PassageiroId",
                        column: x => x.PassageiroId,
                        principalTable: "passageiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_viagens_ViagemId",
                        column: x => x.ViagemId,
                        principalTable: "viagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_passageiros_Cpf",
                table: "passageiros",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_Codigo",
                table: "reservas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_PassageiroId",
                table: "reservas",
                column: "PassageiroId");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_ViagemId_NumeroAssento",
                table: "reservas",
                columns: new[] { "ViagemId", "NumeroAssento" },
                unique: true,
                filter: "\"Status\" = 'Confirmada'");

            migrationBuilder.CreateIndex(
                name: "IX_rotas_Origem_Destino",
                table: "rotas",
                columns: new[] { "Origem", "Destino" });

            migrationBuilder.CreateIndex(
                name: "IX_viagens_RotaId_DataHoraPartida",
                table: "viagens",
                columns: new[] { "RotaId", "DataHoraPartida" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "passageiros");

            migrationBuilder.DropTable(
                name: "viagens");

            migrationBuilder.DropTable(
                name: "rotas");
        }
    }
}
