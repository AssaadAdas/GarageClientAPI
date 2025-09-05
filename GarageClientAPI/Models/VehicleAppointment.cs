using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class VehicleAppointment
{
    public int Id { get; set; }

    public int Vehicleid { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string? Note { get; set; }
    public int Garageid { get; set; }
    
    public virtual Vehicle? Vehicle { get; set; } = null!;
    public virtual GarageProfile? Garage { get; set; } = null!;
}
