using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class ClientReminder
{
    public int Id { get; set; }

    public int Clientid { get; set; }

    public DateTime? ReminderDate { get; set; }

    public string? Notes { get; set; }

    public virtual ClientProfile? Client { get; set; } = null!;
}
