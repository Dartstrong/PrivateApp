using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrivateApp.Resources.Entities
{
    public class AuthorizationData
    {
        public string LoginStr { get; set; }
        public string PasswordStr { get; set; }
        public string? EmailStr { get; set; }
        public string DeviceIdStr { get; set; }
    }
}
