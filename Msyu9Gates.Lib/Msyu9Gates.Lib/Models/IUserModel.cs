namespace Msyu9Gates.Lib.Models;

public interface IUserModel
{
    public int Id { get; set; }
    public string DiscordId { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
}
