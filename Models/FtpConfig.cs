using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtility.Models
{
    public class FtpConfig
    {
        public string Host { get; set; }

        public int Port { get; set; } = 21;

        public string Username { get; set; }

        public string Password { get; set; }

        public bool UseSsl { get; set; } = true;

        public bool UsePassive { get; set; } = true;

        public bool ValidateAnyCertificate { get; set; } = false;
    }
}
