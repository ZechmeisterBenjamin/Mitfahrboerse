using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

[PrimaryKey("OfferId", "ValidUntil")]
[Table("t_Offer")]
[Index("Price", Name = "IX_Offer_Price")]
[Index("ValidUntil", Name = "IX_Offer_ValidUntil")]
public partial class t_Offer
{
    [Key]
    public int OfferId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Title { get; set; } = null!;

    public short Price { get; set; }

    [Key]
    public DateOnly ValidUntil { get; set; }

    [ForeignKey("FK_OfferId, FK_ValidUntil")]
    [InverseProperty("t_Offers")]
    public virtual ICollection<t_Person> FK_People { get; set; } = new List<t_Person>();

    [InverseProperty("Person")]
    public virtual ICollection<t_PersonOffer> PersonOffers { get; set; } = new List<t_PersonOffer>();
}
