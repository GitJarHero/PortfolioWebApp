using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities;

[Table("directmessage")]
public class DirectMessage : EntityBase
{

    [Required]
    [Column("content", TypeName = "text")]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [ForeignKey("from")]
    public User FromUser { get; set; }

    [Required]
    [ForeignKey("to")]
    public User ToUser { get; set; }
    
    [Required]
    [Column("created")]
    public DateTime Created { get; set; }
    
    [Column("delivered")]
    public DateTime? Delivered { get; set; }

    [Column("read")]
    public DateTime? Read { get; set; }
    
}
