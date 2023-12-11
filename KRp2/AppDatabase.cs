using KR2.Models;
using Microsoft.EntityFrameworkCore;

namespace KR2; 

public class AppDatabase : DbContext {

    private static readonly string ConnectionString =
        "server=localhost;user=dev;password=devPassword;database=krp2";

    public DbSet<ConsultationAppointment> Appointments { get; set; } = null!;
    public DbSet<Consultation> Consultations { get; set; } = null!;
    public DbSet<Owner> Owners { get; set; } = null!;

    public AppDatabase() {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
    }
}