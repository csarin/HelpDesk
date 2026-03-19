using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;    
    }
}
