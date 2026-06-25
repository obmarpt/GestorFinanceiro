using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorFinanceiro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMetaMovimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetaMovimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaMovimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaMovimentos_Metas_MetaId",
                        column: x => x.MetaId,
                        principalTable: "Metas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaMovimentos_MetaId",
                table: "MetaMovimentos",
                column: "MetaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaMovimentos");
        }
    }
}
