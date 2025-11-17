# Guide for VS Code & JetBrains Rider

This guide provides instructions for running the ChatClient application in Visual Studio Code and JetBrains Rider.

## Visual Studio Code

### Prerequisites

1.  **.NET 6.0 SDK**: Download and install the .NET 6.0 SDK from the official Microsoft website: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2.  **C# Extension for VS Code**: Install the official C# extension from the Visual Studio Marketplace: [https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

### Steps to Run

1.  **Open the Project Folder**: Open the root folder of this project in Visual Studio Code.
2.  **Restore Dependencies**: Open the terminal in VS Code (`Ctrl+\` `) and run the following command to restore the necessary NuGet packages:
    ```bash
    dotnet restore
    ```
3.  **Run the Application**: In the same terminal, run the following command to build and run the application:
    ```bash
    dotnet run
    ```

## JetBrains Rider

### Prerequisites

1.  **.NET 6.0 SDK**: Make sure you have the .NET 6.0 SDK installed. You can download it from [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

### Steps to Run

1.  **Open the Project**: In Rider, select **File > Open > Project or Solution** and choose the `ChatClient.csproj` file.
2.  **Restore Dependencies**: Rider will automatically restore the NuGet packages when you open the project.
3.  **Run the Application**: Click the green "Run" button in the toolbar (or press `F5`) to build and run the application.
