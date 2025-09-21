using System;
using System.Collections.Generic;
using GarageClientAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageClientAPI.Data;

public partial class GarageClientContext : DbContext
{
    public GarageClientContext()
    {
    }

    public GarageClientContext(DbContextOptions<GarageClientContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClientNotification> ClientNotifications { get; set; }

    public virtual DbSet<ClientPaymentMethod> ClientPaymentMethods { get; set; }

    public virtual DbSet<ClientPaymentOrder> ClientPaymentOrders { get; set; }

    public virtual DbSet<ClientPremiumRegistration> ClientPremiumRegistrations { get; set; }

    public virtual DbSet<ClientProfile> ClientProfiles { get; set; }

    public virtual DbSet<ClientReminder> ClientReminders { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<FuelType> FuelTypes { get; set; }

    public virtual DbSet<GaragePaymentMethod> GaragePaymentMethods { get; set; }

    public virtual DbSet<GaragePaymentOrder> GaragePaymentOrders { get; set; }

    public virtual DbSet<GaragePremiumRegistration> GaragePremiumRegistrations { get; set; }

    public virtual DbSet<GarageProfile> GarageProfiles { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<MeassureUnit> MeassureUnits { get; set; }

    public virtual DbSet<PremiumOffer> PremiumOffers { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<ServicesTypeSetUp> ServicesTypeSetUps { get; set; }

    public virtual DbSet<Specialization> Specializations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleAppointment> VehicleAppointments { get; set; }

    public virtual DbSet<VehicleCheck> VehicleChecks { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    public virtual DbSet<VehiclesRefuel> VehiclesRefuels { get; set; }

    public virtual DbSet<VehiclesService> VehiclesServices { get; set; }

    public virtual DbSet<VehiclesServiceType> VehiclesServiceTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("GarageClientConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientNotification>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notes)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Client).WithMany(p => p.ClientNotifications)
                .HasForeignKey(d => d.Clientid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientNotifications_ClientProfiles");
        });

        modelBuilder.Entity<ClientPaymentMethod>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CardHolderName).HasMaxLength(100);
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Cvv)
                .HasMaxLength(10)
                .HasColumnName("CVV");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.PaymentType).HasMaxLength(50);

            entity.HasOne(d => d.Client).WithMany(p => p.ClientPaymentMethods)
                .HasForeignKey(d => d.Clientid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPaymentMethods_ClientProfiles");
        });

        modelBuilder.Entity<ClientPaymentOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClientPa__3214EC079D07E9C8");

            entity.HasIndex(e => e.OrderNumber, "UQ__ClientPa__CAC5E743CBBFBBBB").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.ProcessedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientPaymentOrders)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPaymentOrders_ClientPaymentMethods");

            entity.HasOne(d => d.Curr).WithMany(p => p.ClientPaymentOrders)
                .HasForeignKey(d => d.Currid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPaymentOrders_Currencies");

            entity.HasOne(d => d.PremiumOffer).WithMany(p => p.ClientPaymentOrders)
                .HasForeignKey(d => d.PremiumOfferid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPaymentOrders_PremiumOffers");
        });

        modelBuilder.Entity<ClientPremiumRegistration>(entity =>
        {
            entity.ToTable("ClientPremiumRegistration");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.Registerdate).HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientPremiumRegistrations)
                .HasForeignKey(d => d.Clientid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPremiumRegistration_ClientProfiles");
        });

        modelBuilder.Entity<ClientProfile>(entity =>
        {
            entity.HasIndex(e => new { e.FirstName, e.LastName, e.CountryId }, "NonClusteredIndex-20250522-121728");

            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneExt)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Country).WithMany(p => p.ClientProfiles)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientProfiles_Countries");

            entity.HasOne(d => d.User).WithMany(p => p.ClientProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientProfiles_Users");
        });

        modelBuilder.Entity<ClientReminder>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.ReminderDate).HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientReminders)
                .HasForeignKey(d => d.Clientid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientReminders_ClientProfiles1");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CountryFlag).HasColumnType("image");
            entity.Property(e => e.CountryName).HasMaxLength(50);
            entity.Property(e => e.PhoneExt)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CurrDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FuelType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FuelType");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FuelTypeDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<GaragePaymentMethod>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CardHolderName).HasMaxLength(100);
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Cvv)
                .HasMaxLength(10)
                .HasColumnName("CVV");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.PaymentType).HasMaxLength(50);

            entity.HasOne(d => d.Garage).WithMany(p => p.GaragePaymentMethods)
                .HasForeignKey(d => d.Garageid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaragePaymentMethods_GarageProfiles");
        });

        modelBuilder.Entity<GaragePaymentOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GaragePa__3214EC071BA06F78");

            entity.HasIndex(e => e.OrderNumber, "UQ__GaragePa__CAC5E7439988859B").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.ProcessedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Curr).WithMany(p => p.GaragePaymentOrders)
                .HasForeignKey(d => d.Currid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaragePaymentOrders_Currencies");

            entity.HasOne(d => d.Garage).WithMany(p => p.GaragePaymentOrders)
                .HasForeignKey(d => d.GarageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaragePaymentOrders_GarageProfiles");

            entity.HasOne(d => d.PremiumOffer).WithMany(p => p.GaragePaymentOrders)
                .HasForeignKey(d => d.PremiumOfferid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaragePaymentOrders_PremiumOffers");
        });

        modelBuilder.Entity<GaragePremiumRegistration>(entity =>
        {
            entity.ToTable("GaragePremiumRegistration");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.Registerdate).HasColumnType("datetime");

            entity.HasOne(d => d.Garage).WithMany(p => p.GaragePremiumRegistrations)
                .HasForeignKey(d => d.Garageid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaragePremiumRegistration_GarageProfiles");
        });

        modelBuilder.Entity<GarageProfile>(entity =>
        {
            entity.HasIndex(e => new { e.GarageName, e.CountryId }, "NonClusteredIndex-20250522-121750");

            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GarageName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneExt)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Country).WithMany(p => p.GarageProfiles)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GarageProfiles_Countries");

            entity.HasOne(d => d.Specialization).WithMany(p => p.GarageProfiles)
                .HasForeignKey(d => d.SpecializationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GarageProfiles_Specialization");

            entity.HasOne(d => d.User).WithMany(p => p.GarageProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GarageProfiles_Users");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ManufacturerDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ManufacturerLogo).HasColumnType("image");
        });

        modelBuilder.Entity<MeassureUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MeassureUnit");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MeassureUnitDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PremiumOffer>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrId).HasColumnName("CurrID");
            entity.Property(e => e.PremiumCost).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PremiumDesc)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Curr).WithMany(p => p.PremiumOffers)
                .HasForeignKey(d => d.CurrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PremiumOffers_Currencies");

            entity.HasOne(d => d.UserType).WithMany(p => p.PremiumOffers)
                .HasForeignKey(d => d.UserTypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PremiumOffers_UserTypes");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ServicesTypeSetUp>(entity =>
        {
            entity.ToTable("ServicesTypeSetUp");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.HasOne(d => d.MeassureUnit).WithMany(p => p.ServicesTypeSetUps)
                .HasForeignKey(d => d.MeassureUnitid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServicesTypeSetUp_MeassureUnits");

            entity.HasOne(d => d.ServiceTypes).WithMany(p => p.ServicesTypeSetUps)
                .HasForeignKey(d => d.ServiceTypesid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServicesTypeSetUp_ServiceTypes");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.ServicesTypeSetUps)
                .HasForeignKey(d => d.Vehicleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServicesTypeSetUp_Vehicles");
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Specialization");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.SpecializationDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.UserType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserTypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_UserTypes");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserTypeDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => new { e.VehicleTypeId, e.VehicleName, e.FuelTypeId, e.ClientId }, "NonClusteredIndex-20250522-121631");

            entity.Property(e => e.ChassisNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FuelTypeId).HasColumnName("FuelTypeID");
            entity.Property(e => e.Identification)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LiscencePlate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VehicleName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

            entity.HasOne(d => d.Client).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicles_ClientProfiles");

            entity.HasOne(d => d.FuelType).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.FuelTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicles_FuelType");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicles_Manufacturers");

            entity.HasOne(d => d.MeassureUnit).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.MeassureUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicles_MeassureUnit");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicles_VehicleTypes");
        });

        modelBuilder.Entity<VehicleAppointment>(entity =>
        {
            entity.ToTable("VehicleAppointment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.Note)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleAppointments)
                .HasForeignKey(d => d.Vehicleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VehicleAppointment_Vehicles");

            entity.HasOne(d => d.Garage).WithMany(p => p.VehicleAppointments)
                  .HasForeignKey(d => d.Garageid)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_VehicleAppointment_GarageProfiles");

        });

        modelBuilder.Entity<VehicleCheck>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CarChecks");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CheckStatus)
                .HasMaxLength(3)
                .IsUnicode(false);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleChecks)
                .HasForeignKey(d => d.Vehicleid)
                .HasConstraintName("FK_CarChecks_Vehicles");

            entity.HasOne(d => d.GarageProfile).WithMany(p => p.VehicleChecks)
                .HasForeignKey(d => d.GarageId)
                .HasConstraintName("FK_VehicleChecks_GarageProfiles");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleTypesDesc)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<VehiclesRefuel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CarRefuel");

            entity.ToTable("VehiclesRefuel");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RefuelCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefuleValue).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehiclesRefuels)
                .HasForeignKey(d => d.Vehicleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarRefuel_Vehicles");
        });

        modelBuilder.Entity<VehiclesService>(entity =>
        {
            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.ServiceDate).HasColumnType("datetime");
            entity.Property(e => e.ServiceLocation)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Garage).WithMany(p => p.VehiclesServices)
                .HasForeignKey(d => d.Garageid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarServices_GarageProfiles");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehiclesServices)
                .HasForeignKey(d => d.Vehicleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarServices_Vehicles");
        });

        modelBuilder.Entity<VehiclesServiceType>(entity =>
        {
            entity.HasKey(e => new { e.VehicleServiceId, e.ServiceTypeId }).HasName("PK_CarServiceType");

            entity.ToTable("VehiclesServiceType");

            entity.Property(e => e.Cost).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Notes).HasMaxLength(200);

            entity.HasOne(d => d.Curr).WithMany(p => p.VehiclesServiceTypes)
                .HasForeignKey(d => d.CurrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarServiceType_Currencies");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.VehiclesServiceTypes)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarServiceType_ServiceTypes");

            entity.HasOne(d => d.VehicleService).WithMany(p => p.VehiclesServiceTypes)
                .HasForeignKey(d => d.VehicleServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarServiceType_CarServices");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
