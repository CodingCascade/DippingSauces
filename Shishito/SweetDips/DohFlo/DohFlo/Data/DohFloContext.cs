using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DohFlo.Data
{
    public class DohFloContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DohFloContext(DbContextOptions<DohFloContext> options) : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Payee> Payees => Set<Payee>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<TransactionCatSplit> TransactionCatSplits => Set<TransactionCatSplit>();
        public DbSet<TransactionTag> TransactionTags => Set<TransactionTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToTable("Users");

            // Composite key for join table
            modelBuilder.Entity<TransactionTag>()
                .HasKey(tt => new { tt.TransactionId, tt.TagId });

            // Relationships for Category self-reference
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Amount precision everywhere (also doable via [Precision])
            modelBuilder.Entity<Transaction>()
                .Property(p => p.Amount).HasPrecision(19, 4);
            modelBuilder.Entity<TransactionCatSplit>()
                .Property(p => p.Amount).HasPrecision(19, 4);

            // Cascade delete rules that are safer for finance ledgers
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // Automatically set timestamps whenever SaveChanges is called
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditedEntity>();
            var now = DateTime.UtcNow;

            foreach (var e in entries)
            {
                if (e.State == EntityState.Added)
                {
                    e.Entity.CreatedAt = now;
                    e.Entity.UpdatedAt = now;
                }
                else if (e.State == EntityState.Modified)
                {
                    e.Entity.UpdatedAt = now;
                }
            }

            foreach (var entry in ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}