using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("ServiceOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ProblemDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.TechnicianId);

        // Customer value object stored inline as owned columns.
        builder.OwnsOne(o => o.Customer, customer =>
        {
            customer.Property(c => c.Name)
                .HasColumnName("CustomerName")
                .IsRequired()
                .HasMaxLength(200);

            customer.Property(c => c.Phone)
                .HasColumnName("CustomerPhone")
                .HasMaxLength(50);

            customer.Property(c => c.Email)
                .HasColumnName("CustomerEmail")
                .HasMaxLength(320);
        });
        builder.Navigation(o => o.Customer).IsRequired();

        // Equipment value object stored inline as owned columns.
        builder.OwnsOne(o => o.Equipment, equipment =>
        {
            equipment.Property(e => e.Type)
                .HasColumnName("EquipmentType")
                .IsRequired()
                .HasMaxLength(200);

            equipment.Property(e => e.Brand)
                .HasColumnName("EquipmentBrand")
                .HasMaxLength(200);

            equipment.Property(e => e.Model)
                .HasColumnName("EquipmentModel")
                .HasMaxLength(200);

            equipment.Property(e => e.SerialNumber)
                .HasColumnName("EquipmentSerialNumber")
                .HasMaxLength(200);
        });
        builder.Navigation(o => o.Equipment).IsRequired();
    }
}
