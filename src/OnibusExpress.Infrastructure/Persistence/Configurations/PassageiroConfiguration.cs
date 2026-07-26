using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Infrastructure.Persistence.Converters;

namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class PassageiroConfiguration : IEntityTypeConfiguration<Passageiro>
{
    public void Configure(EntityTypeBuilder<Passageiro> builder)
    {
        builder.ToTable("passageiros");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);

        builder.Property(p => p.Cpf)
            .HasConversion(new CpfConverter())
            .HasColumnType("varchar(11)")
            .IsRequired();

        builder.Property(p => p.Email).IsRequired().HasMaxLength(200);
        builder.Property(p => p.DataNascimento).HasColumnType("date").IsRequired();

        builder.HasIndex(p => p.Cpf).IsUnique();
    }
}
