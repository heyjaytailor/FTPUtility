using System;
using System.IO;
using System.Security.Authentication;
using System.Net.Security;

using FluentFTP;

// IMPORTANT:
// FluentFTP also contains FtpConfig and FtpResult,
// so we create aliases for OUR classes.
using MyFtpConfig = FTPUtility.Models.FtpConfig;
using MyFtpResult = FTPUtility.Models.FtpResult;
using FluentFTP.Exceptions;

namespace FTPUtility
{
    public class FTPManager
    {
        private readonly MyFtpConfig _config;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public FTPManager(MyFtpConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Host))
                throw new ArgumentException("FTP Host is required.");

            if (string.IsNullOrWhiteSpace(config.Username))
                throw new ArgumentException("FTP Username is required.");

            _config = config;
        }


        // ============================================================
        // CREATE FTP CLIENT
        // ============================================================

        private FluentFTP.FtpClient CreateClient()
        {
            FluentFTP.FtpClient client =
                new FluentFTP.FtpClient(
                    _config.Host,
                    _config.Username,
                    _config.Password,
                    _config.Port
                );


            // --------------------------------------------------------
            // SSL / FTPS
            // --------------------------------------------------------

            if (_config.UseSsl)
            {
                client.Config.EncryptionMode =
                    FtpEncryptionMode.Explicit;

                client.Config.SslProtocols =
                    SslProtocols.Tls12;
            }
            else
            {
                client.Config.EncryptionMode =
                    FtpEncryptionMode.None;
            }


            // --------------------------------------------------------
            // PASSIVE / ACTIVE
            // --------------------------------------------------------

            if (_config.UsePassive)
            {
                client.Config.DataConnectionType =
                    FtpDataConnectionType.PASV;
            }
            else
            {
                client.Config.DataConnectionType =
                    FtpDataConnectionType.PORT;
            }


            // --------------------------------------------------------
            // TIMEOUTS
            // --------------------------------------------------------

            client.Config.ConnectTimeout = 30000;

            client.Config.ReadTimeout = 30000;

            client.Config.DataConnectionConnectTimeout = 30000;

            client.Config.DataConnectionReadTimeout = 30000;


            // --------------------------------------------------------
            // CERTIFICATE VALIDATION
            // --------------------------------------------------------

            client.ValidateCertificate += (control, e) =>
            {
                if (_config.ValidateAnyCertificate)
                {
                    // Accept any certificate.
                    //
                    // WARNING:
                    // This disables certificate authenticity checking
                    // for this FTP client.
                    e.Accept = true;
                }
                else
                {
                    // Normal certificate validation
                    e.Accept =
                        e.PolicyErrors == SslPolicyErrors.None;
                }
            };


            return client;
        }


        // ============================================================
        // TEST FTP CONNECTION
        // ============================================================

        public MyFtpResult TestConnection()
        {
            try
            {
                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        return MyFtpResult.Ok(
                            "FTP connection successful."
                        );
                    }

                    return MyFtpResult.Fail(
                        "Unable to connect to FTP server.",
                        "CONNECTION_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // UPLOAD FILE
        // ============================================================

        public MyFtpResult UploadFile(
            string localFilePath,
            string remoteFilePath)
        {
            try
            {
                // ----------------------------------------------------
                // Validate local file
                // ----------------------------------------------------

                if (string.IsNullOrWhiteSpace(localFilePath))
                {
                    return MyFtpResult.Fail(
                        "Local file path is required.",
                        "LOCAL_PATH_REQUIRED"
                    );
                }

                if (!File.Exists(localFilePath))
                {
                    return MyFtpResult.Fail(
                        "Local file does not exist: " +
                        localFilePath,
                        "LOCAL_FILE_NOT_FOUND"
                    );
                }


                // ----------------------------------------------------
                // Validate remote path
                // ----------------------------------------------------

                if (string.IsNullOrWhiteSpace(remoteFilePath))
                {
                    return MyFtpResult.Fail(
                        "Remote FTP file path is required.",
                        "REMOTE_PATH_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    // ------------------------------------------------
                    // Connect
                    // ------------------------------------------------

                    client.Connect();


                    if (!client.IsConnected)
                    {
                        return MyFtpResult.Fail(
                            "Could not establish FTP connection.",
                            "CONNECTION_FAILED"
                        );
                    }


                    // ------------------------------------------------
                    // Upload
                    // ------------------------------------------------

                    FtpStatus status =
                        client.UploadFile(
                            localFilePath,
                            remoteFilePath,
                            FtpRemoteExists.Overwrite,
                            true
                        );


                    // ------------------------------------------------
                    // Check status
                    // ------------------------------------------------

                    if (status == FtpStatus.Success)
                    {
                        return MyFtpResult.Ok(
                            "File uploaded successfully."
                        );
                    }


                    if (status == FtpStatus.Skipped)
                    {
                        return MyFtpResult.Fail(
                            "FTP upload was skipped.",
                            "UPLOAD_SKIPPED"
                        );
                    }


                    return MyFtpResult.Fail(
                        "FTP upload failed. Status: " +
                        status,
                        "UPLOAD_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // DOWNLOAD FILE
        // ============================================================

        public MyFtpResult DownloadFile(
            string remoteFilePath,
            string localFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteFilePath))
                {
                    return MyFtpResult.Fail(
                        "Remote FTP file path is required.",
                        "REMOTE_PATH_REQUIRED"
                    );
                }


                if (string.IsNullOrWhiteSpace(localFilePath))
                {
                    return MyFtpResult.Fail(
                        "Local file path is required.",
                        "LOCAL_PATH_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();


                    if (!client.IsConnected)
                    {
                        return MyFtpResult.Fail(
                            "Could not establish FTP connection.",
                            "CONNECTION_FAILED"
                        );
                    }


                    FtpStatus status =
                        client.DownloadFile(
                            localFilePath,
                            remoteFilePath,
                            FtpLocalExists.Overwrite,
                            FtpVerify.None
                        );


                    if (status == FtpStatus.Success)
                    {
                        return MyFtpResult.Ok(
                            "File downloaded successfully."
                        );
                    }


                    return MyFtpResult.Fail(
                        "FTP download failed. Status: " +
                        status,
                        "DOWNLOAD_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // DELETE FILE
        // ============================================================

        public MyFtpResult DeleteFile(string remoteFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteFilePath))
                {
                    return MyFtpResult.Fail(
                        "Remote FTP file path is required.",
                        "REMOTE_PATH_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();


                    if (!client.IsConnected)
                    {
                        return MyFtpResult.Fail(
                            "Could not establish FTP connection.",
                            "CONNECTION_FAILED"
                        );
                    }


                    // Check first
                    if (!client.FileExists(remoteFilePath))
                    {
                        return MyFtpResult.Fail(
                            "FTP file does not exist: " +
                            remoteFilePath,
                            "REMOTE_FILE_NOT_FOUND"
                        );
                    }


                    client.DeleteFile(remoteFilePath);


                    // Verify deletion
                    if (!client.FileExists(remoteFilePath))
                    {
                        return MyFtpResult.Ok(
                            "File deleted successfully."
                        );
                    }


                    return MyFtpResult.Fail(
                        "FTP server did not delete the file.",
                        "DELETE_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // CHECK FILE EXISTS
        // ============================================================

        public MyFtpResult FileExists(string remoteFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteFilePath))
                {
                    return MyFtpResult.Fail(
                        "Remote FTP file path is required.",
                        "REMOTE_PATH_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();


                    bool exists =
                        client.FileExists(remoteFilePath);


                    if (exists)
                    {
                        return MyFtpResult.Ok(
                            "FTP file exists."
                        );
                    }


                    return MyFtpResult.Fail(
                        "FTP file does not exist.",
                        "REMOTE_FILE_NOT_FOUND"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // CREATE DIRECTORY
        // ============================================================

        public MyFtpResult CreateDirectory(
            string remoteDirectory)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteDirectory))
                {
                    return MyFtpResult.Fail(
                        "Remote directory is required.",
                        "REMOTE_DIRECTORY_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();


                    if (client.DirectoryExists(remoteDirectory))
                    {
                        return MyFtpResult.Ok(
                            "FTP directory already exists."
                        );
                    }


                    bool created =
                        client.CreateDirectory(remoteDirectory);


                    if (created)
                    {
                        return MyFtpResult.Ok(
                            "FTP directory created successfully."
                        );
                    }


                    return MyFtpResult.Fail(
                        "Unable to create FTP directory.",
                        "CREATE_DIRECTORY_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // DELETE DIRECTORY
        // ============================================================

        public MyFtpResult DeleteDirectory(
            string remoteDirectory)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteDirectory))
                {
                    return MyFtpResult.Fail(
                        "Remote directory is required.",
                        "REMOTE_DIRECTORY_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();


                    if (!client.DirectoryExists(remoteDirectory))
                    {
                        return MyFtpResult.Fail(
                            "FTP directory does not exist.",
                            "REMOTE_DIRECTORY_NOT_FOUND"
                        );
                    }


                    client.DeleteDirectory(remoteDirectory);


                    if (!client.DirectoryExists(remoteDirectory))
                    {
                        return MyFtpResult.Ok(
                            "FTP directory deleted successfully."
                        );
                    }


                    return MyFtpResult.Fail(
                        "FTP directory could not be deleted.",
                        "DELETE_DIRECTORY_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // RENAME FILE
        // ============================================================

        public MyFtpResult RenameFile(
            string oldRemotePath,
            string newRemotePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldRemotePath) ||
                    string.IsNullOrWhiteSpace(newRemotePath))
                {
                    return MyFtpResult.Fail(
                        "Old and new FTP paths are required.",
                        "REMOTE_PATH_REQUIRED"
                    );
                }


                using (FluentFTP.FtpClient client = CreateClient())
                {
                    client.Connect();


                    if (!client.FileExists(oldRemotePath))
                    {
                        return MyFtpResult.Fail(
                            "Source FTP file does not exist.",
                            "REMOTE_FILE_NOT_FOUND"
                        );
                    }


                    client.MoveFile(
                        oldRemotePath,
                        newRemotePath,
                        FtpRemoteExists.Overwrite
                    );


                    if (client.FileExists(newRemotePath))
                    {
                        return MyFtpResult.Ok(
                            "FTP file renamed successfully."
                        );
                    }


                    return MyFtpResult.Fail(
                        "Unable to rename FTP file.",
                        "RENAME_FAILED"
                    );
                }
            }
            catch (FtpException ex)
            {
                return CreateFtpError(ex);
            }
            catch (Exception ex)
            {
                return CreateGeneralError(ex);
            }
        }


        // ============================================================
        // FTP ERROR HANDLING
        // ============================================================

        private MyFtpResult CreateFtpError(
            FtpException ex)
        {
            string message =
                "FTP Error: " +
                ex.Message;


            if (ex.InnerException != null)
            {
                message +=
                    Environment.NewLine +
                    "Inner Error: " +
                    ex.InnerException.Message;
            }


            return MyFtpResult.Fail(
                message,
                "FTP_ERROR"
            );
        }


        // ============================================================
        // GENERAL ERROR HANDLING
        // ============================================================

        private MyFtpResult CreateGeneralError(
            Exception ex)
        {
            string message =
                "Exception Type: " +
                ex.GetType().FullName +
                Environment.NewLine +
                "Message: " +
                ex.Message;


            if (ex.InnerException != null)
            {
                message +=
                    Environment.NewLine +
                    "Inner Exception: " +
                    ex.InnerException.Message;
            }


            return MyFtpResult.Fail(
                message,
                ex.GetType().Name
            );
        }
    }
}