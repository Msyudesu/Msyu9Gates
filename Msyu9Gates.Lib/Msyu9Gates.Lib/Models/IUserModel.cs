using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msyu9Gates.Lib.Models;

public interface IUserModel
{
    public int Id { get; set; }
    public string DiscordId { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
}
