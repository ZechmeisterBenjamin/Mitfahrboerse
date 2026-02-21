using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

[Table("t_Person")]
[Index("Email", Name = "IX_Person_Email")]
[Index("LastName", Name = "IX_Person_LastName")]
[Index("Points", Name = "IX_Person_Points")]
public partial class t_Person
{
    [Key]
    [StringLength(50)]
    [Unicode(false)]
    public string PersonId { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string LastName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string? Class { get; set; }

    public int Points { get; set; }

    public byte Design { get; set; }

    public byte Startpage { get; set; }

    public bool IsAdmin { get; set; } = false;

    [InverseProperty("FK_Owner_Person")]
    public virtual ICollection<t_Car> t_Cars { get; set; } = new List<t_Car>();

    [InverseProperty("FK_Driver_Person")]
    public virtual ICollection<t_Ride> t_Rides { get; set; } = new List<t_Ride>();

    [InverseProperty("Person")]
    public virtual ICollection<t_PersonRide> PersonRides { get; set; } = new List<t_PersonRide>();

    [InverseProperty("FK_People")]
    public virtual ICollection<t_Offer> t_Offers { get; set; } = new List<t_Offer>();
    
    [InverseProperty("FK_Person")]
    public virtual ICollection<t_PersonOffer> PersonOffers { get; set; } = new List<t_PersonOffer>();
}
