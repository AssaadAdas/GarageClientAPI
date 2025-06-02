using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int UserTypeid { get; set; }

    public virtual ICollection<ClientProfile> ClientProfiles { get; set; } = new List<ClientProfile>();

    public virtual ICollection<GarageProfile> GarageProfiles { get; set; } = new List<GarageProfile>();

    public virtual UserType UserType { get; set; } = null!;
}
