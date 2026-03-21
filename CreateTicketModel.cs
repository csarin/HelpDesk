using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Web.Models
{
    public class CreateTicketModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
    }
}
