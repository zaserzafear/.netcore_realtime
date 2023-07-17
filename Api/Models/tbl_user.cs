using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

[Index("username", Name = "username_UNIQUE", IsUnique = true)]
[MySqlCharSet("utf8mb3")]
[MySqlCollation("utf8mb3_general_ci")]
public partial class tbl_user
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [StringLength(45)]
    public string username { get; set; } = null!;

    [StringLength(45)]
    public string password { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime created_timestamp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? updated_timestamp { get; set; }
}
