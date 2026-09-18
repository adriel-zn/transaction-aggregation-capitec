using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionAggregation.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202609180001_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "transactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                AccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ExternalTransactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                TransactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_transactions", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_transactions_CustomerId_Category",
            table: "transactions",
            columns: new[] { "CustomerId", "Category" });

        migrationBuilder.CreateIndex(
            name: "IX_transactions_CustomerId_TransactionDate",
            table: "transactions",
            columns: new[] { "CustomerId", "TransactionDate" });

        migrationBuilder.CreateIndex(
            name: "IX_transactions_Source_ExternalTransactionId",
            table: "transactions",
            columns: new[] { "Source", "ExternalTransactionId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "transactions");
}
