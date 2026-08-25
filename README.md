# FTPUtility

A **lightweight and reusable C# FTP class library (DLL)** designed to simplify network file transfers with minimal code integration. Easily inject FTP functionality into any **Web, Desktop, API, or Web Service** ecosystem without rewriting complex transfer logic.

## 🚀 Features
* **Plug-and-Play:** Reference the DLL and start transferring files immediately.
* **Universal:** Built for Web, Desktop, API, and Web Services.
* **Minimalistic:** Eliminates boilerplate connection and transfer logic.

---

## 🛠️ Installation & Requirements

### 1. Add Dependencies
This utility relies on **FluentFTP**. Run the following command in your Package Manager Console:

```bash
Install-Package FluentFTP
```

*Note: If you encounter TLS exceptions or connection errors, install the GnuTLS extension:*
```bash
Install-Package FluentFTP.GnuTLS
```

### 2. Add the Reference
1. Import **`FTPUtility.dll`** into your solution's **References**.
2. For a practical implementation guide, refer to the **`FTPTesting.cs`** file located in the solution's `Refer` folder.

---

## 💻 Quick Start Example

```csharp
using FTPUtility;

// Example initialization and usage
FTPClient client = new FTPClient("://yourserver.com", "username", "password");
client.UploadFile("localpath/file.txt", "remotepath/file.txt");
```
