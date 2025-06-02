using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class ClientNotification
{
    public int Id { get; set; }

    public int Clientid { get; set; }

    public string? Notes { get; set; }

    public bool? IsRead { get; set; }

    public virtual ClientProfile Client { get; set; } = null!;
}
