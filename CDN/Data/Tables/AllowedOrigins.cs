using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CDN.Data.Tables
{
    public class AllowedOrigins
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Automatically generates Id
        public int Id { get; set; }

        [Required] // Ensures that Origin is not null
        public string Origin { get; set; }
    }
}
