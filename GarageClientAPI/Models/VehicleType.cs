using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class VehicleType
{
    public int Id { get; set; }

    public string VehicleTypesDesc { get; set; } = null!;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
