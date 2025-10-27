using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class PaymentType
{
    public int Id { get; set; }

    public string? PaymentTypeDesc { get; set; }

    public virtual ICollection<GaragePaymentMethod>? GaragePaymentMethods { get; set; } = new List<GaragePaymentMethod>();
    public virtual ICollection<ClientPaymentMethod>? ClientPaymentMethods { get; set; } = new List<ClientPaymentMethod>();
}
