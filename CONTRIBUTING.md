# Contributing to Sharppad

Thank you for your interest in contributing to Sharppad! Your contributions help improve the project and make it better for everyone. This document outlines the guidelines for contributing to the repository.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Pull Requests](#pull-requests)
- [Development Guidelines](#development-guidelines)
- [Setting Up the Development Environment](#setting-up-the-development-environment)
- [Branching and Commit Conventions](#branching-and-commit-conventions)
- [Communication](#communication)
- [License](#license)
- [Acknowledgements](#acknowledgements)

---

## Code of Conduct

Sharppad adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). Please read it to understand the expectations for community behavior. We are committed to creating a welcoming and respectful environment for everyone.

---

## How Can I Contribute?

There are several ways you can contribute to Sharppad:

### Reporting Bugs

If you encounter any bugs or issues:
- **Search** existing issues to avoid duplicates.
- **Open an Issue** with a clear and descriptive title.
- **Provide Details:**  
  Include steps to reproduce the bug, the expected behavior, screenshots (if applicable), and any error messages.

### Suggesting Enhancements

Have an idea for a new feature or improvement?
- **Search** existing discussions to ensure the idea hasn’t been suggested already.
- **Open a Discussion:**  
  Clearly explain your idea and why it would benefit the project.
- **Discuss:**  
  Engage with the community and maintainers to refine the idea.

### Pull Requests

If you’d like to contribute code:
1. **Fork the Repository:**  
   Click the "Fork" button on GitHub to create your own copy.
2. **Create a Branch:**  
   Use a descriptive branch name for your feature or bug fix.  
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Develop Your Changes:**  
   - Follow the existing coding style.
   - Add tests for your changes when applicable.
   - Document your code as needed.
4. **Commit and Push:**  
   Write clear, descriptive commit messages.  
   ```bash
   git commit -m "Describe your changes"
   git push origin feature/your-feature-name
   ```
5. **Submit a Pull Request:**  
   Open a pull request against the main repository. In your PR, explain the changes and link to any relevant issues.

---

## Development Guidelines

- **Coding Style:**  
  Please adhere to the existing code style and formatting. Consistency is key.
- **Testing:**  
  Ensure that any new code includes tests or updates to the existing test suite.
- **Documentation:**  
  Update or add documentation when making significant changes or adding new features.

---

## Setting Up the Development Environment

To get started with developing on Sharppad:
1. **Clone the Repository:**
   ```bash
   git clone https://github.com/coretravis/sharppad.git
   cd sharppad
   ```
2. **Install Prerequisites:**
   - [.NET 8 SDK or later](https://dotnet.microsoft.com/download)
   - A modern code editor (e.g., Visual Studio or Visual Studio Code)
3. **Build the Project:**
   ```bash
   dotnet build
   ```
4. **Run Tests:**
   ```bash
   dotnet test
   ```

---

## Branching and Commit Conventions

- **Feature Branches:**  
  Create a new branch for every feature or bug fix. This keeps changes organized and isolated.
- **Commit Messages:**  
  Use clear, descriptive commit messages. A good commit message briefly explains the "what" and "why" of your changes.
- **Pull Requests:**  
  Ensure your pull request addresses a single issue or feature for easier review and integration.

---

## Communication

- **GitHub Issues & Discussions:**  
  Use [GitHub Discussions](https://github.com/coretravis/sharppad/discussions) to ask questions, propose ideas, or get help.
- **Direct Contact:**  
  If you need further assistance, feel free to reach out via email at [info@coretravis.work](mailto:info@coretravis.work).

---

## License

Sharppad is licensed under the MIT License. By contributing, you agree that your contributions will also be licensed under the MIT License.

---

## Acknowledgements

We appreciate your time and effort in helping improve Sharppad. Thank you for being a part of our community and for your valuable contributions!

Happy coding!