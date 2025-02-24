# Installation Guide for Sharppad

This guide will help you set up Sharppad on your local machine. Follow the steps below to install all prerequisites, clone the repository, configure the project, and run the application.

---

## Prerequisites

Before installing Sharppad, ensure you have the following installed:

- **.NET 8 SDK or later**  
  Download from [Microsoft .NET](https://dotnet.microsoft.com/download).

- **Node.js and npm** (if needed for managing frontend dependencies)  
  Download from [Node.js](https://nodejs.org/).

- **Git**  
  Download from [Git SCM](https://git-scm.com/).

- A modern code editor such as **Visual Studio** or **Visual Studio Code**.

---

## Cloning the Repository

Open your terminal or command prompt and run the following commands:

```bash
# Clone the Sharppad repository
git clone https://github.com/coretravis/sharppad.git

# Change into the project directory
cd sharppad
```

---

## Configuration

### Backend Configuration

1. **Update `appsettings.json`:**  
   In the server project, locate the `appsettings.json` file and update the following settings:
   - **File Storage Path & Maximum File Size:**  
     Configure the path where uploaded files will be stored and set a limit on file sizes.
   - **Database Connection Strings:**  
     Provide the correct connection string for your database.
   - **Environment-Specific Settings:**  
     Adjust any other settings (like logging levels, API endpoints, etc.) according to your local environment.

### Frontend Configuration

1. **JavaScript Interop:**  
   Ensure that the JavaScript modules for the Monaco Editor and the output panel are placed in the `wwwroot/js` directory.
2. **Razor Component Paths:**  
   Verify that all paths referenced in the Razor components match your project structure.

---

## Building and Running Sharppad

### Using the Command Line

1. **Build the Project:**  
   In your terminal, run:
   ```bash
   dotnet build
   ```

2. **Run the Application:**  
   Once the build completes, start the application with:
   ```bash
   dotnet run
   ```

3. **Access the Application:**  
   Open your web browser and navigate to:  
   ```
   http://localhost:5000
   ```
   *(or the port specified in your configuration)*

### Using Visual Studio

1. **Open the Solution:**  
   Open the Sharppad solution file in Visual Studio.

2. **Set Startup Project:**  
   Right-click the Sharppad server project in the Solution Explorer and select **"Set as Startup Project"**.

3. **Run the Application:**  
   Press **F5** (or click the **"Start"** button) to build and run Sharppad.

4. **Access the Application:**  
   Once running, open your browser and navigate to:
   ```
   http://localhost:5000
   ```
   *(or your configured port)*

---

## Troubleshooting Installation Issues

- **Dependency Errors:**  
  If you encounter issues during `dotnet restore` or `dotnet build`, make sure all prerequisites are installed and that your environment variables are correctly set.

- **Configuration Problems:**  
  Double-check your `appsettings.json` file for any incorrect or missing configuration values.

- **Frontend Issues:**  
  If the Monaco Editor or other frontend components aren�t loading properly, inspect the browser console for JavaScript errors and verify the asset paths in `wwwroot/js`.

---

Happy coding and enjoy using Sharppad!