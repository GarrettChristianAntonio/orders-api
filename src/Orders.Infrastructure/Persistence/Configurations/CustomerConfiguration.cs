using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.OwnsOne(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("shipping_street")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("shipping_city")
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("shipping_state")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("shipping_country")
                .HasMaxLength(100);

            address.Property(a => a.ZipCode)
                .HasColumnName("shipping_zip_code")
                .HasMaxLength(20);
        });

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(c => c.DomainEvents);
    }
}
