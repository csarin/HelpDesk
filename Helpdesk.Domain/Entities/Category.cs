using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        private Category()
        {}

        public Category(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Your name is required.", nameof(name));

            Name = name.Trim();
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Your name is required.", nameof(name));

            Name = name.Trim();
        }
    }
}
