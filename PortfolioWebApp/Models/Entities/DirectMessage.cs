using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities;

[Table("directmessage")]
public class DirectMessage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("content", TypeName = "text")]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [Column("from")]
    public User FromUser { get; set; }

    [Required]
    [Column("to")]
    public User ToUser { get; set; }
    
    [Required]
    [Column("created")]
    public DateTime Created { get; set; }
    
    [Column("delivered")]
    public DateTime? Delivered { get; set; }

    [Column("read")]
    public DateTime? Read { get; set; }
    
}
