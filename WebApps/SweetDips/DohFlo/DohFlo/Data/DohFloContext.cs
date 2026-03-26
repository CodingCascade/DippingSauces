using Dohflo.Data;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Data
{
    public class DohFloContext : DbContext
    {
        public DohFloContext(DbContextOptions<DohFloContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Payee> Payees => Set<Payee>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<TransactionCatSplit> TransactionCatSplits => Set<TransactionCatSplit>();
        public DbSet<TransactionTag> TransactionTags => Set<TransactionTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Seed your first user (Boolie Kets)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FirstName = "Boolie",
                LastName = "Kets",
                DisplayName = "Boolie Kets",
                Email = "boolie@dohflo.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Automatically set timestamps whenever SaveChanges is called
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditedEntity>();
            var now = DateTime.UtcNow;

            foreach (var e in entries)
            {
                if(e.State == EntityState.Added)
                {
                    e.Entity.CreatedAt = now;
                    e.Entity.UpdatedAt = now;
                }
                else if(e.State == EntityState.Modified) 
                {
                    e.Entity.UpdatedAt = now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
