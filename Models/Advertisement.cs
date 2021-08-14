using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab4.Models
{
    public class Advertisement
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string CommunityId { get; set; }

        [Required]
        [StringLength(50)]
        [MinLength(3)]
        public string FileName { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        public Community Community { get; set; }
    }
}
