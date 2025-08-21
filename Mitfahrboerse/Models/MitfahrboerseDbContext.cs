using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

public partial class MitfahrboerseDbContext : DbContext
{
    public MitfahrboerseDbContext()
    {
    }

    public MitfahrboerseDbContext(DbContextOptions<MitfahrboerseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<t_Car> t_Cars { get; set; }

    public virtual DbSet<t_Offer> t_Offers { get; set; }

    public virtual DbSet<t_Person> t_People { get; set; }

    public virtual DbSet<t_Position> t_Positions { get; set; }

    public virtual DbSet<t_Ride> t_Rides { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=Mitfahrboerse;User ID=sa;Password=Password123!;MultipleActiveResultSets=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<t_Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__t_Car__68A0342E5D79D025");

            entity.Property(e => e.CarId).ValueGeneratedNever();

            entity.HasOne(d => d.FK_Owner_Person).WithMany(p => p.t_Cars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__t_Car__FK_Owner___5AEE82B9");
        });

        modelBuilder.Entity<t_Offer>(entity =>
        {
            entity.HasKey(e => new { e.OfferId, e.ValidUntil }).HasName("PK__t_Offer__9C2DC74548CBFE42");

            entity.Property(e => e.ValidUntil).HasDefaultValueSql("(CONVERT([date],dateadd(year,(1),getdate())))");

            entity.HasMany(d => d.FK_People).WithMany(p => p.t_Offers)
                .UsingEntity<Dictionary<string, object>>(
                    "t_PersonOffer",
                    r => r.HasOne<t_Person>().WithMany()
                        .HasForeignKey("FK_PersonId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__t_PersonO__FK_Pe__44FF419A"),
                    l => l.HasOne<t_Offer>().WithMany()
                        .HasForeignKey("FK_OfferId", "FK_ValidUntil")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__t_PersonOffer__440B1D61"),
                    j =>
                    {
                        j.HasKey("FK_OfferId", "FK_PersonId", "FK_ValidUntil").HasName("PK__t_Person__A1AB7C96D5059798");
                        j.ToTable("t_PersonOffer");
                        j.HasIndex(new[] { "FK_PersonId" }, "IX_PersonOffer_PersonId");
                        j.HasIndex(new[] { "FK_ValidUntil" }, "IX_PersonOffer_ValidUntil");
                        j.IndexerProperty<string>("FK_PersonId")
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        j.IndexerProperty<DateOnly>("FK_ValidUntil").HasDefaultValueSql("(CONVERT([date],dateadd(year,(1),getdate())))");
                    });
        });

        modelBuilder.Entity<t_Person>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PK__t_Person__AA2FFBE50C4D7DC2");
        });

        modelBuilder.Entity<t_Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__t_Positi__60BB9A79C90DA882");

            entity.Property(e => e.PositionId).ValueGeneratedNever();
        });

        modelBuilder.Entity<t_Ride>(entity =>
        {
            entity.HasKey(e => e.RideId).HasName("PK__t_Ride__C5B8C4F43457DE73");

            entity.Property(e => e.RideId).ValueGeneratedNever();
            entity.Property(e => e.RideDateTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FK_Driver_Person).WithMany(p => p.t_Rides)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__t_Ride__FK_Drive__52593CB8");

            entity.HasOne(d => d.FK_EndsAt_Position).WithMany(p => p.t_RideFK_EndsAt_Positions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__t_Ride__FK_EndsA__5165187F");

            entity.HasOne(d => d.FK_StartsAt_Position).WithMany(p => p.t_RideFK_StartsAt_Positions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__t_Ride__FK_Start__5070F446");

            entity.HasMany(d => d.FK_People).WithMany(p => p.FK_Rides)
                .UsingEntity<Dictionary<string, object>>(
                    "t_PersonRide",
                    r => r.HasOne<t_Person>().WithMany()
                        .HasForeignKey("FK_PersonId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__t_PersonR__FK_Pe__5629CD9C"),
                    l => l.HasOne<t_Ride>().WithMany()
                        .HasForeignKey("FK_RideId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__t_PersonR__FK_Ri__5535A963"),
                    j =>
                    {
                        j.HasKey("FK_RideId", "FK_PersonId").HasName("PK__t_Person__DA7D13BD064FFE9D");
                        j.ToTable("t_PersonRide");
                        j.HasIndex(new[] { "FK_PersonId" }, "IX_PersonRide_PersonId");
                        j.IndexerProperty<string>("FK_PersonId")
                            .HasMaxLength(50)
                            .IsUnicode(false);
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
