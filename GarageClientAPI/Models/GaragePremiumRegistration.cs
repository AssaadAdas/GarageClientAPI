using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class GaragePremiumRegistration
{
    public int Id { get; set; }

    public int Garageid { get; set; }

    public DateTime Registerdate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public virtual GarageProfile? Garage { get; set; } = null!;
}
