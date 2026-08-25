# FTPUtility

A reusable **C# FTP/FTPS utility library** for Windows Desktop applications, Web Applications, Web APIs, Windows Services, and other .NET Framework applications.

`FTPUtility` provides a simple application-level API for common FTP operations while hiding the underlying FTP/FTPS implementation.

The goal is simple:

> Configure the FTP server once, call an operation, and receive a structured success/error result.

---

## Why FTPUtility?

Implementing FTP directly inside every application creates duplicated code, inconsistent error handling, certificate problems, connection configuration, and difficult maintenance.

Without a reusable FTP library, an application may need to implement:

```text
Application
    │
    ├── FTP connection
    ├── Authentication
    ├── SSL / FTPS
    ├── Certificate validation
    ├── Passive / Active mode
    ├── Upload
    ├── Download
    ├── Delete
    ├── Rename
    ├── Directory handling
    ├── File existence checks
    ├── Exception handling
    └── FTP response handling
```

This code then gets duplicated across multiple applications.

With FTPUtility:

```text
                    FTPUtility
                        │
        ┌───────────────┼────────────────┐
        │               │                │
     Desktop          Web API         Web App
        │               │                │
        └───────────────┼────────────────┘
                        │
                  FTP / FTPS Server
```

Each application only needs to configure the server and call the required method.

---

# Features

## Connection

* FTP connection
* Explicit FTPS
* Username/password authentication
* Configurable port
* Configurable connection timeout
* Configurable read timeout
* Passive mode
* Active mode
* Certificate validation configuration

## File Operations

* Upload file
* Download file
* Delete file
* Check whether a file exists
* Rename/move file

## Directory Operations

* Create directory
* Delete directory
* Check directory existence through the underlying FTP client

## Error Handling

Every operation returns a structured `FtpResult`.

Instead of forcing the application to catch FTP-specific exceptions everywhere:

```csharp
try
{
    // FTP code
}
catch
{
    // Handle FTP error
}
```

the application can use:

```csharp
FtpResult result = ftp.UploadFile(...);

if (result.Success)
{
    // Success
}
else
{
    // Error
}
```

---

# Supported Application Types

The DLL can be referenced by:

* Windows Forms
* WPF
* ASP.NET applications
* ASP.NET Web API
* Windows Services
* Console applications
* C# class libraries
* Background services
* Other compatible .NET Framework applications

The FTP implementation remains inside the DLL.

---

# Architecture

The library follows a simple separation of responsibilities.

```text
Application
    │
    │ FtpConfig
    ▼
FTPManager
    │
    ├── Connection
    ├── FTPS configuration
    ├── Certificate handling
    ├── Upload
    ├── Download
    ├── Delete
    ├── Rename
    └── Directory operations
    │
    ▼
FluentFTP
    │
    ▼
FTP / FTPS Server
```

The application does not need to directly work with `FtpWebRequest` or FluentFTP.

---

# Installation

## Option 1 — Add the project to your solution

Add the `FTPUtility` project to your Visual Studio solution.

Then:

```text
Application
    ↓
Add Reference
    ↓
Projects
    ↓
FTPUtility
```

---

## Option 2 — Reference the compiled DLL

Build the project:

```text
Build
→ Build Solution
```

Then reference:

```text
FTPUtility.dll
```

from the application's project.

Make sure the required FluentFTP dependencies are also available to the application.

---

# Basic Configuration

Create an `FtpConfig` object:

```csharp
using FTPUtility.Models;

FtpConfig config = new FtpConfig
{
    Host = "ftp.example.com",
    Port = 21,
    Username = "ftp-user",
    Password = "ftp-password",

    UseSsl = true,
    UsePassive = true,
    ValidateAnyCertificate = false
};
```

Create the FTP manager:

```csharp
using FTPUtility;

FTPManager ftp = new FTPManager(config);
```

The same `FTPManager` can then be used for the required FTP operations.

---

# Configuration Properties

| Property                 | Description                                 |
| ------------------------ | ------------------------------------------- |
| `Host`                   | FTP server hostname or IP address           |
| `Port`                   | FTP server port, normally `21`              |
| `Username`               | FTP username                                |
| `Password`               | FTP password                                |
| `UseSsl`                 | Enables FTPS                                |
| `UsePassive`             | Uses passive FTP data connections           |
| `ValidateAnyCertificate` | Accepts any server certificate when enabled |

Example:

```csharp
FtpConfig config = new FtpConfig
{
    Host = "192.168.1.100",
    Port = 21,
    Username = "myuser",
    Password = "mypassword",

    UseSsl = true,
    UsePassive = true,
    ValidateAnyCertificate = true
};
```

---

# Security Warning — ValidateAnyCertificate

This setting is intentionally available for FTP servers using:

* Self-signed certificates
* Internal certificates
* Certificates whose hostname does not match the server address
* Development/test FTP servers

Example:

```csharp
ValidateAnyCertificate = true
```

This means the client accepts the server certificate without normal certificate-chain validation.

### Recommended for production

Use:

```csharp
ValidateAnyCertificate = false
```

whenever the FTP server has a valid, trusted certificate.

### Development/testing

For an internal or development FTP server:

```csharp
ValidateAnyCertificate = true
```

may be necessary.

Do not enable this setting blindly on an Internet-facing production system.

---

# Test Connection

```csharp
FtpResult result = ftp.TestConnection();

if (result.Success)
{
    Console.WriteLine(result.Message);
}
else
{
    Console.WriteLine(result.ErrorCode);
    Console.WriteLine(result.ErrorMessage);
}
```

Example successful response:

```text
FTP connection successful.
```

---

# Upload File

```csharp
FtpResult result = ftp.UploadFile(
    @"C:\Files\invoice.pdf",
    "/documents/invoice.pdf"
);

if (result.Success)
{
    Console.WriteLine("Upload successful");
}
else
{
    Console.WriteLine("Upload failed");
    Console.WriteLine(result.ErrorMessage);
}
```

The remote path is an FTP path:

```text
/documents/invoice.pdf
```

not a Windows path:

```text
C:\documents\invoice.pdf
```

---

# Upload Example — Windows Forms

```csharp
private void btnUpload_Click(object sender, EventArgs e)
{
    FtpConfig config = new FtpConfig
    {
        Host = "ftp.example.com",
        Port = 21,
        Username = "ftp-user",
        Password = "ftp-password",

        UseSsl = true,
        UsePassive = true,
        ValidateAnyCertificate = true
    };

    FTPManager ftp = new FTPManager(config);

    FtpResult result = ftp.UploadFile(
        @"C:\Files\invoice.pdf",
        "/documents/invoice.pdf"
    );

    if (result.Success)
    {
        MessageBox.Show(
            result.Message,
            "FTP Success",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    else
    {
        MessageBox.Show(
            "Error Code: " + result.ErrorCode +
            Environment.NewLine +
            Environment.NewLine +
            result.ErrorMessage,
            "FTP Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
```

---

# Download File

```csharp
FtpResult result = ftp.DownloadFile(
    "/documents/invoice.pdf",
    @"C:\Downloads\invoice.pdf"
);

if (result.Success)
{
    Console.WriteLine("Download successful");
}
else
{
    Console.WriteLine(result.ErrorMessage);
}
```

---

# Delete File

```csharp
FtpResult result = ftp.DeleteFile(
    "/documents/invoice.pdf"
);

if (result.Success)
{
    Console.WriteLine("File deleted successfully.");
}
else
{
    Console.WriteLine(
        result.ErrorCode + ": " +
        result.ErrorMessage
    );
}
```

---

# Check File Exists

```csharp
FtpResult result = ftp.FileExists(
    "/documents/invoice.pdf"
);

if (result.Success)
{
    Console.WriteLine("File exists.");
}
else
{
    Console.WriteLine("File does not exist.");
}
```

---

# Create Directory

```csharp
FtpResult result = ftp.CreateDirectory(
    "/documents/2026"
);

if (result.Success)
{
    Console.WriteLine(result.Message);
}
```

---

# Delete Directory

```csharp
FtpResult result = ftp.DeleteDirectory(
    "/documents/2026"
);

if (result.Success)
{
    Console.WriteLine(result.Message);
}
```

---

# Rename / Move File

```csharp
FtpResult result = ftp.RenameFile(
    "/documents/old-name.pdf",
    "/documents/new-name.pdf"
);

if (result.Success)
{
    Console.WriteLine(
        "File renamed successfully."
    );
}
```

---

# FtpResult

All major operations return:

```csharp
FtpResult
```

The result contains:

```csharp
public bool Success { get; set; }

public string Message { get; set; }

public string ErrorMessage { get; set; }

public string ErrorCode { get; set; }

public List<string> Data { get; set; }
```

Example:

```csharp
FtpResult result = ftp.UploadFile(
    localFile,
    remoteFile
);

if (result.Success)
{
    // Operation succeeded
}
else
{
    // Operation failed
}
```

---

# Error Handling

Instead of depending on exceptions for normal FTP operation results:

```csharp
if (result.Success)
{
    // Success
}
else
{
    Console.WriteLine(
        result.ErrorCode
    );

    Console.WriteLine(
        result.ErrorMessage
    );
}
```

Example error:

```text
Error Code:
UPLOAD_FAILED

Error Message:
FTP upload failed: 425 Can't open data connection
```

This allows the calling application to decide what to do.

For example:

```csharp
switch (result.ErrorCode)
{
    case "CONNECTION_FAILED":
        // Retry connection
        break;

    case "UPLOAD_FAILED":
        // Log upload failure
        break;

    case "REMOTE_FILE_NOT_FOUND":
        // File doesn't exist
        break;

    default:
        // General handling
        break;
}
```

---

# FTP 425 and Upload Verification

Some FTP/FTPS servers can exhibit unusual behavior where the server receives the uploaded file but the client receives an error while completing the transfer.

For example:

```text
Client
  │
  │ STOR file.pdf
  ▼
FTP Server
  │
  │ receives file
  │
  │ file exists on server
  │
  ▼
Client receives 425
```

This can result in an apparent situation where:

```text
Upload = physically completed
FTP response = error
```

FTPUtility contains a verification fallback for certain upload exceptions.

The implementation checks whether the remote file exists and compares the remote size with the original local file size.

Conceptually:

```text
Local file
    │
    │ Upload
    ▼
FTP Server
    │
    │ Exception 425
    ▼
Verification
    │
    ├── Remote file does not exist
    │       ↓
    │     FAILURE
    │
    └── Remote file exists
            ↓
       Compare sizes
            │
       ┌────┴────┐
       │         │
      Same    Different
       │         │
    SUCCESS    FAILURE
```

This should not be interpreted as:

> "425 always means success."

A genuine failed or incomplete transfer must remain a failure.

---

# FTP vs FTPS

FTP:

```text
FTP
Port 21
No TLS encryption
```

FTPS:

```text
FTPS
Port 21
Explicit TLS
```

With this library:

```csharp
UseSsl = true
```

enables Explicit FTPS.

Example:

```csharp
FtpConfig config = new FtpConfig
{
    Host = "ftp.example.com",
    Port = 21,
    Username = "user",
    Password = "password",
    UseSsl = true,
    UsePassive = true,
    ValidateAnyCertificate = false
};
```

---

# Passive vs Active FTP

FTP uses two types of connections:

```text
Control connection
       +
Data connection
```

The control connection handles commands such as:

```text
USER
PASS
PWD
DELE
STOR
RETR
```

The data connection is used for operations such as:

```text
UPLOAD
DOWNLOAD
LIST
```

Passive mode is generally preferable when the client is behind:

```text
NAT
Firewall
Router
Corporate network
```

Enable it with:

```csharp
UsePassive = true;
```

---

# Why Use a DLL?

Without a shared FTP library:

```text
Application A
    └── FTP code

Application B
    └── FTP code

Application C
    └── FTP code

Application D
    └── FTP code
```

Every application has its own:

```text
Connection code
SSL code
Certificate code
Upload code
Delete code
Exception handling
Logging
```

With FTPUtility:

```text
                 FTPUtility.dll
                       │
          ┌────────────┼────────────┐
          │            │            │
       Desktop       Web API      Service
          │            │            │
          └────────────┼────────────┘
                       │
                   FTP Server
```

This provides a single location for FTP-related behavior.

---

# Advantages

## 1. Reusable

Write FTP logic once and reuse it across multiple applications.

## 2. Centralized configuration

FTP settings are represented through:

```csharp
FtpConfig
```

## 3. Consistent error handling

Applications receive:

```csharp
FtpResult
```

instead of implementing their own result models.

## 4. FTPS support

The library supports explicit FTPS configuration.

## 5. Certificate handling

Development/internal FTP servers with unusual certificates can be supported through:

```csharp
ValidateAnyCertificate
```

## 6. Multiple FTP operations

The same manager provides:

```text
Upload
Download
Delete
Rename
File Exists
Create Directory
Delete Directory
Test Connection
```

## 7. Application independence

The calling application does not need to implement FluentFTP logic directly.

---

# What You Lose If You Don't Use a Shared FTP DLL

If each application implements FTP independently, you typically lose:

```text
Centralized FTP behavior
Consistent error handling
Reusable configuration
Centralized bug fixes
Consistent certificate handling
Consistent FTPS configuration
Reduced duplicated code
```

For example, if an FTP server changes its TLS requirements, you may need to modify:

```text
Desktop application
Web API
Web application
Windows service
Background worker
```

With a shared library:

```text
Update FTPUtility
       ↓
Rebuild / redeploy DLL
       ↓
Applications use updated implementation
```

---

# What You Gain by Using FTPUtility

```text
                 ┌─────────────────────┐
                 │     FTPUtility      │
                 └──────────┬──────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
      Upload              Download             Delete
        │                   │                   │
        └───────────────────┼───────────────────┘
                            │
                     Error Handling
                            │
                     FTP / FTPS Layer
```

Instead of repeatedly writing:

```csharp
FtpWebRequest request = ...
request.Credentials = ...
request.EnableSsl = ...
request.UsePassive = ...
```

your application simply uses:

```csharp
FTPManager ftp = new FTPManager(config);

FtpResult result = ftp.UploadFile(
    localFile,
    remoteFile
);
```

---

# Recommended Production Usage

Do not hard-code credentials into source code.

Avoid:

```csharp
Password = "MyRealPassword"
```

Instead use:

```text
Environment Variables
Configuration Files
Azure Key Vault
AWS Secrets Manager
Windows Credential Manager
Other secure secret stores
```

For example:

```csharp
FtpConfig config = new FtpConfig
{
    Host = Environment.GetEnvironmentVariable("FTP_HOST"),
    Port = 21,
    Username = Environment.GetEnvironmentVariable("FTP_USERNAME"),
    Password = Environment.GetEnvironmentVariable("FTP_PASSWORD"),

    UseSsl = true,
    UsePassive = true,
    ValidateAnyCertificate = false
};
```

Never commit real FTP credentials to GitHub.

---

# Example Complete Workflow

A typical application workflow can look like:

```csharp
FtpConfig config = new FtpConfig
{
    Host = ftpHost,
    Port = 21,
    Username = ftpUsername,
    Password = ftpPassword,

    UseSsl = true,
    UsePassive = true,
    ValidateAnyCertificate = false
};

FTPManager ftp = new FTPManager(config);

// 1. Test connection
FtpResult connection =
    ftp.TestConnection();

if (!connection.Success)
{
    Console.WriteLine(
        connection.ErrorMessage
    );

    return;
}

// 2. Upload
FtpResult upload =
    ftp.UploadFile(
        localFile,
        remoteFile
    );

if (!upload.Success)
{
    Console.WriteLine(
        upload.ErrorMessage
    );

    return;
}

// 3. Verify existence
FtpResult exists =
    ftp.FileExists(
        remoteFile
    );

if (exists.Success)
{
    Console.WriteLine(
        "File is available on FTP."
    );
}

// 4. Delete when required
FtpResult delete =
    ftp.DeleteFile(
        remoteFile
    );

if (delete.Success)
{
    Console.WriteLine(
        "File deleted."
    );
}
```

---

# Current API

| Method              | Purpose                            |
| ------------------- | ---------------------------------- |
| `TestConnection()`  | Test FTP/FTPS connectivity         |
| `UploadFile()`      | Upload a local file                |
| `DownloadFile()`    | Download a remote file             |
| `DeleteFile()`      | Delete a remote file               |
| `FileExists()`      | Check whether a remote file exists |
| `CreateDirectory()` | Create a remote directory          |
| `DeleteDirectory()` | Delete a remote directory          |
| `RenameFile()`      | Rename/move a remote file          |

---

# Example Project

A Windows Forms test application can use:

```csharp
using FTPUtility;
using FTPUtility.Models;

FtpConfig config = new FtpConfig
{
    Host = "ftp.example.com",
    Port = 21,
    Username = "user",
    Password = "password",

    UseSsl = true,
    UsePassive = true,
    ValidateAnyCertificate = true
};

FTPManager ftp =
    new FTPManager(config);

FtpResult result =
    ftp.UploadFile(
        @"C:\Files\test.pdf",
        "/test/test.pdf"
    );

if (result.Success)
{
    MessageBox.Show(
        "Upload successful."
    );
}
else
{
    MessageBox.Show(
        result.ErrorMessage
    );
}
```

---

# Project Status

Current functionality:

* [x] FTP connection
* [x] Explicit FTPS
* [x] Username/password authentication
* [x] Certificate validation configuration
* [x] Passive/active configuration
* [x] Upload
* [x] Upload verification fallback
* [x] Download
* [x] Delete
* [x] File existence check
* [x] Create directory
* [x] Delete directory
* [x] Rename/move file
* [x] Structured result handling

Potential future improvements:

* [ ] Async API
* [ ] CancellationToken support
* [ ] Progress reporting
* [ ] Configurable retry policies
* [ ] Structured logging
* [ ] Directory listing API
* [ ] File metadata API
* [ ] Checksum/hash verification
* [ ] NuGet package
* [ ] Unit/integration tests
* [ ] .NET Standard / modern .NET support
* [ ] Dependency injection support

---

# Important Notes

FTP server behavior varies significantly between implementations.

A server that works correctly with one FTP client may behave differently with another client because of differences in:

* Passive mode
* EPSV/PASV
* TLS negotiation
* TLS data-channel handling
* Certificate validation
* Firewall/NAT configuration
* Server-side FTP configuration

Therefore, FTPUtility should not assume that an FTP response such as `425` automatically means that the file was successfully uploaded.

The library's upload verification exists specifically to handle cases where the server appears to have received the file but the client receives an error during transfer completion.

---

# Contributing

Contributions are welcome.

Recommended workflow:

```text
Fork repository
    ↓
Create feature branch
    ↓
Implement change
    ↓
Add/update tests
    ↓
Build solution
    ↓
Submit Pull Request
```

When submitting FTP-related changes, include:

* FTP server type/version if known
* FTP vs FTPS
* Passive/active mode
* Relevant FTP response
* FluentFTP version
* .NET Framework/.NET version
* Reproduction steps

Never include:

```text
FTP password
Private keys
Production credentials
Sensitive server information
```

---

# Disclaimer

This project is intended to simplify FTP/FTPS integration in C# applications.

FTP server behavior depends on the server implementation and network environment. Always test the library against the specific FTP/FTPS server used by your application before deploying it to production.
