using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtility.Models
{
    public class FtpResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorCode { get; set; }

        public List<string> Data { get; set; }

        public static FtpResult Ok(string message)
        {
            return new FtpResult
            {
                Success = true,
                Message = message
            };
        }

        public static FtpResult Fail(
            string errorMessage,
            string errorCode = null)
        {
            return new FtpResult
            {
                Success = false,
                Message = "FTP operation failed.",
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }
}
