using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Firstname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Lastname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Gender)
            .HasMaxLength(50)
            .IsRequired();

        // Owned Entity: Address
        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(p => p.Street)
                .HasMaxLength(100)
                .IsRequired();

            a.Property(p => p.City)
                .HasMaxLength(50)
                .IsRequired();

            a.Property(p => p.State)
                .HasMaxLength(50);

            a.Property(p => p.PostalCode)
                .HasMaxLength(20)
                .IsRequired();

            a.Property(p => p.Country)
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(50)
                .IsRequired();
        });

        // Relationships
        builder.HasMany(u => u.BasketItems)
            .WithOne(bi => bi.User)
            .HasForeignKey(bi => bi.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}