using System;
using System.Collections.Generic;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Data;

public partial class DataSyncCoreContext : DbContext
{
    public DataSyncCoreContext()
    {
    }

    public DataSyncCoreContext(DbContextOptions<DataSyncCoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DataMigrationLogger> DataMigrationLoggers { get; set; }

    public virtual DbSet<DoneTable> DoneTables { get; set; }

    public virtual DbSet<ModuleRun> ModuleRuns { get; set; }

    public virtual DbSet<StatusOnlineAuthor> StatusOnlineAuthors { get; set; }

    public virtual DbSet<StatusOnlineChapter> StatusOnlineChapters { get; set; }

    public virtual DbSet<StatusOnlineStory> StatusOnlineStories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=master;User Id=sa;Password=2Secure*Password2;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DataMigrationLogger>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DataMigr__3214EC073299D3B7");

            entity.ToTable("DataMigrationLogger");

            entity.Property(e => e.CreateDto).HasColumnName("CreateDTO");
        });

        modelBuilder.Entity<DoneTable>(entity =>
        {
            entity.HasKey(e => e.TraceId).HasName("PK__DoneTabl__70C9CE9CF33BAEF3");

            entity.ToTable("DoneTable");

            entity.Property(e => e.TraceId)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Value)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ModuleRun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ModuleRu__3214EC07DD58497F");

            entity.ToTable("ModuleRun");

            entity.Property(e => e.LastRun).HasColumnType("datetime");
            entity.Property(e => e.SqlQuery).HasColumnType("text");
        });

        modelBuilder.Entity<StatusOnlineAuthor>(entity =>
        {
            entity.HasKey(e => e.AuthorNameId).HasName("PK_StatusOnline_Author_AuthorNameId");

            entity.ToTable("StatusOnline_Author");

            entity.Property(e => e.AuthorName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<StatusOnlineChapter>(entity =>
        {
            entity.HasKey(e => e.ChapterId).HasName("PK_StatusOnline_Chapter_ChapterId");

            entity.ToTable("StatusOnline_Chapter");

            entity.Property(e => e.ChapterNextChapter).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ChapterPreChapter).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ChapterTags)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ChapterText).IsUnicode(false);
            entity.Property(e => e.ChapterTitle)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ChapterUrl)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StoryId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Story).WithMany(p => p.StatusOnlineChapters)
                .HasForeignKey(d => d.StoryId)
                .HasConstraintName("StatusOnline_Chapter$StatusOnline_Chapter_ibfk_1");
        });

        modelBuilder.Entity<StatusOnlineStory>(entity =>
        {
            entity.HasKey(e => e.StoryId).HasName("PK_StatusOnline_Story_StoryId");

            entity.ToTable("StatusOnline_Story");

            entity.Property(e => e.AuthorNameId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StoryCompleted).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StoryTags)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StoryTitle)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StoryUrlSource)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.AuthorName).WithMany(p => p.StatusOnlineStories)
                .HasForeignKey(d => d.AuthorNameId)
                .HasConstraintName("StatusOnline_Story$StatusOnline_Story_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
