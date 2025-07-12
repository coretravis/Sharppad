# Sharppad

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)  
[![GitHub stars](https://img.shields.io/github/stars/coretravis/sharppad)](https://github.com/coretravis/sharppad/stargazers)

Sharppad is an open-source, browser-based interactive development environment for writing, executing, embedding, and sharing C# code.

---

![Sharppad Editor][screenshot]

---

## Table of Contents

- [Overview](#overview)
- [Project Status](#project-status)
- [Demo](#demo)
- [Features](#features)
- [Endpoints](#endpoints)
- [Architecture & Tech Stack](#architecture--tech-stack)
- [Installation](#installation)
  - [Prerequisites](#prerequisites)
  - [Clone the Repository](#clone-the-repository)
  - [Configuration](#configuration)
  - [Build and Run](#build-and-run)
- [Usage](#usage)
- [Tests](#tests)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Overview

Sharppad empowers developers to quickly prototype and execute .NET C# scripts and programs directly in the browser. Its intuitive Monaco Editor integration, seamless real-time server-side execution, and advanced features make experimenting with ideas or sharing complete programs a breeze.

---

## Project Status

**Disclaimer: This project is in its early stages.**

Sharppad is currently in the alpha phase, and many of the features documented are experimental. While the core functionality is operational, significant work remains to refine the interface, enhance performance, and expand the feature set. Expect frequent updates, potential bugs, and breaking changes as the project evolves. We welcome feedback, bug reports, and contributions from the community to help guide Sharppad toward a stable and fully-featured release.

---

## Demo

A live demo of Sharppad is available to explore its features in action. Visit the demo at:  
[Shappard Demo](https://cronux-sharp-dme6ccfmehgbf4c2.westeurope-01.azurewebsites.net/)  

---

## Features

- **Interactive Code Editor**  
  Powered by the [Monaco Editor](https://microsoft.github.io/monaco-editor/), Sharppad offers syntax highlighting, IntelliSense, auto-completion, diagnostics, and “Go-To Definition” via Roslyn.

- **Code Execution**  
  Run your C# scripts on the server with support for multiple .NET compiler versions. Detailed execution output includes standard output, error messages, and generated files.  
  You can execute scripts and programs interactively which allows expected functionality like Console Input and streaming outputs.

- **Rich Code Execution Output Panel**  
  Enjoy an advanced output panel with multiple tabs (Output, Errors, etc.) and integrated controls for search, copy, export, and fullscreen mode.

- **Script Management & Sharing**  
  Create, save, update, and delete scripts within an integrated library. Sharppad lets you generate embed codes or shareable links so you can easily embed your scripts on web pages or share them with others.

- **AI-Powered Code Assistance**  
  Receive automated code explanations, optimizations, documentation enhancements, and answers to coding questions with context-aware AI endpoints.

- **NuGet Package Integration**  
  Add or remove NuGet packages to include external libraries and dependencies in your scripts and programs.

- **File Management**  
  Upload and download files to support complex execution scenarios, with configurable storage settings and file size limits.

- **Authentication & Authorization**  
  Secure user registration, login, and external authentication with access control for managing private and public scripts.

---

## Endpoints

Sharppad exposes a set of RESTful endpoints to power its features:

- **Authentication:**  
  Endpoints for login, registration, and external authentication.

- **AI Code Assistance:**  
  Endpoints to explain, optimize, document, and answer questions about your code.

- **Code Execution:**  
  An endpoint to compile and execute scripts (interactive and non-interactive), returning detailed execution results and metrics.

- **File Management:**  
  Endpoints for uploading and downloading files.

- **Script Library:**  
  Endpoints to create, retrieve, update, delete, and search for scripts (both personal and public).

- **Roslyn Integration:**  
  Endpoints for auto-completion, diagnostics, formatting, and definition lookup.

---

## Architecture & Tech Stack

- **Frontend:**  
  - **Framework:** Blazor (WebAssembly/Server) with Razor Components  
  - **Editor:** Monaco Editor via JavaScript interop  
  - **UI Components:** Rich output panel, modals, side panels, and real-time state management  
  - **SignalR:** Real-time streaming for interactive code sessions

- **Backend:**  
  - **Framework:** ASP.NET Core Web API  
  - **SignalR:** Real-time streaming for interactive code sessions  
  - **EFCore:** SQL Database for Authentication and Script storage  
  - **Services:**  
    - Authentication  
    - Code Execution  
    - AI Code Assistance  
    - File Management  
    - Script Library Management  
    - Roslyn-based IDE features  
  - **Design:** RESTful API with dependency injection and modular services

- **Additional Integrations:**  
  - **Logging & Monitoring:** For error tracking and performance metrics  
  - **Security:** Token-based authentication, OAuth, and robust authorization checks

---

## Installation

### Prerequisites

- [.NET 8 SDK or later](https://dotnet.microsoft.com/download)
- Git
- A modern code editor (e.g., [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/))

### Clone the Repository

```bash
git clone https://github.com/coretravis/sharppad.git
cd sharppad
```

### Configuration

1. **Backend Configuration:**  
   Update the `appsettings.json` file in the server project to configure:
   - File storage path and maximum file size
   - Database connection strings 
   - Secrets
   - Other environment-specific settings

2. **Frontend Configuration:**  
   Ensure that JavaScript interop modules (e.g., for the Monaco Editor and output panel) are located in the `wwwroot/js` directory and that paths in the Razor components match your project structure.

### Build and Run

#### Using the Command Line

```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

#### Using Visual Studio

- Open the solution in Visual Studio.
- Set the startup project to the Sharppad server project.
- Press `F5` to build and run the application.

Access the application at [http://localhost:5000](http://localhost:5000) (or your configured port).

---

## Usage

1. **Access the Application:**  
   Open your web browser and navigate to the Sharppad URL or the live demo.

2. **Editing Scripts:**  
   Click the **"New Script"** button to start with a default C# template, and write or modify your script using the integrated Monaco Editor.

3. **Executing Scripts:**  
   Click the **"Run"** button to execute your code. The rich output panel will display results, errors, detailed metrics, and additional controls.

4. **Script Management & Sharing:**  
   Log in to access your Script Library to save, update, or delete scripts. Generate an embed code to insert your script into other web pages or create a shareable link for direct access.

5. **AI Code Assistance:**  
   Use the AI-powered features to receive code explanations, optimizations, and documentation enhancements.

6. **NuGet & File Management:**  
   Manage dependencies by adding or removing NuGet packages. Upload necessary files or download outputs as required.

---

## Tests

Sharppad includes a test suite to ensure functionality and maintain code quality. While the current tests cover essential features, additional tests are planned for future releases.

### Running Tests

To run the tests, use the following command from the solution directory:

```bash
dotnet test
```

Contributions to expand the test suite are welcome.

---

Below is the updated "Roadmap" section with additional details on the cross-platform mobile app:

---

## Roadmap

- **Mobile Compatibility (Design):**  
  We plan to optimize Sharppad’s user interface for mobile devices, ensuring a responsive design and seamless interaction on smaller screens.

- **Benchmarking and Exporting:**  
  Future updates will include robust benchmarking tools to measure code performance, along with the ability to export results in various formats (e.g., JSON, CSV, PDF) for further analysis and sharing.

- **Enhanced Console Tools and Reporting:**  
  We aim to improve the integrated console with advanced debugging features, detailed logging, and reporting capabilities to provide better insights during code execution.

- **Cross Platform Mobile App:**  
  We are exploring the development of a dedicated cross-platform mobile application using .NET MAUI Blazor Hybrid. This approach will allow us to leverage the strengths of .NET MAUI to deliver a native, high-performance app across iOS, Android, Windows, and macOS. By utilizing Blazor Hybrid, we can reuse and adapt many of our existing Razor components and Blazor-based UI elements, ensuring a consistent user experience between the web and mobile platforms.  
  The mobile app will feature:
  - A responsive, touch-friendly code editor integrated with Monaco Editor capabilities.
  - Real-time code execution and output streaming, similar to the browser experience.
  - Seamless synchronization with Sharppad’s cloud services for script management, file uploads, and authentication.
  - Enhanced mobile-specific optimizations, including offline support and intuitive navigation.
  
Your feedback is invaluable as we work on these features, so please share your thoughts or contribute to the discussion on GitHub!

---

## Contributing

Contributions are welcome! To contribute:

1. **Fork the Repository:**  
   Click the **Fork** button on GitHub to create your own copy.

2. **Create a Branch:**  
   Create a new branch for your feature or fix:
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Develop Your Changes:**  
   Follow the existing coding conventions and add tests where applicable.

4. **Commit and Push:**  
   Commit your changes and push them to your fork:
   ```bash
   git commit -m "Describe your feature or fix"
   git push origin feature/your-feature-name
   ```

5. **Submit a Pull Request:**  
   Open a pull request against the main repository with a clear description of your changes.

For more detailed guidelines, please see [CONTRIBUTING.md](CONTRIBUTING.md).

---

## Communication

- **GitHub Issues & Discussions:**  
  Use [GitHub Discussions](https://github.com/coretravis/sharppad/discussions) to ask questions, propose ideas, or get help.
- **Direct Contact:**  
  If you need further assistance, feel free to reach out via email at [info@coretravis.work](mailto:info@coretravis.work).

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Contact

Please contact [info@coretravis.work](mailto:info@coretravis.work).

---

Sharppad is an evolving project—your feedback and contributions are invaluable. Happy coding!

[screenshot]: docs/assets/home.png
