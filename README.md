# InsightVision V2.0 - MCS

**InsightVision V2.0** is a comprehensive software suite for Computed Tomography (CT) scanners. This repository contains the **MCS (Main Control Software)** component, which provides the main user interface and control logic for the CT system.

## About The Project

The MCS is a modular, multi-layered application built with .NET. It handles everything from patient management and scan protocols to image reconstruction and viewing. It's designed to be extensible and configurable, with a rich set of features for clinical use.

### Features

*   **Patient Management**: Register and manage patient data.
*   **Scan Protocol Management**: Create, edit, and manage scan protocols.
*   **Examination Workflow**: A complete workflow for patient examination.
*   **Image Viewer**: View and analyze DICOM images.
*   **Offline Reconstruction**: Reconstruct CT images from raw data.
*   **Job Management**: A-queue and monitor system jobs like printing and archiving.
*   **System Services**: A suite of background services for logging, synchronization, and more.
*   **Hardware Control**: Interfaces for controlling CT scanner hardware.
*   **Service Tools**: A collection of tools for calibration, testing, and maintenance.

## Getting Started

To get a local copy up and running follow these simple steps.

### Prerequisites

*   .NET 8.0 SDK
*   C++ Redistributable (x64)
*   MySQL Server 8.0.30

### Installation & Building

1.  Clone the repo.
2.  Make sure you have all the prerequisites installed.
3.  The solution can be built and published using the standard `dotnet` CLI commands:
    ```sh
    dotnet build
    dotnet publish
    ```
4.  You can also use the scripts in the `CompilingManager` directory to build the entire system.

## Usage

The main entry points for the application are the `Console` projects:

*   `Console/NanoConsole`: The main console application.
*   `Console/AuxConsole`: The auxiliary console application.

These projects can be run after a successful build. Configuration files for the system are located in the `Config` directory.

## Project Structure

The project is organized into several layers and components:

*   `CompilingManager`: Contains scripts and tools for compiling the entire system.
*   **`Config`**: System configuration files.
    *   `Common`: Common configuration files for all components.
    *   `ConfigMCS`: Configuration files specific to the MCS.
*   **`Infrastructure`**: Core infrastructure components.
    *   `Daemon`: A daemon process for starting and managing services.
    *   `Logging`: A library for logging.
    *   `LoggingServer`: A service for collecting and storing logs.
*   **`Common`**: Shared libraries and components.
    *   `CommonAttribute`: A library for Aspect-Oriented Programming (AOP).
    *   `CTS`: The Common Type System, containing shared data models and enums.
    *   `DicomUtility`: A library for working with DICOM files.
    *   `Controls`: A library of custom UI controls.
    *   `Protocol`: A library for managing scan protocols.
*   **`Skin`**: UI skins, styles, and localization.
    *   `MaterialDesign`: A library for the Material Design visual style.
    *   `Language`: Resources for multi-language support.
    -   `ErrorCode`: Definitions for system error codes.
*   **`SystemService`**: Backend services.
    *   `CoreService`: Core services for the system.
    *   `JobService`: A service for managing background jobs.
    *   `SyncService`: A service for data synchronization.
*   **`SystemInterface`**: Interfaces to other systems and components.
    *   `MRSIntegration`: Integration with the MRS (Motion & Robotics System).
    *   `MCSRuntime`: A runtime library for the MCS.
*   **`Service`**: Standalone service applications and tools.
    *   `AutoCalibration`: A tool for automatic calibration.
    *   `HardwareTest`: A tool for testing hardware components.
    *   `QualityTest`: A tool for quality assurance testing.
    *   `TubeHistory`: A tool for tracking the history of the X-ray tube.
    *   `Upgrade`: A tool for firmware upgrades.
    *   `TubeCalibration`: A tool for calibrating the X-ray tube filament.
    *   `TubeWarmUp`: A tool for warming up the X-ray tube.
*   **`Console`**: The main console applications.
    *   `NanoConsole`: The main user interface application.
    *   `AuxConsole`: An auxiliary console application.
*   **`Alg`**: Algorithms for scan and reconstruction calculations.
*   **`Application`**: Business logic and application-level services for various workflows.
    *   `Examination`: The main examination workflow.
    *   `Recon`: Offline image reconstruction.
    *   `ImageViewer`: The image viewer application.
    *   `Intervention`: The interventional scan workflow.
    *   `PatientBrowser`: The patient browser and registration application.
    *   `PatientManagement`: The patient management application.
    *   `JobViewer`: A tool for viewing and managing system jobs.
    *   `Print`: The printing application.
    *   `ProtocolManagement`: A tool for managing scan protocols.
    *   `RGT`: The tablet application.

## Built With

*   [fo-dicom](https://github.com/fo-dicom/fo-dicom)
*   [AspectInjector](https://github.com/pamidur/aspect-injector)
*   MySQL + [Dapper](https://github.com/DapperLib/Dapper)
*   gRPC + [NamedPipeWrapper](https://github.com/andrerpena/named-pipe-wrapper)
*   [Autofac](https://autofac.org/)
*   [AutoMapper](https://automapper.org/)
*   [Newtonsoft.Json](https://www.newtonsoft.com/json)
*   [Prism](https://prismlibrary.com/)
*   [Serilog](https://serilog.net/)
*   [LiveCharts](https://lvcharts.net/)

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

## License

Distributed under the MIT License. See `LICENSE` for more information.
