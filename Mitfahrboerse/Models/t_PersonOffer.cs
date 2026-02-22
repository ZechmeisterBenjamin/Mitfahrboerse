using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mitfahrboerse.Models
{
    [Table("t_PersonOffer")]
    [Index("FK_PersonId", Name = "IX_PersonOffer_PersonId")]
    [Index("FK_ValidUntil", Name = "IX_PersonOffer_ValidUntil")]
    public partial class t_PersonOffer
    {
        [Key]
        public int FK_OfferId { get; set; }

        [Key]
        [StringLength(50)]
        [Unicode(false)]   
        public string FK_PersonId { get; set; } = null!;

        public DateOnly FK_ValidUntil { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string Code { get; set; } = null!;

        public bool IsUsed { get; set; } = false;

        [ForeignKey("FK_OfferId")]
        [InverseProperty("PersonOffers")]
        public virtual t_Offer FK_Offer { get; set; } = null!;

        [ForeignKey("FK_PersonId")]
        [InverseProperty("PersonOffers")]
        public virtual t_Person FK_Person { get; set; } = null!;

        public t_PersonOffer() { }

        public t_PersonOffer(int fk_offerId, string fk_personId, DateOnly validuntil, string code)
        {
            FK_OfferId = fk_offerId;
            FK_PersonId = fk_personId;
            FK_ValidUntil = validuntil;
            Code = code;
        }
    }
}
