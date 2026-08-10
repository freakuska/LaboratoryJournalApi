using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LaboratoryJournal.Models;

namespace LaboratoryJournal.Data
{
    /// <summary>
    /// Контекст базы данных приложения "Лабораторный журнал"
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Таблица экспериментов
        /// </summary>
        public DbSet<Experiment> Experiments { get; set; }

        /// <summary>
        /// Таблица результатов экспериментов
        /// </summary>
        public DbSet<ExperimentResult> ExperimentResults { get; set; }

        /// <summary>
        /// Таблица записей в журнале
        /// </summary>
        public DbSet<JournalEntry> JournalEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация для ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Experiments)
                .WithOne(e => e.Researcher)
                .HasForeignKey(e => e.ResearcherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.JournalEntries)
                .WithOne(j => j.Author)
                .HasForeignKey(j => j.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация для Experiment
            modelBuilder.Entity<Experiment>()
                .HasMany(e => e.Results)
                .WithOne(r => r.Experiment)
                .HasForeignKey(r => r.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Experiment>()
                .HasMany(e => e.JournalEntries)
                .WithOne(j => j.Experiment)
                .HasForeignKey(j => j.ExperimentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для оптимизации поиска
            modelBuilder.Entity<Experiment>()
                .HasIndex(e => e.Title)
                .HasDatabaseName("IX_Experiment_Title");

            modelBuilder.Entity<Experiment>()
                .HasIndex(e => e.ResearcherId)
                .HasDatabaseName("IX_Experiment_ResearcherId");

            modelBuilder.Entity<Experiment>()
                .HasIndex(e => e.StartDate)
                .HasDatabaseName("IX_Experiment_StartDate");

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(j => j.ExperimentId)
                .HasDatabaseName("IX_JournalEntry_ExperimentId");

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(j => j.AuthorId)
                .HasDatabaseName("IX_JournalEntry_AuthorId");

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(j => j.Title)
                .HasDatabaseName("IX_JournalEntry_Title");

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(j => j.Tags)
                .HasDatabaseName("IX_JournalEntry_Tags");

            // Ограничения на свойства
            modelBuilder.Entity<Experiment>()
                .Property(e => e.Title)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Experiment>()
                .Property(e => e.Description)
                .HasMaxLength(2000);

            modelBuilder.Entity<JournalEntry>()
                .Property(j => j.Title)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<JournalEntry>()
                .Property(j => j.Content)
                .HasMaxLength(5000);

            modelBuilder.Entity<ExperimentResult>()
                .Property(r => r.Name)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<ExperimentResult>()
                .Property(r => r.Value)
                .HasMaxLength(1000);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(255);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Position)
                .HasMaxLength(255);
        }
    }
}
