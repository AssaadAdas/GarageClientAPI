using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class Vehicle
{
    public int Id { get; set; }

    public int VehicleTypeId { get; set; }

    public string VehicleName { get; set; } = null!;

    public int ManufacturerId { get; set; }

    public string Model { get; set; } = null!;

    public string LiscencePlate { get; set; } = null!;

    public int FuelTypeId { get; set; }

    public string ChassisNumber { get; set; } = null!;

    public int MeassureUnitId { get; set; }

    public string? Identification { get; set; }

    public bool Active { get; set; }

    public int ClientId { get; set; }

    public int Odometer { get; set; }

    public virtual ClientProfile? Client { get; set; } = null!;

    public virtual FuelType? FuelType { get; set; } = null!;

    public virtual Manufacturer? Manufacturer { get; set; } = null!;

    public virtual MeassureUnit? MeassureUnit { get; set; } = null!;

    public virtual ICollection<VehicleAppointment>? VehicleAppointments { get; set; } = new List<VehicleAppointment>();

    public virtual ICollection<VehicleCheck>? VehicleChecks { get; set; } = new List<VehicleCheck>();

    public virtual VehicleType? VehicleType { get; set; } = null!;

    public virtual ICollection<VehiclesRefuel>? VehiclesRefuels { get; set; } = new List<VehiclesRefuel>();

    public virtual ICollection<VehiclesService>? VehiclesServices { get; set; } = new List<VehiclesService>();
}
