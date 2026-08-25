using FTPUtility.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtility
{
    public class FtpClient
    {
        private readonly FtpConfig _config;

        public FtpClient(FtpConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Host))
                throw new ArgumentException("FTP Host is required.");

            if (string.IsNullOrWhiteSpace(config.Username))
                throw new ArgumentException("FTP Username is required.");

            _config = config;
        }

        private FtpResult HandleWebException(WebException ex)
        {
            string errorCode = ex.Status.ToString();
            string errorMessage = ex.Message;

            try
            {
                if (ex.Response is FtpWebResponse ftpResponse)
                {
                    errorCode = ((int)ftpResponse.StatusCode).ToString();

                    errorMessage =
                        "FTP Status Code: " + (int)ftpResponse.StatusCode +
                        Environment.NewLine +
                        "FTP Status: " + ftpResponse.StatusCode +
                        Environment.NewLine +
                        "Server Response: " + ftpResponse.StatusDescription +
                        Environment.NewLine +
                        "WebException Status: " + ex.Status +
                        Environment.NewLine +
                        "Exception: " + ex.Message;

                    ftpResponse.Close();
                }
                else
                {
                    errorMessage =
                        "WebException Status: " + ex.Status +
                        Environment.NewLine +
                        "Exception: " + ex.Message;

                    if (ex.InnerException != null)
                    {
                        errorMessage +=
                            Environment.NewLine +
                            "Inner Exception: " +
                            ex.InnerException.Message;
                    }
                }
            }
            catch
            {
                errorMessage = ex.ToString();
            }

            return FtpResult.Fail(errorMessage, errorCode);
        }

        private FtpWebRequest CreateRequest(
    string remotePath,
    string method)
        {
            string url = BuildUrl(remotePath);

            if (_config.UseSsl &&
                _config.ValidateAnyCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }

            FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(url);

            request.Method = method;

            request.Credentials = new NetworkCredential(
                _config.Username,
                _config.Password
            );

            request.UsePassive = _config.UsePassive;
            request.EnableSsl = _config.UseSsl;

            request.UseBinary = true;

            request.KeepAlive = false;

            request.Timeout = 60000;
            request.ReadWriteTimeout = 60000;

            return request;
        }

        private string BuildUrl(string remotePath)
        {
            string host = _config.Host.TrimEnd('/');

            if (!host.StartsWith("ftp://",
                StringComparison.OrdinalIgnoreCase))
            {
                host = "ftp://" + host;
            }

            remotePath = (remotePath ?? "")
                .TrimStart('/');

            Uri hostUri = new Uri(host);

            string authority =
                hostUri.Host + ":" + _config.Port;

            return "ftp://" +
                   authority +
                   "/" +
                   remotePath;
        }

        public FtpResult UploadFile(
    string localFilePath,
    string remoteFilePath)
        {
            try
            {
                if (!File.Exists(localFilePath))
                {
                    return FtpResult.Fail(
                        "Local file does not exist: " + localFilePath,
                        "LOCAL_FILE_NOT_FOUND"
                    );
                }

                FileInfo fileInfo = new FileInfo(localFilePath);

                FtpWebRequest request = CreateRequest(
                    remoteFilePath,
                    WebRequestMethods.Ftp.UploadFile
                );

                request.ContentLength = fileInfo.Length;

                using (FileStream fileStream = new FileStream(
                    localFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                using (Stream ftpStream = request.GetRequestStream())
                {
                    byte[] buffer = new byte[81920];

                    int bytesRead;

                    while ((bytesRead = fileStream.Read(
                        buffer,
                        0,
                        buffer.Length)) > 0)
                    {
                        ftpStream.Write(
                            buffer,
                            0,
                            bytesRead
                        );
                    }
                }

                using (FtpWebResponse response =
                    (FtpWebResponse)request.GetResponse())
                {
                    return FtpResult.Ok(
                        "File uploaded successfully." +
                        Environment.NewLine +
                        "FTP Status: " +
                        response.StatusCode +
                        Environment.NewLine +
                        "Server Response: " +
                        response.StatusDescription
                    );
                }
            }
            catch (WebException ex)
            {
                return HandleWebException(ex);
            }
            catch (Exception ex)
            {
                return FtpResult.Fail(
                    ex.ToString(),
                    "GENERAL_ERROR"
                );
            }
        }

        public FtpResult DeleteFile(string remoteFilePath)
        {
            try
            {
                FtpWebRequest request = CreateRequest(
                    remoteFilePath,
                    WebRequestMethods.Ftp.DeleteFile
                );

                using (FtpWebResponse response =
                    (FtpWebResponse)request.GetResponse())
                {
                    return FtpResult.Ok(
                        "File deleted successfully. " +
                        response.StatusDescription
                    );
                }
            }
            catch (WebException ex)
            {
                return HandleWebException(ex);
            }
            catch (Exception ex)
            {
                return FtpResult.Fail(
                    ex.Message,
                    "GENERAL_ERROR"
                );
            }
        }

        public FtpResult DownloadFile(string remoteFilePath, string localFilePath)
        {
            try
            {
                FtpWebRequest request = CreateRequest(
                    remoteFilePath,
                    WebRequestMethods.Ftp.DownloadFile
                );

                using (FtpWebResponse response =
                    (FtpWebResponse)request.GetResponse())
                using (Stream responseStream =
                    response.GetResponseStream())
                using (FileStream fileStream =
                    new FileStream(
                        localFilePath,
                        FileMode.Create,
                        FileAccess.Write))
                {
                    responseStream.CopyTo(fileStream);
                }

                return FtpResult.Ok(
                    "File downloaded successfully."
                );
            }
            catch (WebException ex)
            {
                return HandleWebException(ex);
            }
            catch (Exception ex)
            {
                return FtpResult.Fail(
                    ex.Message,
                    "GENERAL_ERROR"
                );
            }
        }

        public FtpResult TestConnection()
        {
            try
            {
                FtpWebRequest request = CreateRequest(
                    "",
                    WebRequestMethods.Ftp.ListDirectory
                );

                using (FtpWebResponse response =
                    (FtpWebResponse)request.GetResponse())
                {
                    return FtpResult.Ok(
                        "FTP connection successful."
                    );
                }
            }
            catch (WebException ex)
            {
                return HandleWebException(ex);
            }
            catch (Exception ex)
            {
                return FtpResult.Fail(
                    ex.Message,
                    "GENERAL_ERROR"
                );
            }
        }

        public FtpResult ListFiles(string remoteDirectory)
        {
            try
            {
                FtpWebRequest request = CreateRequest(
                    remoteDirectory,
                    WebRequestMethods.Ftp.ListDirectory
                );

                List<string> files = new List<string>();

                using (FtpWebResponse response =
                    (FtpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        files.Add(line);
                    }
                }

                return new FtpResult
                {
                    Success = true,
                    Message = "FTP files retrieved successfully.",
                    Data = files
                };
            }
            catch (WebException ex)
            {
                return HandleWebException(ex);
            }
            catch (Exception ex)
            {
                return FtpResult.Fail(
                    ex.Message,
                    "GENERAL_ERROR"
                );
            }
        }
    }
}
