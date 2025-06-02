using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class Manufacturer
{
    public int Id { get; set; }

    public string ManufacturerDesc { get; set; } = null!;

    public byte[]? ManufacturerLogo { get; set; }

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
