using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities;

[Table("directmessage")]
public class DirectMessage
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

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
    [Column("created", TypeName = "timestamp without time zone")]
    public DateTime Created { get; set; }
    
    [Column("delivered", TypeName = "timestamp without time zone")]
    public DateTime? Delivered { get; set; }

    [Column("read", TypeName = "timestamp without time zone")]
    public DateTime? Read { get; set; }
    
}
