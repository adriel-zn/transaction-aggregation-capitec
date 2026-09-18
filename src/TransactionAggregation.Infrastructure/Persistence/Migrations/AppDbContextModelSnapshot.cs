using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionAggregation.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.8");
        modelBuilder.Entity("TransactionAggregation.Domain.Transactions.Transaction", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid");
            b.Property<string>("AccountId").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<decimal>("Amount").HasPrecision(19,4).HasColumnType("numeric(19,4)");
            b.Property<string>("Category").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<string>("Currency").IsRequired().HasMaxLength(3).HasColumnType("character varying(3)");
            b.Property<string>("CustomerId").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("character varying(500)");
            b.Property<string>("ExternalTransactionId").IsRequired().HasMaxLength(255).HasColumnType("character varying(255)");
            b.Property<string>("Source").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<DateTimeOffset>("TransactionDate").HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.HasIndex("CustomerId", "Category");
            b.HasIndex("CustomerId", "TransactionDate");
            b.HasIndex("Source", "ExternalTransactionId").IsUnique();
            b.ToTable("transactions");
        });
    }
}
