using Microsoft.EntityFrameworkCore;
using FinTrack.Domain.Entities;

namespace FinTrack.Infrastructure.Data
{
    /// <summary>
    /// EF Core DbContext for FinTrack application.
    /// Manages all database entities and relationships.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes the DbContext.
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>Transactions DbSet.</summary>
        public DbSet<Transaction> Transactions { get; set; } = null!;

        /// <summary>SharedExpenses DbSet.</summary>
        public DbSet<SharedExpense> SharedExpenses { get; set; } = null!;

        /// <summary>ExpenseParticipants DbSet.</summary>
        public DbSet<ExpenseParticipant> ExpenseParticipants { get; set; } = null!;

        /// <summary>Users DbSet.</summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Configures entity relationships and constraints.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Transaction entity configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedDate);
            });

            // SharedExpense entity configuration
            modelBuilder.Entity<SharedExpense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatorId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.SplitType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.HasMany(e => e.Participants)
                    .WithOne(p => p.SharedExpense)
                    .HasForeignKey(p => p.SharedExpenseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.CreatorId);
                entity.HasIndex(e => e.Status);
            });

            // ExpenseParticipant entity configuration
            modelBuilder.Entity<ExpenseParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SharedExpenseId).IsRequired();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShareAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.SharedExpenseId, e.UserId }).IsUnique();
            });

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}
