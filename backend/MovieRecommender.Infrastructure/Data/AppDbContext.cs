using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using MovieRecommender.Domain.Entities;

namespace MovieRecommender.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.ToTable("Movies");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Title).IsRequired();
                var converter = new ValueConverter<List<float>, string>(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<List<float>>(v, new JsonSerializerOptions()) ?? new List<float>());

                entity.Property(m => m.Embedding)
                      .HasConversion(converter)
                      .HasColumnType("TEXT");
            });
        }
    }
}
