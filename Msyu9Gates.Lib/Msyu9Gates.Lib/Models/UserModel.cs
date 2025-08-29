using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public class UserModel
{
    [Key] public int Id { get; set; }
    [Required] public string? DiscordId { get; set; }
    [Required] public string? Username { get; set; }
    [Required] public string? Avatar { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; }
    public DateTimeOffset? LastLoginUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
