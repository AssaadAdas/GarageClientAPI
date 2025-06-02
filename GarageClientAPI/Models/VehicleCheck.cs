using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class VehicleCheck
{
    public int Id { get; set; }

    public int? Vehicleid { get; set; }

    public string? CheckStatus { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
