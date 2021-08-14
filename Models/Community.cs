using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab4.Models
{
    public class Community : IEquatable<Community>
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Registration Number")]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        [MinLength(3)]
        public string Title { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Budget { get; set; }

        public bool Equals(Community other)
        {
            if(other is null)
            {
                return false;
            }
            if (this.Id == other.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public ICollection<CommunityMembership> Membership { get; set; }
    }
}
