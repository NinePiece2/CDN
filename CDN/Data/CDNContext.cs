using CDN.Data.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CDN.Data;

public class CDNContext : IdentityDbContext<CDNUser>
{
    public CDNContext(DbContextOptions<CDNContext> options)
        : base(options)
    {
    }

    public DbSet<PowerSettings> PowerSettings { get; set; }
    public DbSet<AllowedOrigins> AllowedOrigins { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("dbo");

        // Configure table names
        builder.Entity<CDNUser>().ToTable("users");
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("userroles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("userclaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("userlogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("roleclaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("usertokens");

        // Configure column naming convention - convert all properties to lowercase
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (columnName != null && columnName != columnName.ToLower())
                {
                    property.SetColumnName(columnName.ToLower());
                }
            }
        }

        builder.Entity<PowerSettings>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.ToTable("powersettings");
        });

        builder.Entity<AllowedOrigins>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("allowedorigins");
        });
    }
}
