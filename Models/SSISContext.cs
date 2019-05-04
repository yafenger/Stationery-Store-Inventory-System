using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ADProjectBase2.Models
{
    public partial class SSISContext : DbContext
    {
        public virtual DbSet<AdjustmentVoucher> AdjustmentVoucher { get; set; }
        public virtual DbSet<AVDetails> Avdetails { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CollectionPoint> CollectionPoint { get; set; }
        public virtual DbSet<Delegation> Delegation { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<DeptRequest> DeptRequest { get; set; }
        public virtual DbSet<Item> Item { get; set; }
        public virtual DbSet<Login> Login { get; set; }
        public virtual DbSet<MyUser> MyUser { get; set; }
        public virtual DbSet<PODetails> Podetails { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrder { get; set; }
        public virtual DbSet<Request> Request { get; set; }
        public virtual DbSet<RequestDetails> RequestDetails { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Supplier> Supplier { get; set; }

        public SSISContext(DbContextOptions<SSISContext> options)
    : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=.;Database=SSIS;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdjustmentVoucher>(entity =>
            {
                entity.HasKey(e => e.AdjustId);

                entity.Property(e => e.AdjustId).HasColumnName("AdjustID");

                entity.Property(e => e.Status).HasMaxLength(15);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AdjustmentVoucher)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AdjustmentVoucher_User");
            });

            modelBuilder.Entity<AVDetails>(entity =>
            {
                entity.HasKey(e => e.AVDid);

                entity.ToTable("AVDetails");

                entity.Property(e => e.AVDid).HasColumnName("AVDID");

                entity.Property(e => e.AdjustId).HasColumnName("AdjustID");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasColumnName("ItemID")
                    .HasMaxLength(10);

                entity.Property(e => e.Operations).HasMaxLength(1);

                entity.Property(e => e.Remarks).HasMaxLength(50);

                entity.HasOne(d => d.Adjust)
                    .WithMany(p => p.Avdetails)
                    .HasForeignKey(d => d.AdjustId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AV Details_AdjustmentVoucher");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Avdetails)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AV Details_Item");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CatId);

                entity.Property(e => e.CatId).HasColumnName("CatID");

                entity.Property(e => e.CatName)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Supplier1Navigation)
                    .WithMany(p => p.CategorySupplier1Navigation)
                    .HasForeignKey(d => d.Supplier1)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Category_Supplier1");

                entity.HasOne(d => d.Supplier2Navigation)
                    .WithMany(p => p.CategorySupplier2Navigation)
                    .HasForeignKey(d => d.Supplier2)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Category_Supplier2");

                entity.HasOne(d => d.Supplier3Navigation)
                    .WithMany(p => p.CategorySupplier3Navigation)
                    .HasForeignKey(d => d.Supplier3)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Category_Supplier3");
            });

            modelBuilder.Entity<CollectionPoint>(entity =>
            {
                entity.HasKey(e => e.Cpid);

                entity.Property(e => e.Cpid).HasColumnName("CPID");

                entity.Property(e => e.Details).HasMaxLength(100);

                entity.Property(e => e.ImgUrl).HasMaxLength(100);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Delegation>(entity =>
            {
                entity.Property(e => e.DelegationId).HasColumnName("DelegationID");

                entity.Property(e => e.DeptId).HasColumnName("DeptID");

                entity.Property(e => e.Enddate).HasColumnType("datetime");

                entity.Property(e => e.Startdate).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Delegation)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Delegation_Department");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Delegation)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Delegation_User");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DeptId);

                entity.Property(e => e.DeptId).HasColumnName("DeptID");

                entity.Property(e => e.Cpid).HasColumnName("CPID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Cp)
                    .WithMany(p => p.Department)
                    .HasForeignKey(d => d.Cpid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Department_Collection Point");
            });

            modelBuilder.Entity<DeptRequest>(entity =>
            {
                entity.HasKey(e => e.DeptReqId);

                entity.Property(e => e.DeptReqId).HasColumnName("DeptReqID");

                entity.Property(e => e.DeptId).HasColumnName("DeptID");

                entity.Property(e => e.GeneratedTime).HasColumnType("datetime");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasColumnName("ItemID")
                    .HasMaxLength(10);

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.DeptRequest)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeptRequest_Department");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.DeptRequest)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeptRequest_Item");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(e => e.ItemId)
                    .HasColumnName("ItemID")
                    .HasMaxLength(10)
                    .ValueGeneratedNever();

                entity.Property(e => e.CatId).HasColumnName("CatID");

                entity.Property(e => e.ItemName).HasMaxLength(30);

                entity.Property(e => e.Uom)
                    .HasColumnName("UOM")
                    .HasMaxLength(15);

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.Item)
                    .HasForeignKey(d => d.CatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Item_Category");
            });

            modelBuilder.Entity<Login>(entity =>
            {
                entity.HasKey(e => e.Email);

                entity.Property(e => e.Email)
                    .HasMaxLength(30)
                    .ValueGeneratedNever();

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(15);
            });

            modelBuilder.Entity<MyUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.DeptId).HasColumnName("DeptID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(300);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.MyUser)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employee_Department");

                entity.HasOne(d => d.EmailNavigation)
                    .WithMany(p => p.MyUser)
                    .HasForeignKey(d => d.Email)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employee_Login");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.MyUser)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employee_Role");
            });

            modelBuilder.Entity<PODetails>(entity =>
            {
                entity.ToTable("PODetails");

                entity.Property(e => e.PodetailsId).HasColumnName("PODetailsID");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasColumnName("ItemID")
                    .HasMaxLength(10);

                entity.Property(e => e.PoId).HasColumnName("PoID");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Podetails)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PODetails_Item");

                entity.HasOne(d => d.Po)
                    .WithMany(p => p.Podetails)
                    .HasForeignKey(d => d.PoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PODetails_PurchaseOrder");
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.PoId);

                entity.Property(e => e.PoId).HasColumnName("PoID");

                entity.Property(e => e.PurchaseDate).HasColumnType("date");

                entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.PurchaseOrder)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrder_Supplier");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(e => e.RequestId).HasColumnName("RequestID");

                entity.Property(e => e.Approvaltime).HasColumnType("datetime");

                entity.Property(e => e.Remarks).HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(15);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Request)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Request_User");
            });

            modelBuilder.Entity<RequestDetails>(entity =>
            {
                entity.HasKey(e => e.ReqDetailsId);

                entity.Property(e => e.ReqDetailsId).HasColumnName("ReqDetailsID");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasColumnName("ItemID")
                    .HasMaxLength(10);

                entity.Property(e => e.RequestId).HasColumnName("RequestID");

                entity.Property(e => e.Type).HasMaxLength(15);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Request Details_Item");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Request Details_Request ");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId)
                    .HasColumnName("RoleID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasOne(d => d.RoleNavigation)
                    .WithOne(p => p.InverseRoleNavigation)
                    .HasForeignKey<Role>(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_Role");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

                entity.Property(e => e.SupplierEmail).HasColumnType("nchar(30)");

                entity.Property(e => e.SupplierName).HasMaxLength(30);
            });
        }
    }
}
