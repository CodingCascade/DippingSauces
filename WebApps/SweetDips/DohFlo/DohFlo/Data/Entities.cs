using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace Dohflo.Data
{
    // A timestamp for audit
    public abstract class AuditedEntity
    {
        // CreatedAt/UpdatedAt fields map to SQL datetime2 by default on SQL server provider
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Add time when inserted
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Time updated when the data gets updated
    }

    // Start creating tables here

    // User table ----------------------------------------------
    [Index(nameof(Email), IsUnique = true)]
    public class User : AuditedEntity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; } = "";

        [MaxLength(100)]
        public string LastName { get; set; } = "";

        [MaxLength(150)]
        public string DisplayName { get; set; } = "";

        [MaxLength(255)]
        public string Email { get; set; } = "";//string<<unique>>

    }

    // Table Accounts -------------------------------------
    [Index(nameof(UserId), nameof(Name), IsUnique = true)] // unique(UserId, Name)
    public class Account : AuditedEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } //FK --> User table
        public User? User { get; set; }

        [MaxLength(120)]
        public string Name { get; set; } = "";

        [MaxLength(50)]
        public string Type { get; set; } = ""; // e.g., hecking, Savings, CreditCard

        [MaxLength(200)]
        public string Institution { get; set; } = "";

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "USD";

        public bool IsClosed { get; set; } = false;
    }

    //  Payee table -------------------------------------------------
    [Index(nameof(UserId), nameof(Name), IsUnique = true)] // unique(UserId, Name)
    public class Payee : AuditedEntity
    {
        [Key]
        public int Id { get; set; } //PK

        public int UserId { get; set; }
        public User? User { get; set; } // FK --> Users

        [MaxLength(160)]
        public string Name { get; set; } = "";

        [MaxLength(160)]
        public string NormalizedName { get; set; } = ""; // Store upper/trim if you want
    }

    // Category Table -----------------------------------------
    [Index(nameof(UserId), nameof(Name), IsUnique = true)] // unique(UserId, Name)
    public class Category : AuditedEntity
    {
        [Key]
        public int Id { get; set; } // PK

        public int UserId { get; set; } // FK --> Users
        public User? User { get; set;  }

        [MaxLength(140)]
        public string Name { get; set; } = "";

        public int? ParentCategoryId { get; set; } // self FK (nullable)
        public Category? ParentCategory { get; set; }

        [MaxLength(40)]
        public string CatType { get; set; } = "Expense"; // "Expense" | "Income" | "Transfer"

    }

    // Tag table -----------------------------------------------
    [Index(nameof(UserId), nameof(Name), IsUnique = true)]
    public class Tag : AuditedEntity
    {
        [Key]
        public int Id { get; set; } // PK

        public int UserId { get; set; } // FK ---> the User Table
        public User? User { get; set; }

        [MaxLength(140)]
        public string Name { get; set; } = "";
    }

    // Transaction table -----------------------------
    public class Transaction : AuditedEntity
    {
        [Key]
        public int Id { get; set; } // PK

        public int UserId { get; set; } // FK --> User table
        public User? User { get; set; }

        public int AccountId { get; set; } // FK --> Account table
        public Account? Account { get; set; }

        public int? PayeeId { get; set; } // FK --> Payee table (nullable if needed)
        public Payee? Payee { get; set; }

        public int? CategoryId { get; set; } // FK --> Category table (nullable when split)
        public Category? Category { get; set; }

        [Precision(19, 4)]
        public decimal Amount { get; set; } // decimal(19,4)

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "USD";

        public DateTime Date { get; set; } // the transaction date
        public DateTime? ClearedData { get; set; } // nullable
        public string? Notes { get; set; } // long text is ok with (nvarchar(max))

        public bool IsPending { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public List<TransactionCatSplit> Splits { get; set; } = new(); // one-to-many (optional)
        public List<TransactionTag> TransactionTags { get; set; } = new();
    }

     // TransactionCatSplit table --------------------------------------
    public class TransactionCatSplit : AuditedEntity
    {
        [Key]
        public int Id { get; set; } //PK

        public int TransactionId { get; set; } // FK --> Transaction table
        public Transaction? Transaction { get; set; }

        public int CategoryId { get; set; } // --> Category table
        public Category? Category { get; set; }

        [Precision(19,4)]
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    // Transaction Tag: many to many relationship (Transactions <-> Tags) -------------------------
    // Composite primary key defined in OnModelCreating
    public class TransactionTag
    { 
        public int TransactionId { get; set; } // PK/FK --> Transaction table
        public Transaction? Transaction { get; set; }

        public int TagId { get; set; } // PK/FK --> Tag table
        public Tag? Tag { get; set; }

    }
}