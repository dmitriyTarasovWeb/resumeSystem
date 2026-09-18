using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Domain;

namespace resumeSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RequiredUserAttributes> RequiredUserAttributes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RequiredUserAttributes>(entity =>
        {
            entity.ToTable("required_user_attributes");

            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId)
                .HasColumnName("user_id");

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();

            entity.Property(x => x.SecondName)
                .HasColumnName("second_name")
                .IsRequired();

            entity.Property(x => x.PhotoUrl)
                .HasColumnName("photo_url");

            entity.Property(x => x.Description)
                .HasColumnName("description");

            entity.Property(x => x.Location)
                .HasColumnName("location");

            entity.Property(x => x.Age)
                .HasColumnName("age");

            entity.HasOne(x => x.User)
                .WithOne(x => x.RequiredUserAttributes)
                .HasForeignKey<RequiredUserAttributes>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
