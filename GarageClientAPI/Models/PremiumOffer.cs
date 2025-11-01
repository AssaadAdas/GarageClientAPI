using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class PremiumOffer
{
    public int Id { get; set; }

    public int UserTypeid { get; set; }

    public string PremiumDesc { get; set; } = null!;

    public decimal PremiumCost { get; set; }

    public int CurrId { get; set; }

    public int PremiumRange { get; set; }

    public virtual ICollection<ClientPaymentOrder> ClientPaymentOrders { get; set; } = new List<ClientPaymentOrder>();

    public virtual Currency Curr { get; set; } = null!;

    public virtual ICollection<GaragePaymentOrder> GaragePaymentOrders { get; set; } = new List<GaragePaymentOrder>();

    public virtual UserType UserType { get; set; } = null!;
}
