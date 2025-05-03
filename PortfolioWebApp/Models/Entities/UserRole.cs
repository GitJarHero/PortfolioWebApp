using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities;

[Table("userrole")]
public class UserRole
{
    [Key]
    [Column("id")]
    public short Id { get; set; }
    
    [Required]
    [Column("name")]
    [MaxLength(20)]
    public string Name { get; set; }
}