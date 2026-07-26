using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnibusExpress.Domain.Entities;

namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class ViagemConfiguration : IEntityTypeConfiguration<Viagem>
{
    public void Configure(EntityTypeBuilder<Viagem> builder)
    {
        builder.ToTable("viagens");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.DataHoraPartida)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(v => v.PrecoBase)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(v => v.TotalAssentos).IsRequired();

        builder.HasOne(v => v.Rota)
            .WithMany()
            .HasForeignKey(v => v.RotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.RotaId, v.DataHoraPartida });
    }
}
