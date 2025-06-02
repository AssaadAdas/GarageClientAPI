using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class FuelType
{
    public int Id { get; set; }

    public string FuelTypeDesc { get; set; } = null!;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
