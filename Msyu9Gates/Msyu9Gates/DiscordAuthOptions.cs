using Microsoft.AspNetCore.SignalR.Protocol;
using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates
{
    public sealed class DiscordAuthOptions
    {
        public const string ConfigSection = "Authentication:Discord";
        [Required] public string? ClientId { get; set; } = string.Empty;
        [Required] public string? ClientSecret { get; set; } = string.Empty;
        public string? CallbackPath { get; set; } = "/signin-discord";
    }
}
