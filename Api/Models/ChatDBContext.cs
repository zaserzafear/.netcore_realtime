using Microsoft.EntityFrameworkCore;

namespace Api.Models;

public partial class ChatDBContext : DbContext
{
    public ChatDBContext(DbContextOptions<ChatDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<tbl_user> tbl_users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<tbl_user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PRIMARY");

            entity.Property(e => e.created_timestamp).HasDefaultValueSql("current_timestamp()");
            entity.Property(e => e.updated_timestamp).ValueGeneratedOnAddOrUpdate();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
