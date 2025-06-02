using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class Country
{
    public int Id { get; set; }

    public string CountryName { get; set; } = null!;

    public string? PhoneExt { get; set; }

    public byte[]? CountryFlag { get; set; }

    public virtual ICollection<ClientProfile> ClientProfiles { get; set; } = new List<ClientProfile>();

    public virtual ICollection<GarageProfile> GarageProfiles { get; set; } = new List<GarageProfile>();
}
