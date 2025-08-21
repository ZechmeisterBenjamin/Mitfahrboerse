using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

[Table("t_Position")]
[Index("Longitude", "Latitude", Name = "IX_Position_Longitude_Latitude")]
public partial class t_Position
{
    [Key]
    public int PositionId { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal Longitude { get; set; }

    [Column(TypeName = "decimal(8, 6)")]
    public decimal Latitude { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Description { get; set; }

    [InverseProperty("FK_EndsAt_Position")]
    public virtual ICollection<t_Ride> t_RideFK_EndsAt_Positions { get; set; } = new List<t_Ride>();

    [InverseProperty("FK_StartsAt_Position")]
    public virtual ICollection<t_Ride> t_RideFK_StartsAt_Positions { get; set; } = new List<t_Ride>();
}
