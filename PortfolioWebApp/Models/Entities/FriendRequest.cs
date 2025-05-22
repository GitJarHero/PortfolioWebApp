using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities;

[Table("friendrequest")]
public class FriendRequest {
    
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("from_user")]
    public User From { get; set; }
    
    [Required]
    [Column("to_user")]
    public User To { get; set; }

    [Required]
    [Column("created", TypeName = "timestamp without time zone")]
    public DateTime Created { get; set; }
    
}