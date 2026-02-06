# StickyNotes - Desktop Sticky Notes Application

A modern, feature-rich sticky notes application for Windows built with WPF (.NET) that runs in the system tray with global hotkey support.

![Application Screenshot](https://github.com/rammara/StickyNotes/blob/main/sample.png)

## ✨ Features

### 🎯 Core Functionality
- **System Tray Integration** - Runs in background with system tray icon
- **Global Hotkey Support** - Create new notes instantly from anywhere (default: Shift+F4)
- **Multiple Notes** - Create and manage multiple sticky notes simultaneously
- **Customizable Hotkeys** - Configure both main and save hotkeys
- **Single Instance** - Only one instance runs at a time, with argument passing

### 📝 Note Management
- **Rich Text Editing** - Full text editing with formatting support
- **Pinning Notes** - Keep notes always on top of other windows
- **Custom Fonts** - Configure default font family, size, weight, and style
- **Auto-Save Position** - Notes remember their position and size
- **Resizable Windows** - Custom resize handles on all edges
- **Drag & Drop** - Move notes by dragging the title bar

### ⚙️ Settings & Customization
- **Configurable Settings** - User-friendly settings window
- **Start with Windows** - Option to launch on system startup
- **Default Save Folder** - Choose where to save notes
- **Double-Click Action** - Configure tray icon double-click behavior
- **Font Configuration** - System font dialog integration

### 🔧 Advanced Features
- **Named Pipe Communication** - For single instance and inter-process communication
- **Global Keyboard Hook** - System-wide hotkey detection
- **Window Management** - Custom window positioning and activation
- **File Association** - Open note files directly with the application

## 🛠️ Technologies & Techniques

### Architecture & Patterns
- **MVVM Pattern** - Clean separation of concerns
- **Dependency Injection** - Custom ServiceProvider for service management
- **Event-Driven Architecture** - Global events for keyboard and system tray
- **Singleton Pattern** - Single application instance enforcement

### WPF & UI
- **Custom Window Chrome** - Borderless windows with custom title bar
- **Custom Controls** - Styled buttons, scrollbars, and icons
- **Data Binding** - Extensive use of WPF data binding
- **Converters** - Value converters for UI transformations
- **Styles & Templates** - Custom control styling

### System Integration
- **P/Invoke** - Extensive Windows API calls for system integration
- **Global Hooks** - Low-level keyboard hooks for hotkey detection
- **System Tray** - Native system tray integration
- **Registry Access** - For startup configuration
- **Named Pipes** - Inter-process communication

### Data & Serialization
- **JSON Serialization** - For settings and window positions
- **File I/O** - Note saving and loading
- **Clipboard Integration** - Copy note content to clipboard

### Async & Threading
- **Async/Await** - For pipe communication
- **Dispatcher** - UI thread synchronization
- **Thread Safety** - Mutex for single instance enforcement

## 📁 Project Structure
StickyNotes/
├── Models/ # Data models (SettingsModel, Hotkey, etc.)
├── ViewModels/ # MVVM ViewModels
├── Views/ # WPF Views and Windows
├── Services/ # Business logic and system services
├── Converters/ # WPF value converters
└── Resources/ # Icons and application resources

### Key Components
- **App.xaml(.cs)** - Application entry point and lifecycle
- **MainViewModel** - Main application controller
- **WindowNote** - Individual note window
- **SettingsViewModel** - Settings management
- **ServiceProvider** - Dependency injection container
- **GlobalHookService** - Global keyboard hook
- **TrayIconService** - System tray integration
- **NamedPipeService** - Inter-process communication

## 🚀 Getting Started

### Prerequisites
- .NET 6.0 or later
- Windows 10/11
- Visual Studio 2022 or later (for development)

### Installation
1. Download the latest release from the Releases page
2. Run the installer or executable
3. The application will start in the system tray

### Usage
- **Create New Note**: Press the configured hotkey (default: Shift+F4)
- **Save Note**: Ctrl+S or configured save hotkey
- **Pin Note**: Click the pin icon in the note title bar
- **Close Note**: Click the X icon or press Escape
- **Access Settings**: Right-click the system tray icon

## ⚙️ Configuration

### Settings File Location
Settings are stored in: %APPDATA%\StickyNotes\settings.json

### Default Hotkeys
- **Main Hotkey**: Shift+F4 (creates new note)
- **Save Hotkey**: F2 (saves current note)

## 🔧 Development

### Building from Source
1. Clone the repository
2. Open `StickyNotes.sln` in Visual Studio
3. Restore NuGet packages
4. Build and run the project

### Key Dependencies
- .NET 6.0 WPF
- System.Drawing.Common
- Windows API Code Pack (P/Invoke)

### Architecture Notes
- The application uses a custom service container for dependency management
- Global hooks are implemented via low-level keyboard hooks
- Window management uses native Windows APIs for proper foreground activation
- Named pipes ensure single instance with argument forwarding

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## ⚠️ Known Issues & Limitations

- Requires Windows due to extensive P/Invoke usage
- Global hooks may conflict with other keyboard hook applications
- Some antivirus software may flag global hooks

## 📞 Support

For issues and feature requests, please use the GitHub Issues page.

---

**Note**: This application makes extensive use of Windows APIs and may require administrative privileges for certain features like global hooks and startup registration.
