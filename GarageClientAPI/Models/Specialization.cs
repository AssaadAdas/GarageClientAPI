using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class Specialization
{
    public int Id { get; set; }

    public string SpecializationDesc { get; set; } = null!;

    public virtual ICollection<GarageProfile> GarageProfiles { get; set; } = new List<GarageProfile>();
}
