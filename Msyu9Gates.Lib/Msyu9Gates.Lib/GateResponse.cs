using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msyu9Gates.Lib
{
    public class GateResponse
    {
        public string? Key { get; set; } = null;
        public string? KeyID { get; set; } = String.Empty;
        public string? Message { get; set; } = String.Empty;
        public bool Success { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();

        public GateResponse(string? key, string? keyID, bool success, string? message = null)
        {
            Key = key;
            KeyID = keyID;
            Message = message;
            Success = success;
        }
    }
}
