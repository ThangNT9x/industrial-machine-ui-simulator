# Industrial Machine UI Simulator

## Overview
A C#/.NET WPF desktop application that simulates an industrial machine operator UI.

This project is inspired by real industrial machine software and focuses on:
- machine state flow
- operator dashboard UI
- alarm and safety interlock behavior
- simulator control panel
- role-based navigation
- mock industrial signals

## Current Features
- [x] Main dashboard UI
- [x] Status summary bar
- [x] Right command bar
- [x] Bottom navigation with role-based access
- [x] Login and role switching
- [x] Basic machine state flow
- [x] Init / Start / Stop / Cycle Stop / Reset logic
- [x] Simulator control window
- [x] Power / Laser / Diode / Pulsing / Door / Alarm signals
- [x] Door interlock and reset behavior
- [x] Basic tower lamp simulation

## In Progress
- [ ] Alarm/event log page
- [ ] SQLite local storage
- [ ] JSON configuration
- [ ] Mock PLC / MES communication
- [ ] Demo GIF / portfolio screenshots cleanup

## Screenshots

### Home - Running State
![Home Running](docs/screenshots/01_home_running.PNG)

### Recipe Management
![Recipe Management](docs/screenshots/02_recipe_management.PNG)

### IO Simulation
![IO Simulation](docs/screenshots/03_io_simulation.PNG)

### MES Monitor
![MES Monitor](docs/screenshots/04_mes_monitor.PNG)

### Alarm State
![Alarm State](docs/screenshots/05_alarm_state.PNG)

### Simulator Control Window
<img width="1901" height="1032" alt="simulator-window" src="https://github.com/user-attachments/assets/87438761-f7bb-45b6-894e-0cc5b4b1ff64" />


## Project Goals
- Practice industrial desktop software architecture
- Build a portfolio-ready machine UI simulator
- Learn machine state management, safety interlock logic, and reusable WPF UI structure

## Tech Stack
- C#
- .NET
- WPF
- SQLite
- Serilog

## Status
Active development

## Next Steps
- Add alarm/event log page
- Add SQLite persistence
- Add JSON-based configuration
- Add mock communication layer
- Polish visuals and demo flow
