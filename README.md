<h1 align="center">
<img src="ScreenShot/logo.png" alt="Ping Legacy" width="150">
<br/>
Ping Legacy
<br/>
<a href='https://www.microsoft.com/store/apps/9P1KVKT59T2M'><img src='https://raw.githubusercontent.com/avikeid2007/WinDev-Utility/dev/ScreenShots/store.png' alt='Get it on Microsoft Store' width="150" /></a>
</h1>

<p align="center">
<b>A modern network diagnostic tool for Windows</b><br/>
Test your connection status and quality with ease.
</p>

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🌐 **Live Ping Testing** | Continuous ICMP ping to any host with real-time results |
| 🎯 **Multi-Ping Monitor** | Ping up to 8 targets simultaneously with live dashboard |
| 📊 **Statistics** | Min/Max/Avg latency, packet loss percentage |
| 📈 **Real-time Graph** | Visual latency chart showing ping trends |
| ⭐ **Favorites** | Quick access to frequently pinged hosts |
| 🌙 **Dark/Light Theme** | System-aware theming support |
| 📁 **Export** | Save ping results to text file with statistics |
| 🔌 **Network Info** | IPv4, IPv6, Public IP, profile, adapters |
| 🌍 **Public IP** | Auto-fetch external IP with copy support |
| 🔍 **DNS Lookup** | Resolve hostnames and display IP records |
| 🔔 **Drop Notifications** | Windows toast alerts on connection loss |
| 🛤️ **Traceroute** | Trace hop-by-hop path to any destination |
| 🔐 **Port Scanner** | Check open ports with legal authorization |
| ⏰ **Scheduled Pings** | Monitor hosts at regular intervals |
| 🚀 **Speed Test** | Test download, upload speeds and latency |
| 📜 **Unified History** | View all operations with filtering & export |
| 📉 **Network Statistics** | Track network data usage with connection info |
| 🗑️ **Auto Cleanup** | Configurable history retention (default 15 days) |
| 💬 **GitHub Feedback** | Submit suggestions directly to GitHub Issues |

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `F5` | Start ping |
| `Esc` | Stop ping |
| `Ctrl+E` | Export results |
| `Ctrl+Delete` | Clear results |
| `Ctrl+F` | Add to favorites |

## 🛠️ Tech Stack

- **WinUI 3** - Modern Windows UI framework
- **Windows App SDK 1.5** - Latest Windows development platform
- **.NET 8** - Cross-platform runtime
- **CommunityToolkit.Mvvm** - MVVM architecture
- **SQLite** - Local data storage

## 📸 Screenshots

| Main Ping | Multi-Ping Monitor |
|-----------|--------------------|
| ![Main](ScreenShot/Screenshot%202026-01-16%20142133.png) | ![Multi-Ping](ScreenShot/Screenshot%202026-01-16%20142206.png) |

| Speed Test | History |
|------------|--------|
| ![Speed Test](ScreenShot/Screenshot%202026-01-16%20142244.png) | ![History](ScreenShot/Screenshot%202026-01-16%20142301.png) |

## 🚀 Getting Started

### Prerequisites

- Windows 10 version 1809 or later
- Visual Studio 2022 with Windows App SDK workload

### Build

```bash
git clone https://github.com/avikeid2007/Ping-Tool.git
cd Ping-Tool
dotnet build PingTool.WinUI3.sln -c Release -p:Platform=x64
```

### Run

Open `PingTool.WinUI3.sln` in Visual Studio and press F5.

## 📄 License

MIT

---

> [avnishkumar.co.in](http://avnishkumar.co.in) &nbsp;·&nbsp;
> GitHub [@avikeid2007](https://github.com/avikeid2007) &nbsp;·&nbsp;
> Twitter [@avikeid2007](https://twitter.com/avikeid2007)
