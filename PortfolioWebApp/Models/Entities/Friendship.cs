using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioWebApp.Models.Entities
{
    [Table("friendship")]
    public class Friendship {
        
        public User User1 { get; set; }
        public User User2 { get; set; }

        [Column("created", TypeName = "timestamp without time zone")]
        public DateTime Created { get; set; }
    }
}