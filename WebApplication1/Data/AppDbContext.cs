using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BusinessUnit> BusinessUnits { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<DamageReport> DamageReports { get; set; }
        public DbSet<RepairSchedule> RepairSchedules { get; set; }
        public DbSet<KYTReport> KYTReports { get; set; }
        public DbSet<RepairLog> RepairLogs { get; set; }
        public DbSet<KYTReportTechnician> KYTReportTechnicians { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KYTReport>()
                .HasMany(kr => kr.Technicians)
                .WithMany(u => u.AssignedKYTReports)
                .UsingEntity<KYTReportTechnician>(
                    j => j.HasOne(kt => kt.Technician).WithMany().HasForeignKey(kt => kt.TechnicianId),
                    j => j.HasOne(kt => kt.KYTReport).WithMany().HasForeignKey(kt => kt.KytId),
                    j =>
                    {
                        j.HasKey(kt => new { kt.KytId, kt.TechnicianId });
                        j.ToTable("kyt_report_technicians");
                    }
                );

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Machine>() 
                .Property(m => m.Status)
                .HasDefaultValue("operasional");

            modelBuilder.Entity<DamageReport>()
                .Property(dr => dr.ReportedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<KYTReport>()
                .Property(kr => kr.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<DamageReport>()
                .HasIndex(dr => dr.MachineId);

            modelBuilder.Entity<RepairSchedule>()
                .HasIndex(rs => rs.ReportId);

            modelBuilder.Entity<Machine>() 
                .HasIndex(m => m.ProductionLineId);

            modelBuilder.Entity<RepairSchedule>()
                .HasOne(rs => rs.DamageReport)        
                .WithMany(dr => dr.RepairSchedules) 
                .HasForeignKey(rs => rs.ReportId)     
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DamageReport>()
                .HasOne(dr => dr.Machine)
                .WithMany(m => m.DamageReports)
                .HasForeignKey(dr => dr.MachineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DamageReport>()
                .HasOne(dr => dr.Reporter)
                .WithMany(u => u.ReportedDamageReports)
                .HasForeignKey(dr => dr.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairSchedule>()
                .HasOne(rs => rs.Creator)
                .WithMany(u => u.CreatedRepairSchedules)
                .HasForeignKey(rs => rs.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KYTReport>()
                .HasOne(kr => kr.Creator)
                .WithMany(u => u.CreatedKYTReports)
                .HasForeignKey(kr => kr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KYTReport>()
                .HasOne(kr => kr.RepairSchedule)
                .WithMany(rs => rs.KYTReports)
                .HasForeignKey(kr => kr.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KYTReport>()
                .HasOne(kr => kr.Reviewer)
                .WithMany(u => u.ReviewedKYTReports)
                .HasForeignKey(kr => kr.ReviewedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<RepairLog>()
                .HasOne(rl => rl.DamageReport)       
                .WithMany(dr => dr.RepairLogs)       
                .HasForeignKey(rl => rl.ReportId)    
                .OnDelete(DeleteBehavior.Cascade);  

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.HasOne(d => d.User)
                      .WithMany(p => p.Notifications)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<BusinessUnit>().ToTable("business_units");
            modelBuilder.Entity<ProductionLine>().ToTable("production_lines");
            modelBuilder.Entity<Machine>().ToTable("machines"); 
            modelBuilder.Entity<DamageReport>().ToTable("damage_reports");
            modelBuilder.Entity<RepairSchedule>().ToTable("repair_schedules");
            modelBuilder.Entity<KYTReport>().ToTable("kyt_reports");
            modelBuilder.Entity<RepairLog>().ToTable("repair_logs");
            modelBuilder.Entity<Notification>().ToTable("notifications");
            modelBuilder.Entity<KYTReportTechnician>().ToTable("kyt_report_technicians");
        }
    }
}