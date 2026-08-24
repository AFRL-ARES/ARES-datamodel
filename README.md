# ARES-datamodel

<!-- License -->
[![License](https://img.shields.io/github/license/AFRL-ARES/ARES-datamodel)](LICENSE)

<!-- Latest Release -->
[![GitHub release](https://img.shields.io/github/v/release/AFRL-ARES/ARES-datamodel)](https://github.com/AFRL-ARES/ARES-datamodel/releases)

---

## Overview

The **ARES-datamodel** repository hosts the protobuf defintions and their generated artifacts (as well as a few tools) that make up the ARES data model. Its main role is to centralize the data definitions and share them across the various ARES services.  
The repository does **not** (typically) contain handwritten business logic; instead it focuses on the data interface layer.

You can find the main documentation on the data model [here](https://afrl-ares.github.io/docs/datamodel/intro) as well as the rest of the ARES ecosystem by visting https://afrl-ares.github.io/ to browse our centralized documentation site.

---

<a id="repository-structure"></a>
## 📂 Repository Structure

| Path | Description |
|------|--------------|
| **protos/** |- Core `.proto` files that define ARES message types and gRPC services |
| **dotnet/** |- Auto Generated C# source files and .NET project configuration |
| **python/** |- Python stubs, build scripts, or utilities for cross-language support |

This structure separates **source definitions** (`protos/`) from **language-specific builds** (`dotnet/`, `python/`), ensuring clarity and reproducibility when regenerating code.

---

<a id="license"></a>
## ⚖️ License

This project, as well as the rest of the ARES software, is licensed under the MIT License. <br></br>
The ARES Datamodel has been cleared for release under clearance number AFRL-2025-5329.

<a id="contact"></a>
## 📬 Contact

For questions, bug reports, or schema proposals please open an issue in this repository with a detailed description of your problem, and how to reproduce it if applicable.
