using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

[Table("t_Ride")]
[Index("FK_Driver_PersonId", Name = "IX_Ride_Driver")]
[Index("FK_EndsAt_PositionId", Name = "IX_Ride_EndsAt")]
[Index("RideDateTime", Name = "IX_Ride_RideDateTime")]
[Index("FK_StartsAt_PositionId", Name = "IX_Ride_StartsAt")]
[Index("Status", Name = "IX_Ride_Status")]
public partial class t_Ride
{
    [Key]
    public int RideId { get; set; }

    public int Distance { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RideDateTime { get; set; }

    public short Status { get; set; }

    public int FK_StartsAt_PositionId { get; set; }

    public int FK_EndsAt_PositionId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string FK_Driver_PersonId { get; set; } = null!;

    [ForeignKey("FK_Driver_PersonId")]
    [InverseProperty("t_Rides")]
    public virtual t_Person FK_Driver_Person { get; set; } = null!;

    [ForeignKey("FK_EndsAt_PositionId")]
    [InverseProperty("t_RideFK_EndsAt_Positions")]
    public virtual t_Position FK_EndsAt_Position { get; set; } = null!;

    [ForeignKey("FK_StartsAt_PositionId")]
    [InverseProperty("t_RideFK_StartsAt_Positions")]
    public virtual t_Position FK_StartsAt_Position { get; set; } = null!;
    
    public int FK_CarId { get; set; }

    [InverseProperty("Ride")]
    public virtual ICollection<t_PersonRide> PersonRides { get; set; } = new List<t_PersonRide>();
    
    [ForeignKey("FK_CarId")]
    [InverseProperty("t_Rides")]
    public virtual t_Car FK_Car { get; set; } = null!;
}
