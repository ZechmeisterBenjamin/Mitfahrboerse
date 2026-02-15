using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

[Table("t_PersonRide")]
public partial class t_PersonRide
{
    [Key]
    public int FK_RideId { get; set; }

    [Key]
    [StringLength(50)]
    [Unicode(false)]
    public string FK_PersonId { get; set; } = null!;

    // This is your new property!
    public short Status { get; set; }
    public string? EventId { get; set; }

    [ForeignKey("FK_PersonId")]
    [InverseProperty("PersonRides")]
    public virtual t_Person Person { get; set; } = null!;

    [ForeignKey("FK_RideId")]
    [InverseProperty("PersonRides")]
    public virtual t_Ride Ride { get; set; } = null!;
    public t_PersonRide()
    {
    }
    public t_PersonRide(string personId, int rideId, short status)
    {
        FK_PersonId = personId;
        FK_RideId = rideId;
        Status = status;
    }
}