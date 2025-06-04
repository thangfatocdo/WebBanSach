using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebBanSach.Models.Entities;

public partial class BookstoreDbContext : DbContext
{
    public BookstoreDbContext()
    {
    }

    public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookImage> BookImages { get; set; }

    public virtual DbSet<BookRating> BookRatings { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<InventoryDetail> InventoryDetails { get; set; }

    public virtual DbSet<InventoryExport> InventoryExports { get; set; }

    public virtual DbSet<InventoryImport> InventoryImports { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Statistic> Statistics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=webbansach.database.windows.net;Initial Catalog=BookstoreDB;Persist Security Info=True;User ID=yendaotg;Password=Thanglolo1090@;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Authors__70DAFC34A58304C2");

            entity.Property(e => e.AuthorName).HasMaxLength(100);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C207AF48A465");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__Books__AuthorId__02FC7413");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Books__CategoryI__03F0984C");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK__Books__Publisher__04E4BC85");
        });

        modelBuilder.Entity<BookImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__BookImag__7516F70CED31ABAF");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);

            entity.HasOne(d => d.Book).WithMany(p => p.BookImages)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_BookImages_Books");
        });

        modelBuilder.Entity<BookRating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__BookRati__FCCDF87CA086A794");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.BookRatings)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookRatin__BookI__01142BA1");

            entity.HasOne(d => d.Customer).WithMany(p => p.BookRatings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookRatin__Custo__02084FDA");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0AEF6253A4");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_CartItems_Books");

            entity.HasOne(d => d.Customer).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_CartItems_Customers");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BF1610BAC");

            entity.Property(e => e.Alias).HasMaxLength(100);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D8B25C300D");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<InventoryDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__Inventor__135C316DDD4116E3");

            entity.ToTable("InventoryDetail");

            entity.Property(e => e.IepId).HasColumnName("Iep_Id");
            entity.Property(e => e.Type).HasMaxLength(10);

            entity.HasOne(d => d.Book).WithMany(p => p.InventoryDetails)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Inventory__BookI__07C12930");

            entity.HasOne(d => d.Iep).WithMany(p => p.InventoryDetails)
                .HasForeignKey(d => d.IepId)
                .HasConstraintName("FK_InventoryDetail_InventoryExport");

            entity.HasOne(d => d.Import).WithMany(p => p.InventoryDetails)
                .HasForeignKey(d => d.ImportId)
                .HasConstraintName("FK__Inventory__Impor__08B54D69");
        });

        modelBuilder.Entity<InventoryExport>(entity =>
        {
            entity.HasKey(e => e.IepId);

            entity.ToTable("InventoryExport");

            entity.Property(e => e.IepId).HasColumnName("Iep_Id");
            entity.Property(e => e.ExportDate)
                .HasColumnType("datetime")
                .HasColumnName("Export_Date");

            entity.HasOne(d => d.Order).WithMany(p => p.InventoryExports)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_InventoryExport_Orders");

            entity.HasOne(d => d.User).WithMany(p => p.InventoryExports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Inventory__UserI__0A9D95DB");
        });

        modelBuilder.Entity<InventoryImport>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__Inventor__869767EA998C2F39");

            entity.ToTable("InventoryImport");

            entity.Property(e => e.ImportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.InventoryImports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Inventory__UserI__0C85DE4D");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFF044D3A8");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ReceiveDate).HasColumnType("datetime");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Orders__Customer__0F624AF8");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK_Orders_PaymentMethods");

            entity.HasOne(d => d.Status).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Orders_OrderStatuses");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__2F302202B0742E44");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItem_Id");
            entity.Property(e => e.BookPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Book).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__OrderItem__BookI__0D7A0286");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderItems_OrderId");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId);

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.Property(e => e.MethodName).HasMaxLength(50);
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("PK__Publishe__4C657FAB4315B49D");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PublisherName).HasMaxLength(150);
        });

        modelBuilder.Entity<Statistic>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Statistic");

            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.TotalBooksSold).HasColumnName("Total_BooksSold");
            entity.Property(e => e.TotalCustomers).HasColumnName("Total_Customers");
            entity.Property(e => e.TotalOrders).HasColumnName("Total_Orders");
            entity.Property(e => e.TotalQuantity).HasColumnName("Total_Quantity");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CEE3A9445");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4C0FF3490").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Admin");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
