using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrivateApp
{
    public class Session
    {
        public int Id { get; set; }
        public string SymmetricKey { get; set; }
        public string InitVector { get; set; }
        public DateTime LastUsed { get; set; }
    }
}
