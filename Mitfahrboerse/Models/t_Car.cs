using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Models;

[Table("t_Car")]
[Index("LicensePlate", Name = "IX_Car_LicensePlate")]
[Index("FK_Owner_PersonId", Name = "IX_Car_Owner")]
public partial class t_Car
{
    public t_Car() { }

    public t_Car(string licensePlate, short seats, string brand, string model, string colour, string personId)
    {
        LicensePlate = licensePlate;
        Seats = seats;
        Brand = brand;
        Model = model;
        Colour = colour;
        FK_Owner_PersonId = personId;
    }

    [Key]
    public int CarId { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string LicensePlate { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Brand { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Model { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Colour { get; set; } = null!;

    public short Seats { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string FK_Owner_PersonId { get; set; } = null!;

    [ForeignKey("FK_Owner_PersonId")]
    [InverseProperty("t_Cars")]
    public virtual t_Person FK_Owner_Person { get; set; } = null!;
    
    [InverseProperty("FK_Car")]
    public virtual ICollection<t_Ride> t_Rides { get; set; } = new List<t_Ride>();
}
