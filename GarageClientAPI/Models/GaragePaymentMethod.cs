using System;
using System.Collections.Generic;

namespace GarageClientAPI.Models;

public partial class GaragePaymentMethod
{
    public int Id { get; set; }

    public int Garageid { get; set; }

    public int PaymentTypeId { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastModified { get; set; }

    public bool IsActive { get; set; }

    public string CardNumber { get; set; } = null!;

    public string CardHolderName { get; set; } = null!;

    public int ExpiryMonth { get; set; }

    public int ExpiryYear { get; set; }

    public string Cvv { get; set; } = null!;

    public virtual PaymentType? PaymentType { get; set; } = null!;
    public virtual GarageProfile? Garage { get; set; } = null!;
    public virtual ICollection<GaragePaymentOrder>? GaragePaymentOrders { get; set; } = new List<GaragePaymentOrder>();
}
