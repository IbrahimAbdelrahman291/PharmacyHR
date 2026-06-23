
using SharedKernel.Common;
using System.ComponentModel.DataAnnotations;

namespace Anouncement.Domain.Entities
{
    public class Announcement : BaseEntity
    {
        public DateTime? AnncouncementDate { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
