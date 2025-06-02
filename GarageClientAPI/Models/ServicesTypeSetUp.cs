using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class ServicesTypeSetUp
{
    public int Id { get; set; }

    public int ServiceTypesid { get; set; }

    public int ServiceTypesValue { get; set; }

    public int MeassureUnitid { get; set; }

    public virtual MeassureUnit MeassureUnit { get; set; } = null!;

    public virtual ServiceType ServiceTypes { get; set; } = null!;
}
