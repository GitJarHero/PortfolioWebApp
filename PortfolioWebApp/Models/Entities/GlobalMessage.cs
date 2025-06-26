using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities;

[Table("globalmessage")]
public class GlobalMessage {
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("created",TypeName = "timestamp without time zone")]
    public DateTime Created { get; set; } = DateTime.Now;
    
    [Required]
    [MaxLength(255)]
    [Column("content")]
    public string Content { get; set; }
    
    [Required]
    [Column("userid")]
    public User User { get; set; }
    
    [Required]
    [Column("stateid")]
    public State State { get; set; } // could be deleted by admin (feature idea)
    
}