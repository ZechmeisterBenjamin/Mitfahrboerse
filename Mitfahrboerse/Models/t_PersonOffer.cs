using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mitfahrboerse.Models
{
    [Table("t_PersonOffer")]
    public partial class t_PersonOffer
    {
        [Key]
        public int FK_OfferId { get; set; }

        [Key]
        [StringLength(50)]
        [Unicode(false)]
        public string FK_PersonId { get; set; } = null!;

        [Key]
        public DateOnly FK_ValidUntil { get; set; }

        [ForeignKey("FK_PersonId")]
        [InverseProperty("PersonOffers")]
        public virtual t_Person Person { get; set; } = null!;

        [ForeignKey("FK_OfferId, FK_ValidUntil")]
        [InverseProperty("PersonOffers")]
        public virtual t_Offer Offer { get; set; } = null!;

        public t_PersonOffer() { }

        public t_PersonOffer(int fk_offerId, string fk_personId, DateOnly validuntil)
        {
            FK_OfferId = fk_offerId;
            FK_PersonId = fk_personId;
            FK_ValidUntil = validuntil;
        }
    }
}
