using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;

namespace DataAL.Models;

public partial class HmsAContext : DbContext
{
    

    public HmsAContext(DbContextOptions<HmsAContext> options)
        : base(options)
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            optionsBuilder
                .UseSqlServer(configuration.GetConnectionString("HMS_A"))
                .EnableSensitiveDataLogging() // Enable sensitive data logging
                .LogTo(Console.WriteLine, LogLevel.Information); // Log to console or a file as needed
        }
    }
    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Disease> Diseases { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

    public virtual DbSet<DoctorDetail> DoctorDetails { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<HospitalStatistic> HospitalStatistics { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PharmacyProduct> PharmacyProducts { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<ResetToken> ResetTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA2B5056574");

            entity.ToTable(tb => tb.HasTrigger("trg_CheckFollowUpAppointment"));

            entity.HasIndex(e => e.AppointmentDate, "IX_Appointments_AppointmentDate");

            entity.HasIndex(e => e.DepartmentId, "IX_Appointments_DepartmentID");

            entity.HasIndex(e => e.DoctorId, "IX_Appointments_DoctorID");

            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate }, "IX_Appointments_DoctorID_AppointmentDate");

            entity.HasIndex(e => e.PatientId, "IX_Appointments_PatientID");

            entity.HasIndex(e => e.Status, "IX_Appointments_Status");

            entity.Property(e => e.AppointmentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.FollowUpAppointmentId).HasColumnName("FollowUpAppointmentID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.RescheduledAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Department).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Depar__6A30C649");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Docto__68487DD7");

            entity.HasOne(d => d.FollowUpAppointment).WithMany(p => p.InverseFollowUpAppointment)
                .HasForeignKey(d => d.FollowUpAppointmentId)
                .HasConstraintName("FK__Appointme__Follo__6B24EA82");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Patie__693CA210");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__A17F23B8CC371DD3");

            entity.Property(e => e.AuditId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AuditID");
            entity.Property(e => e.ChangeDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ChangeType).HasMaxLength(50);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.TableName).HasMaxLength(100);
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__Billing__11F2FC4A3FFBD48B");

            entity.ToTable("Billing");

            entity.Property(e => e.BillId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("BillID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate)
                .HasDefaultValueSql("(dateadd(month,(1),getdate()))")
                .HasColumnType("datetime");
            entity.Property(e => e.IssuedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.Billings)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Billing__OrderID__078C1F06");

            entity.HasOne(d => d.Payment).WithMany(p => p.Billings)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK__Billing__Payment__0B5CAFEA");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD7509E669");

            entity.HasIndex(e => new { e.DepartmentName, e.DepartmentCode }, "IX_Departments_DepartmentName_DepartmentCode");

            entity.HasIndex(e => e.DepartmentCode, "UQ__Departme__6EA8896DA016C128").IsUnique();

            entity.HasIndex(e => e.DepartmentName, "UQ__Departme__D949CC344E564439").IsUnique();

            entity.Property(e => e.DepartmentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
        });

        modelBuilder.Entity<Disease>(entity =>
        {
            entity.HasKey(e => e.DiseaseId).HasName("PK__Diseases__69B533A952F5DA46");

            entity.HasIndex(e => e.DepartmentId, "IX_Diseases_DepartmentID");

            entity.HasIndex(e => e.DiseaseName, "UQ__Diseases__5112584D4C483243").IsUnique();

            entity.Property(e => e.DiseaseId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("DiseaseID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DiseaseName).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Diseases)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Diseases__Depart__5812160E");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Doctors");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<DoctorAvailability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId).HasName("PK__DoctorAv__DA39799134DAB45E");

            entity.ToTable("DoctorAvailability");

            entity.HasIndex(e => new { e.AvailableFrom, e.AvailableTo }, "IX_DoctorAvailability_AvailableFrom_AvailableTo");

            entity.HasIndex(e => e.DoctorId, "IX_DoctorAvailability_DoctorID");

            entity.Property(e => e.AvailabilityId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AvailabilityID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorAvailabilities)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK__DoctorAva__Docto__628FA481");
        });

        modelBuilder.Entity<DoctorDetail>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__DoctorDe__2DC00EDFA5E37025");

            entity.HasIndex(e => e.DepartmentId, "IX_DoctorDetails_DepartmentID");

            entity.HasIndex(e => new { e.Specialization, e.DepartmentId }, "IX_DoctorDetails_Specialization_DepartmentID");

            entity.HasIndex(e => e.UserId, "IX_DoctorDetails_UserID");

            entity.Property(e => e.DoctorId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("DoctorID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Qualification).HasMaxLength(100);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Department).WithMany(p => p.DoctorDetails)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__DoctorDet__Depar__5DCAEF64");

            entity.HasOne(d => d.User).WithMany(p => p.DoctorDetails)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DoctorDet__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorLogId).HasName("PK__ErrorLog__D65247E2ED58528E");

            entity.ToTable("ErrorLog");

            entity.Property(e => e.ErrorLogId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ErrorLogID");
            entity.Property(e => e.ErrorDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<HospitalStatistic>(entity =>
        {
            entity.HasKey(e => e.StatisticId).HasName("PK__Hospital__367DEB370F6D9A11");

            entity.HasIndex(e => new { e.ReportMonth, e.ReportYear }, "IX_HospitalStatistics_ReportMonth_ReportYear");

            entity.HasIndex(e => e.ReportDate, "UQ__Hospital__826382E8CF4E746C").IsUnique();

            entity.Property(e => e.StatisticId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("StatisticID");
            entity.Property(e => e.AppointmentIncome)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.AvailableDoctors).HasDefaultValue(0);
            entity.Property(e => e.AveragePatientAge)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.AverageWaitingTime)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PharmacyIncome)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalDoctors).HasDefaultValue(0);
            entity.Property(e => e.TotalFemalePatients).HasDefaultValue(0);
            entity.Property(e => e.TotalIncome).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalMalePatients).HasDefaultValue(0);
            entity.Property(e => e.TotalOtherGenderPatients).HasDefaultValue(0);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Images__7516F4EC2058CBDC");

            entity.HasIndex(e => e.EntityId, "IX_Images_EntityID");

            entity.Property(e => e.ImageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ImageID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EntityId).HasColumnName("EntityID");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.MedicalRecordId).HasName("PK__MedicalR__4411BBC2FFD45832");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateMedicalRecordsTimestamp"));

            entity.HasIndex(e => e.DiseaseId, "IX_MedicalRecords_DiseaseID");

            entity.HasIndex(e => e.DoctorId, "IX_MedicalRecords_DoctorID");

            entity.HasIndex(e => e.PatientId, "IX_MedicalRecords_PatientID");

            entity.Property(e => e.MedicalRecordId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("MedicalRecordID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiseaseId).HasColumnName("DiseaseID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Disease).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.DiseaseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__MedicalRe__Disea__74AE54BC");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MedicalRe__Docto__73BA3083");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__MedicalRe__Patie__72C60C4A");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E3272C72729");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_Notifications_UserID_IsRead");

            entity.Property(e => e.NotificationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("NotificationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__2A164134");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFFC024506");

            entity.ToTable(tb => tb.HasTrigger("trg_AutomaticBilling"));

            entity.HasIndex(e => new { e.OrderDate, e.Status }, "IX_Orders_OrderDate_Status");

            entity.HasIndex(e => e.PatientId, "IX_Orders_PatientID");

            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrderID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DoctorFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PrescriptionId).HasColumnName("PrescriptionID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Patient).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Orders__PatientI__06CD04F7");

            entity.HasOne(d => d.Prescription).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("FK__Orders__Prescrip__0A9D95DB");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C88CBA743");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_ManageStock");
                    tb.HasTrigger("trg_ValidateOrderTotal");
                });

            entity.HasIndex(e => e.OrderId, "IX_OrderDetails_OrderID");

            entity.HasIndex(e => e.ProductId, "IX_OrderDetails_ProductID");

            entity.Property(e => e.OrderDetailId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__Order__123EB7A3");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderDeta__Produ__1332DBDC");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Patients");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58BDD2BA82");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_CheckOrderOrAppointment");
                    tb.HasTrigger("trg_UpdateBillingStatusOnPayment");
                });

            entity.HasIndex(e => new { e.OrderId, e.AppointmentId }, "IX_Payments_OrderID_AppointmentID");

            entity.HasIndex(e => e.UserId, "IX_Payments_UserID");

            entity.Property(e => e.PaymentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Payments__Appoin__1CBC4616");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Payments__OrderI__1BC821DD");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Payments__UserID__17F790F9");
        });

        modelBuilder.Entity<PharmacyProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Pharmacy__B40CC6ED93124EB6");

            entity.HasIndex(e => e.Name, "IX_PharmacyProducts_Name");

            entity.HasIndex(e => e.Stock, "IX_PharmacyProducts_Stock");

            entity.HasIndex(e => new { e.Stock, e.RequiresPrescription }, "IX_PharmacyProducts_Stock_RequiresPrescription");

            entity.HasIndex(e => e.Name, "UQ__Pharmacy__737584F624D126DC").IsUnique();

            entity.Property(e => e.ProductId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProductID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RequiresPrescription).HasDefaultValue(false);
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.PrescriptionDetailId).HasName("PK__Prescrip__6DB7668AE13D71DC");

            entity.HasIndex(e => e.MedicalRecordId, "IX_PrescriptionDetails_MedicalRecordID");

            entity.HasIndex(e => e.ProductId, "IX_PrescriptionDetails_ProductID");

            entity.Property(e => e.PrescriptionDetailId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PrescriptionDetailID");
            entity.Property(e => e.MedicalRecordId).HasColumnName("MedicalRecordID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.MedicalRecord).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicalRecordId)
                .HasConstraintName("FK__Prescript__Medic__00200768");

            entity.HasOne(d => d.Product).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Prescript__Produ__01142BA1");
        });

        modelBuilder.Entity<ResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__ResetTok__658FEE8AD061CA52");

            entity.HasIndex(e => e.Token, "UQ__ResetTok__1EB4F817FB33E6BD").IsUnique();

            entity.Property(e => e.TokenId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("TokenID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Expiration).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ResetToke__UserI__57A801BA");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A8E7F822B");

            entity.HasIndex(e => e.RoleName, "IX_Roles_RoleName");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160B06EFB89").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC06FCC1D2");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_UpdateTimestamp");
                    tb.HasTrigger("trg_Users_SoftDelete");
                });

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => new { e.Email, e.RoleId, e.IsActive }, "IX_Users_Email_RoleID_IsActive");

            entity.HasIndex(e => new { e.Name, e.DateOfBirth }, "IX_Users_Name_DateOfBirth");

            entity.HasIndex(e => e.RoleId, "IX_Users_RoleID");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__85FB4E38E8403702").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053480D332B4").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(100);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleID__3E52440B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
