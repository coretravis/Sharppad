using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharpPad.Server.Services.Library.Models;

namespace SharpPad.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<LibraryScript> LibraryScripts { get; set; }
    public DbSet<LibraryScriptPackage> LibraryScriptPackages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        // Configure LibraryScript
        modelBuilder.Entity<LibraryScript>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();

            // Configure one-to-many relationship with NugetPackages.
            // (A shadow foreign key "LibraryScriptId" is created in LibraryScriptPackage.)
            entity.HasMany(e => e.NugetPackages)
                  .WithOne()
                  .HasForeignKey("LibraryScriptId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure LibraryScriptPackage.
        modelBuilder.Entity<LibraryScriptPackage>(entity =>
        {
            entity.HasKey("Id");
            entity.Property(p => p.Id).IsRequired();
            entity.Property(p => p.Version).IsRequired();
        });
    }
}
