using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrivateApp.Resources.Entities
{
    public class NewMessage
    {
        public string LoginStr { get; set; }
        public string PasswordStr { get; set; }
        public string DeviceIdStr { get; set; }
        public string SenderData { get; set; }
        public string ReceiverData { get; set; }
    }
}
