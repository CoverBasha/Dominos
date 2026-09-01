# Dominoes Game

A single-player Dominoes game built with **C# and WPF**, following the **MVVM architectural pattern**.

## Overview

This project is a desktop implementation of a single-player Dominoes game. It was developed to practice object-oriented programming, application architecture, and design patterns using C# and WPF. <br><br>This Project was revisited in late 2025 to **refactor** and practice **advanced C#** concepts like events and delegates.

## Technologies

- C#
- WPF
- XAML
- MVVM
- .NET

## Architecture

The application follows the **Model-View-ViewModel (MVVM)** pattern to separate the user interface from the application's logic.

### Model

Contains the core game entities and state, including the dominoes, players, and game-related logic.

### View

Contains the WPF/XAML interface used to display the game and interact with the player.

### ViewModel

Acts as the intermediary between the View and Model, exposing application state and commands to the UI.

## Design Patterns & Concepts

The project applies several C# and software design concepts, including:

- **MVVM** — Separating presentation, UI state, and application logic.
- **Command Pattern** — Handling user actions through commands.
- **Navigation Store** — Managing navigation between different views.
- **Events & Delegates** — Handling communication between components and game events.
- **Object-Oriented Programming** — Applying encapsulation, abstraction, inheritance, and polymorphism where appropriate.
- **Separation of Concerns** — Keeping UI responsibilities separate from game logic.

## Features

- Single-player Dominoes gameplay
- WPF graphical interface
- MVVM-based application structure
- Command-based user interactions
- Navigation between application views
- Game state management

## Getting Started

### Prerequisites

- .NET SDK
- Visual Studio with WPF support

### Running the Application

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Restore the project dependencies.
4. Build the solution.
5. Run the application.

## Project Structure

The project is organized around the MVVM architecture, with separate components for models, views, view models, commands, and application state/navigation.

## Purpose

This project was built to gain practical experience with **C#**, **WPF**, **MVVM**, and software design patterns while developing a complete desktop application from the ground up.
