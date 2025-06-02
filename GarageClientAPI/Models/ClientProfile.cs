using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class ClientProfile
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int CountryId { get; set; }

    public string PhoneExt { get; set; } = null!;

    public int PhoneNumber { get; set; }

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public int UserId { get; set; }

    public bool IsPremium { get; set; }

    public virtual ICollection<ClientNotification> ClientNotifications { get; set; } = new List<ClientNotification>();

    public virtual ICollection<ClientPaymentMethod> ClientPaymentMethods { get; set; } = new List<ClientPaymentMethod>();

    public virtual ICollection<ClientPremiumRegistration> ClientPremiumRegistrations { get; set; } = new List<ClientPremiumRegistration>();

    public virtual ICollection<ClientReminder> ClientReminders { get; set; } = new List<ClientReminder>();

    public virtual Country Country { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
