using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Infrastructure.Persistence.Converters;

namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("reservas");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.NumeroAssento).IsRequired();

        // Enum como texto: torna o filtro do índice parcial legível ('Confirmada').
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Codigo)
            .HasConversion(new CodigoReservaConverter())
            .HasColumnType("varchar(9)")
            .IsRequired();

        builder.HasIndex(r => r.Codigo).IsUnique();

        builder.HasOne(r => r.Viagem)
            .WithMany()
            .HasForeignKey(r => r.ViagemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Passageiro)
            .WithMany()
            .HasForeignKey(r => r.PassageiroId)
            .OnDelete(DeleteBehavior.Restrict);

        // GARANTIA REAL contra corrida de assento: no nível do banco, um assento
        // só pode ter UMA reserva Confirmada por viagem. Reservas Canceladas não
        // entram no índice (filtro parcial), então o assento volta a ficar livre.
        builder.HasIndex(r => new { r.ViagemId, r.NumeroAssento })
            .IsUnique()
            .HasFilter($"\"Status\" = '{nameof(StatusReserva.Confirmada)}'");
    }
}
