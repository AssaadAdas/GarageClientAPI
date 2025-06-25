using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class ClientPremiumRegistration
{
    public int Id { get; set; }

    public int Clientid { get; set; }

    public DateTime Registerdate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ClientProfile? Client { get; set; } = null!;
}
