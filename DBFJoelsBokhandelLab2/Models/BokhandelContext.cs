using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DBFJoelsBokhandelLab2.Models;

public partial class BokhandelContext : DbContext
{
    public BokhandelContext()
    {
    }

    public BokhandelContext(DbContextOptions<BokhandelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Butiker> Butikers { get; set; }

    public virtual DbSet<Böcker> Böckers { get; set; }

    public virtual DbSet<Författare> Författares { get; set; }

    public virtual DbSet<Kunder> Kunders { get; set; }

    public virtual DbSet<LagerSaldo> LagerSaldos { get; set; }

    public virtual DbSet<OrderDetaljer> OrderDetaljers { get; set; }

    public virtual DbSet<Ordrar> Ordrars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Butiker>(entity =>
        {
            entity.HasKey(e => e.ButikId).HasName("PK__Butiker__B5D66BFA311B8396");

            entity.ToTable("Butiker");

            entity.Property(e => e.ButikId).HasColumnName("ButikID");
            entity.Property(e => e.Adress)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ButiksNamn)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Böcker>(entity =>
        {
            entity.HasKey(e => e.Isbn13).HasName("PK__Böcker__3BF79E03C72E8BCB");

            entity.ToTable("Böcker");

            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ISBN13");
            entity.Property(e => e.FörfattareId).HasColumnName("FörfattareID");
            entity.Property(e => e.Genre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Pris).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Språk)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Titel)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Författare).WithMany(p => p.Böckers)
                .HasForeignKey(d => d.FörfattareId)
                .HasConstraintName("FK__Böcker__Författa__5070F446");
        });

        modelBuilder.Entity<Författare>(entity =>
        {
            entity.HasKey(e => e.FörfattareId).HasName("PK__Författa__804114D028A61391");

            entity.ToTable("Författare");

            entity.Property(e => e.FörfattareId).HasColumnName("FörfattareID");
            entity.Property(e => e.Efternamn)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Förnamn)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Kunder>(entity =>
        {
            entity.HasKey(e => e.KundId).HasName("PK__Kunder__F2B5DEACE1579B21");

            entity.ToTable("Kunder");

            entity.HasIndex(e => e.Email, "UQ__Kunder__A9D1053413A497B9").IsUnique();

            entity.Property(e => e.KundId).HasColumnName("KundID");
            entity.Property(e => e.Adress)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Namn)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Ort)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PostNummer)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<LagerSaldo>(entity =>
        {
            entity.HasKey(e => new { e.ButikId, e.Isbn }).HasName("PK__LagerSal__1191B8945491F561");

            entity.ToTable("LagerSaldo");

            entity.Property(e => e.ButikId).HasColumnName("ButikID");
            entity.Property(e => e.Isbn)
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ISBN");

            entity.HasOne(d => d.Butik).WithMany(p => p.LagerSaldos)
                .HasForeignKey(d => d.ButikId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LagerSald__Butik__5535A963");

            entity.HasOne(d => d.IsbnNavigation).WithMany(p => p.LagerSaldos)
                .HasForeignKey(d => d.Isbn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LagerSaldo__ISBN__5629CD9C");
        });

        modelBuilder.Entity<OrderDetaljer>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.Isbn }).HasName("PK__OrderDet__67D788C1BDD2C109");

            entity.ToTable("OrderDetaljer");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Isbn)
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ISBN");
            entity.Property(e => e.PrisVidKöp).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IsbnNavigation).WithMany(p => p.OrderDetaljers)
                .HasForeignKey(d => d.Isbn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDetal__ISBN__6383C8BA");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetaljers)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__628FA481");
        });

        modelBuilder.Entity<Ordrar>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Ordrar__C3905BAFEFDC0CA9");

            entity.ToTable("Ordrar");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ButikId).HasColumnName("ButikID");
            entity.Property(e => e.KundId).HasColumnName("KundID");
            entity.Property(e => e.OrderDatum)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalPris).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Butik).WithMany(p => p.Ordrars)
                .HasForeignKey(d => d.ButikId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ordrar__ButikID__5EBF139D");

            entity.HasOne(d => d.Kund).WithMany(p => p.Ordrars)
                .HasForeignKey(d => d.KundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ordrar__KundID__5DCAEF64");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
