using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "normalized_description",
                table: "transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                computedColumnSql: "UPPER(TRIM(description))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_normalized_description",
                table: "transactions",
                column: "normalized_description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_normalized_description",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "normalized_description",
                table: "transactions");
        }
    }
}
