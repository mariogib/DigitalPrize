using DigitalPrizes.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Data;

/// <summary>
/// Application database context for Digital Prizes.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExternalUser> ExternalUsers => Set<ExternalUser>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<RegistrationField> RegistrationFields => Set<RegistrationField>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<RegistrationAnswer> RegistrationAnswers => Set<RegistrationAnswer>();
    public DbSet<PrizeType> PrizeTypes => Set<PrizeType>();
    public DbSet<PrizePool> PrizePools => Set<PrizePool>();
    public DbSet<Prize> Prizes => Set<Prize>();
    public DbSet<PrizeAward> PrizeAwards => Set<PrizeAward>();
    public DbSet<PrizeRedemption> PrizeRedemptions => Set<PrizeRedemption>();
    public DbSet<Otp> Otps => Set<Otp>();
    public DbSet<SmsMessage> SmsMessages => Set<SmsMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure ExternalUser
        modelBuilder.Entity<ExternalUser>(entity =>
        {
            entity.HasKey(e => e.ExternalUserId);
            entity.HasIndex(e => e.CellNumber).IsUnique();
            entity.Property(e => e.CellNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Competition
        modelBuilder.Entity<Competition>(entity =>
        {
            entity.HasKey(e => e.CompetitionId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure RegistrationField
        modelBuilder.Entity<RegistrationField>(entity =>
        {
            entity.HasKey(e => e.RegistrationFieldId);
            entity.Property(e => e.FieldName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FieldType).HasMaxLength(50).HasDefaultValue("Text");

            entity.HasOne(e => e.Competition)
                .WithMany(c => c.RegistrationFields)
                .HasForeignKey(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Registration
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId);
            entity.Property(e => e.CellNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Competition)
                .WithMany(c => c.Registrations)
                .HasForeignKey(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ExternalUser)
                .WithMany(u => u.Registrations)
                .HasForeignKey(e => e.ExternalUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.CompetitionId, e.CellNumber }).IsUnique();
        });

        // Configure RegistrationAnswer
        modelBuilder.Entity<RegistrationAnswer>(entity =>
        {
            entity.HasKey(e => e.RegistrationAnswerId);

            entity.HasOne(e => e.Registration)
                .WithMany(r => r.RegistrationAnswers)
                .HasForeignKey(e => e.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RegistrationField)
                .WithMany(f => f.RegistrationAnswers)
                .HasForeignKey(e => e.RegistrationFieldId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure PrizeType
        modelBuilder.Entity<PrizeType>(entity =>
        {
            entity.HasKey(e => e.PrizeTypeId);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure PrizePool
        modelBuilder.Entity<PrizePool>(entity =>
        {
            entity.HasKey(e => e.PrizePoolId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Competition)
                .WithOne(c => c.PrizePool)
                .HasForeignKey<PrizePool>(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Prize
        modelBuilder.Entity<Prize>(entity =>
        {
            entity.HasKey(e => e.PrizeId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.MonetaryValue).HasPrecision(18, 2);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PrizePool)
                .WithMany(p => p.Prizes)
                .HasForeignKey(e => e.PrizePoolId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PrizeType)
                .WithMany(t => t.Prizes)
                .HasForeignKey(e => e.PrizeTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PrizeAward
        modelBuilder.Entity<PrizeAward>(entity =>
        {
            entity.HasKey(e => e.PrizeAwardId);
            entity.Property(e => e.CellNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AwardedBySubjectId).HasMaxLength(255);
            entity.Property(e => e.AwardMethod).HasMaxLength(50);
            entity.Property(e => e.NotificationChannel).HasMaxLength(50);
            entity.Property(e => e.NotificationStatus).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Awarded");
            entity.Property(e => e.ExternalReference).HasMaxLength(255);
            entity.Property(e => e.AwardedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Prize)
                .WithMany(p => p.PrizeAwards)
                .HasForeignKey(e => e.PrizeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Competition)
                .WithMany(c => c.PrizeAwards)
                .HasForeignKey(e => e.CompetitionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ExternalUser)
                .WithMany(u => u.PrizeAwards)
                .HasForeignKey(e => e.ExternalUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure PrizeRedemption
        modelBuilder.Entity<PrizeRedemption>(entity =>
        {
            entity.HasKey(e => e.PrizeRedemptionId);
            entity.Property(e => e.RedeemedChannel).HasMaxLength(50);
            entity.Property(e => e.RedeemedFromIp).HasMaxLength(50);
            entity.Property(e => e.RedemptionStatus).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.RedeemedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PrizeAward)
                .WithOne(a => a.PrizeRedemption)
                .HasForeignKey<PrizeRedemption>(e => e.PrizeAwardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Otp
        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.OtpId);
            entity.Property(e => e.CellNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Purpose).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MaxAttempts).HasDefaultValue(3);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.CellNumber, e.Purpose, e.IsUsed });
        });

        // Configure SmsMessage
        modelBuilder.Entity<SmsMessage>(entity =>
        {
            entity.HasKey(e => e.SmsMessageId);
            entity.Property(e => e.CellNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.MessageType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.ProviderReference).HasMaxLength(255);
            entity.Property(e => e.ProviderResponse).HasMaxLength(1000);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubjectId).HasMaxLength(255);
            entity.Property(e => e.SubjectName).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
