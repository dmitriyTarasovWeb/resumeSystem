using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Domain;

namespace resumeSystem.Data;

using DomainAttribute = resumeSystem.Domain.Attribute;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RequiredUserAttributes> RequiredUserAttributes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DataType> DataTypes { get; set; }
    public DbSet<DomainAttribute> Attributes { get; set; }
    public DbSet<AttributeOption> AttributeOptions { get; set; }
    public DbSet<UserAttribute> UserAttributes { get; set; }
    public DbSet<CompareType> CompareTypes { get; set; }
    public DbSet<DataTypeCompareType> DataTypeCompareTypes { get; set; }


    public DbSet<Position> Positions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Experience> Experiences { get; set; }
    public DbSet<ExperienceTag> ExperienceTags { get; set; }

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

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.Title)
                .HasColumnName("title")
                .IsRequired();

            entity.Property(x => x.IsDisplay)
                .HasColumnName("is_display")
                .IsRequired();

            entity.HasData(
                new Category
                {
                    Id = 1,
                    Title = "Общие",
                    IsDisplay = true
                }
            );
        });

        builder.Entity<DataType>(entity =>
        {
            entity.ToTable("data_types");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.DataTypeName)
                .HasColumnName("data_type")
                .IsRequired();

            entity.HasData(
                new DataType
                {
                    Id = 1,
                    DataTypeName = "String"
                },
                new DataType
                {
                    Id = 2,
                    DataTypeName = "Text"
                },
                new DataType
                {
                    Id = 3,
                    DataTypeName = "Number"
                },
                new DataType
                {
                    Id = 4,
                    DataTypeName = "Date"
                },
                new DataType
                {
                    Id = 5,
                    DataTypeName = "Period"
                },
                new DataType
                {
                    Id = 6,
                    DataTypeName = "Boolean"
                },
                new DataType
                {
                    Id = 7,
                    DataTypeName = "One of many"
                }
            );
        });

        builder.Entity<DomainAttribute>(entity =>
        {
            entity.ToTable("attributes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.CategoryId)
                .HasColumnName("category")
                .IsRequired();

            entity.Property(x => x.Title)
                .HasColumnName("title")
                .IsRequired();

            entity.Property(x => x.DataTypeId)
                .HasColumnName("data_type")
                .IsRequired();

            entity.Property(x => x.IsDisplay)
                .HasColumnName("is_display")
                .IsRequired();

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DataType)
                .WithMany()
                .HasForeignKey(x => x.DataTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new DomainAttribute
                {
                    Id = 1,
                    CategoryId = 1,
                    Title = "Любимый язык программирования",
                    DataTypeId = 1,
                    IsDisplay = true
                },

                new DomainAttribute
                {
                    Id = 2,
                    CategoryId = 1,
                    Title = "О себе",
                    DataTypeId = 2,
                    IsDisplay = true
                },

                new DomainAttribute
                {
                    Id = 3,
                    CategoryId = 1,
                    Title = "Количество лет опыта",
                    DataTypeId = 3,
                    IsDisplay = true
                },

                new DomainAttribute
                {
                    Id = 4,
                    CategoryId = 1,
                    Title = "Дата начала карьеры",
                    DataTypeId = 4,
                    IsDisplay = true
                },

                new DomainAttribute
                {
                    Id = 5,
                    CategoryId = 1,
                    Title = "Период работы",
                    DataTypeId = 5,
                    IsDisplay = true
                },

                new DomainAttribute
                {
                    Id = 6,
                    CategoryId = 1,
                    Title = "Готов к удалённой работе",
                    DataTypeId = 6,
                    IsDisplay = true
                },

                new DomainAttribute
                {
                    Id = 7,
                    CategoryId = 1,
                    Title = "Уровень английского",
                    DataTypeId = 7,
                    IsDisplay = true
                }
            );
        });

        builder.Entity<AttributeOption>(entity =>
        {
            entity.ToTable("attribute_options");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.AttributeId)
                .HasColumnName("attribute")
                .IsRequired();

            entity.Property(x => x.Options)
                .HasColumnName("options")
                .IsRequired();

            entity.HasOne(x => x.Attribute)
                .WithMany()
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(
                new AttributeOption
                {
                    Id = 1,
                    AttributeId = 7,
                    Options = "A1"
                },
                new AttributeOption
                {
                    Id = 2,
                    AttributeId = 7,
                    Options = "A2"
                },
                new AttributeOption
                {
                    Id = 3,
                    AttributeId = 7,
                    Options = "B1"
                },
                new AttributeOption
                {
                    Id = 4,
                    AttributeId = 7,
                    Options = "B2"
                },
                new AttributeOption
                {
                    Id = 5,
                    AttributeId = 7,
                    Options = "C1"
                },
                new AttributeOption
                {
                    Id = 6,
                    AttributeId = 7,
                    Options = "C2"
                }
            );
        });

        builder.Entity<UserAttribute>(entity =>
        {
            entity.ToTable("user_attributes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.UserId)
                .HasColumnName("user")
                .IsRequired();

            entity.Property(x => x.AttributeId)
                .HasColumnName("attribute")
                .IsRequired();

            entity.Property(x => x.Value)
                .HasColumnName("value")
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Attribute)
                .WithMany()
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CompareType>(entity =>
        {
            entity.ToTable("compare_types");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.CompareTypeName)
                .HasColumnName("compare_type")
                .IsRequired();

            entity.HasData(
                new CompareType { Id = 1, CompareTypeName = "Equals" },
                new CompareType { Id = 2, CompareTypeName = "Not equals" },
                new CompareType { Id = 3, CompareTypeName = "Contains" },
                new CompareType { Id = 4, CompareTypeName = "Greater than" },
                new CompareType { Id = 5, CompareTypeName = "Less than" },
                new CompareType { Id = 6, CompareTypeName = "Greater than or equal" },
                new CompareType { Id = 7, CompareTypeName = "Less than or equal" }
            );
        });

        builder.Entity<DataTypeCompareType>(entity =>
        {
            entity.ToTable("data_type_compare_types");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("pk");

            entity.Property(x => x.DataTypeId)
                .HasColumnName("data_type")
                .IsRequired();

            entity.Property(x => x.CompareTypeId)
                .HasColumnName("compare_type")
                .IsRequired();

            entity.HasOne(x => x.DataType)
                .WithMany()
                .HasForeignKey(x => x.DataTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CompareType)
                .WithMany()
                .HasForeignKey(x => x.CompareTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(
                new DataTypeCompareType { Id = 1, DataTypeId = 1, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 2, DataTypeId = 1, CompareTypeId = 2 },
                new DataTypeCompareType { Id = 3, DataTypeId = 1, CompareTypeId = 3 },

                new DataTypeCompareType { Id = 4, DataTypeId = 2, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 5, DataTypeId = 2, CompareTypeId = 2 },
                new DataTypeCompareType { Id = 6, DataTypeId = 2, CompareTypeId = 3 },

                new DataTypeCompareType { Id = 7, DataTypeId = 3, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 8, DataTypeId = 3, CompareTypeId = 2 },
                new DataTypeCompareType { Id = 9, DataTypeId = 3, CompareTypeId = 4 },
                new DataTypeCompareType { Id = 10, DataTypeId = 3, CompareTypeId = 5 },
                new DataTypeCompareType { Id = 11, DataTypeId = 3, CompareTypeId = 6 },
                new DataTypeCompareType { Id = 12, DataTypeId = 3, CompareTypeId = 7 },

                new DataTypeCompareType { Id = 13, DataTypeId = 4, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 14, DataTypeId = 4, CompareTypeId = 2 },
                new DataTypeCompareType { Id = 15, DataTypeId = 4, CompareTypeId = 4 },
                new DataTypeCompareType { Id = 16, DataTypeId = 4, CompareTypeId = 5 },
                new DataTypeCompareType { Id = 17, DataTypeId = 4, CompareTypeId = 6 },
                new DataTypeCompareType { Id = 18, DataTypeId = 4, CompareTypeId = 7 },

                new DataTypeCompareType { Id = 19, DataTypeId = 5, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 20, DataTypeId = 5, CompareTypeId = 2 },
                new DataTypeCompareType { Id = 21, DataTypeId = 5, CompareTypeId = 4 },
                new DataTypeCompareType { Id = 22, DataTypeId = 5, CompareTypeId = 5 },
                new DataTypeCompareType { Id = 23, DataTypeId = 5, CompareTypeId = 6 },
                new DataTypeCompareType { Id = 24, DataTypeId = 5, CompareTypeId = 7 },

                new DataTypeCompareType { Id = 25, DataTypeId = 6, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 26, DataTypeId = 6, CompareTypeId = 2 },

                new DataTypeCompareType { Id = 27, DataTypeId = 7, CompareTypeId = 1 },
                new DataTypeCompareType { Id = 28, DataTypeId = 7, CompareTypeId = 2 }
            );
        });


        builder.Entity<Position>(entity =>
        {
            entity.ToTable("positions");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("pk");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.IsDisplay).HasColumnName("is_display");
        });

        builder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("pk");

            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.IsDisplay).HasColumnName("is_display");
        });

        builder.Entity<Experience>(entity =>
        {
            entity.ToTable("experience");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("pk");

            entity.Property(e => e.PositionId).HasColumnName("fk_position_id");
            entity.Property(e => e.UserId).HasColumnName("fk_user_id");

            entity.Property(e => e.CompanyName).HasColumnName("company_name");

            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Description).HasColumnName("description");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.Experiences)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ExperienceTag>(entity =>
        {
            entity.ToTable("experience_tags");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("pk");

            entity.Property(e => e.ExperienceId).HasColumnName("fk_experience_id");
            entity.Property(e => e.TagId).HasColumnName("fk_tag_id");

            entity.HasOne(e => e.Experience)
                .WithMany(e => e.ExperienceTags)
                .HasForeignKey(e => e.ExperienceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ExperienceTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
