using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Data;

public class UrlConfiguation: IEntityTypeConfiguration<Url>
{
    public void Configure(EntityTypeBuilder<Url> builder)
    {
        builder.ToTable("Urls");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.shortCode)
            .IsRequired()
            .HasMaxLength(UrlShortener.Core.ShortCodeGenerator.ShortCodeLength);

        builder.Property(u => u.longUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.HasIndex(u => u.shortCode) // To check the existence of short URL
            .IsUnique(); 

    }
}
