using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class GaragePaymentOrder
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int GarageId { get; set; }

    public decimal Amount { get; set; }

    public int Currid { get; set; }

    public int PaymentMethodId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public int PremiumOfferid { get; set; }

    public virtual Currency? Curr { get; set; } = null!;

    public virtual GarageProfile? Garage { get; set; } = null!;

    public virtual GaragePaymentMethod? PaymentMethod { get; set; } = null!;

    public virtual PremiumOffer? PremiumOffer { get; set; } = null!;
}
