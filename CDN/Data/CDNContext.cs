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

        builder.Entity<CDNUser>(entity => entity.ToTable("Users"));
        builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
        builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));

        builder.Entity<PowerSettings>(entity =>
        {
            entity.HasKey(e => e.Key);
        });

        builder.Entity<AllowedOrigins>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
