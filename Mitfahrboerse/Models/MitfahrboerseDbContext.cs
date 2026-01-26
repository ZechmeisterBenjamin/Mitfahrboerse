using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

public partial class MitfahrboerseDbContext : DbContext
{
    public MitfahrboerseDbContext(DbContextOptions<MitfahrboerseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<t_Car> t_Cars { get; set; }
    public virtual DbSet<t_Offer> t_Offers { get; set; }
    public virtual DbSet<t_Person> t_People { get; set; }
    public virtual DbSet<t_PersonRide> t_PersonRides { get; set; }
    public virtual DbSet<t_Position> t_Positions { get; set; }
    public virtual DbSet<t_Ride> t_Rides { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<t_Car>(entity => { entity.HasKey(e => e.CarId).HasName("PK__t_Car__68A0342E0453EE9E"); });

        modelBuilder.Entity<t_Offer>(entity =>
        {
            entity.HasKey(e => new { e.OfferId, e.ValidUntil }).HasName("PK__t_Offer__9C2DC745C1C27462");

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
                        j.HasKey("FK_OfferId", "FK_PersonId", "FK_ValidUntil")
                            .HasName("PK__t_Person__A1AB7C96D5059798");
                        j.ToTable("t_PersonOffer");
                        j.HasIndex(new[] { "FK_PersonId" }, "IX_PersonOffer_PersonId");
                        j.HasIndex(new[] { "FK_ValidUntil" }, "IX_PersonOffer_ValidUntil");
                        j.IndexerProperty<string>("FK_PersonId")
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        j.IndexerProperty<DateOnly>("FK_ValidUntil")
                            .HasDefaultValueSql("(CONVERT([date],dateadd(year,(1),getdate())))");
                    });
        });

        modelBuilder.Entity<t_Person>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PK__t_Person__AA2FFBE50A75FD1B");
        });

        modelBuilder.Entity<t_PersonRide>(entity =>
        {
            entity.HasKey(e => new { e.FK_RideId, e.FK_PersonId }).HasName("PK__t_Person__DA7D13BD7F4B549D");

            entity.ToTable("t_PersonRide");

            entity.HasIndex(e => e.FK_PersonId, "IX_PersonRide_PersonId");

            entity.Property(e => e.FK_RideId).HasColumnName("FK_RideId");
            
            entity.Property(e => e.FK_PersonId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FK_PersonId");

            entity.Property(e => e.IsProcessed)
                .IsRequired()
                .HasDefaultValueSql("0");

            entity.HasOne(d => d.Person).WithMany(p => p.PersonRides)
                .HasForeignKey(d => d.FK_PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__t_PersonR__FK_Pe__4A8310C6");

            entity.HasOne(d => d.Ride).WithMany(p => p.PersonRides)
                .HasForeignKey(d => d.FK_RideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__t_PersonR__FK_Ri__47A6A41B");
        });

        modelBuilder.Entity<t_Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__t_Positi__60BB9A79D35912D1");
        });

        modelBuilder.Entity<t_Ride>(entity =>
        {
            entity.HasKey(e => e.RideId).HasName("PK__t_Ride__C5B8C4F43975CE97");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}