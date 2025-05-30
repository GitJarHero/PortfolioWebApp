using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities
{
    [Table("appuser")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("username")]
        public string UserName { get; set; }
        
        [Required]
        [MaxLength(255)]
        [Column("password")]
        public string Password { get; set; }
        
        // EF Core sollte Junction Tabelle autom. verwalten
        public List<UserRole> Roles { get; set; } = new();
        
        [Required]
        [MaxLength(7)] // e.g. #FF223E
        [Column("profile_color")]
        public string ProfileColor { get; set; }

        [Required]
        [Column("stateid")]
        public State State { get; set; }
        
        [Column("lastonline",TypeName = "timestamp without time zone")]
        public DateTime? LastOnline { get; set; }
        
        [Required]
        [Column("created",TypeName = "timestamp without time zone")]
        public DateTime Created { get; set; } = DateTime.Now;
        
    }
}