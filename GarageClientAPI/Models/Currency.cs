using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class Currency
{
    public int Id { get; set; }

    public string? CurrDesc { get; set; }

    public virtual ICollection<ClientPaymentOrder> ClientPaymentOrders { get; set; } = new List<ClientPaymentOrder>();

    public virtual ICollection<GaragePaymentOrder> GaragePaymentOrders { get; set; } = new List<GaragePaymentOrder>();

    public virtual ICollection<PremiumOffer> PremiumOffers { get; set; } = new List<PremiumOffer>();

    public virtual ICollection<VehiclesServiceType> VehiclesServiceTypes { get; set; } = new List<VehiclesServiceType>();
}
