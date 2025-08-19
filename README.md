# Cutting Optimizer Demo
## A WPF application for optimizing the cutting layout of glass panels on stock sheets, designed to minimize waste and optimize cutting efficiency.

## Overview
This application helps optimize the placement of multiple panels on stock sheets using advanced packing algorithms. It generates both the optimal layout and the cutting sequence required to produce the panels with minimal waste.

## Features
- **Smart Panel Packing**: Uses a guillotine-based bin packing algorithm to efficiently place panels  
- **Cut Optimization**: Generates optimal cutting sequences with full-length cuts where possible  
- **Visual Layout**: Real-time visualization of panel placements and cut lines  
- **Export Functionality**: Save layouts as PNG images for documentation  
- **Database Integration**: Uses Entity Framework Core for data persistence  
- **Margin Management**: Automatically accounts for 20mm margins around sheet edges  

## Key Components

### PackingService
- Implements guillotine bin packing algorithm  
- Prioritizes panels by area (largest first)  
- Considers panel rotation for optimal fit  
- Favors full-width cuts to reduce cutting time  
- Maintains 20mm margins from sheet edges  

### CutService
- Generates cutting sequences from panel placements  
- Distinguishes between full-length and partial cuts  
- Optimizes cut paths to avoid panel intersections  
- Creates both horizontal and vertical cut segments  

### MainWindow (WPF UI)
- Interactive sheet selection  
- Real-time optimization visualization  
- Zoom and pan controls for detailed inspection  
- PNG export functionality  
- Status updates during optimization  

## Algorithm Details

### Packing Strategy
- Sort panels by descending area to place larger items first  
- Find best fit using area-based scoring with full-width bonuses  
- Place panel at optimal position within available free rectangles  
- Split remaining space using guillotine cuts (vertical or horizontal)  
- Remove overlaps to maintain clean free rectangle list  

### Cutting Strategy
- Extract coordinates from all panel boundaries  
- Generate cut segments that don't intersect placed panels  
- Identify full-length cuts for efficiency  
- Optimize cut sequence to minimize tool movement  

## Technical Requirements
- .NET Framework/Core with WPF support  
- Entity Framework Core for database operations  
- Windows Presentation Foundation (WPF) for UI  

## Usage
1. Select a stock sheet from the dropdown menu  
2. Click "Optimize" to run the packing algorithm  
3. View the layout with panels (blue) and cuts (red/green lines)  
4. Export results as PNG if needed  
5. Use zoom controls to inspect detailed placements  

## Color Coding
- **Blue rectangles**: Placed panels with dimensions  
- **Red lines**: Full-length cuts (optimal for efficiency)  
- **Green dashed lines**: Partial cuts  
- **Gray border**: Sheet boundaries  
- **White area**: Usable cutting area (within margins)  
