using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductBatchConfig : IEntityTypeConfiguration<ProductBatch>
{
    public void Configure(EntityTypeBuilder<ProductBatch> builder)
    {
        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.Stock)
            .IsRequired();

        builder.Property(pb => pb.ManufacturingDate)
            .HasColumnType("datetime");

        builder.Property(pb => pb.ExpirationDate)
            .HasColumnType("datetime");

        builder.HasOne(pb => pb.Product)
            .WithMany(p => p.ProductBatches)
            .HasForeignKey(pb => pb.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
