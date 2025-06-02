using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class MeassureUnit
{
    public int Id { get; set; }

    public string MeassureUnitDesc { get; set; } = null!;

    public virtual ICollection<ServicesTypeSetUp> ServicesTypeSetUps { get; set; } = new List<ServicesTypeSetUp>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
