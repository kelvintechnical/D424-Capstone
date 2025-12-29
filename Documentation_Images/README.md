# Documentation Images for Student Progress Tracker

This folder contains professional documentation diagrams generated for the WGU Software Engineering Capstone (D424 Task 3).

## Generated Diagrams

### 1. `architecture_diagram.png`
**Three-Tier Architecture Diagram**
- **Resolution:** 2400x1600 pixels (300 DPI)
- **Description:** Shows the complete system architecture with three layers:
  - Top Layer: .NET MAUI Mobile Client (iOS/Android) with ViewModels, Views, and ApiService
  - Middle Layer: ASP.NET Core Web API (Azure App Service) with Controllers, Models, and DbContext
  - Bottom Layer: Azure SQL Database with all tables
- **Connections:** HTTPS/JSON RESTful API (MAUI → API) and Entity Framework Core ORM (API → Database)

### 2. `gpa_calculation_flow.png`
**GPA Calculation Data Flow Sequence Diagram**
- **Resolution:** 1800x1080 pixels (300 DPI)
- **Description:** UML sequence diagram showing the complete flow of GPA calculation:
  1. User interaction in MAUI app
  2. ViewModel calls ApiService
  3. API request to GradesController
  4. Database query via EF Core
  5. GPA calculation (weighted formula)
  6. Response flow back to UI

### 3. `database_erd.png`
**Entity Relationship Diagram (ERD)**
- **Resolution:** 2400x1600 pixels (300 DPI)
- **Description:** Database schema diagram showing:
  - All tables: Terms, Courses, Assessments, Grades, Income, Expenses
  - Primary Keys (PK) and Foreign Keys (FK)
  - Relationships with crow's foot notation (1:M)
  - Complete field listings for each table

### 4. `mvvm_pattern.png`
**MVVM Pattern Architecture Diagram**
- **Resolution:** 1875x1312 pixels (300 DPI)
- **Description:** Shows the Model-View-ViewModel pattern implementation:
  - **View Layer:** GPAPage, TermsPage, CoursesPage (XAML)
  - **ViewModel Layer:** GPAViewModel, TermsViewModel, CoursesViewModel with commands
  - **Model/Service Layer:** ApiService, Models, Services
  - Two-way data binding arrows
  - Observable properties update flow

### 5. `csv_export_flow.png`
**CSV Export Flow Diagram**
- **Resolution:** 1500x1875 pixels (300 DPI)
- **Description:** Flowchart showing the complete CSV export process:
  - User initiates export
  - Data validation
  - API service calls
  - Database query and CSV formatting
  - File system operations
  - Native share dialog
  - Decision points and error handling

## Technical Specifications

- **Format:** PNG with transparent/white backgrounds
- **Resolution:** 300 DPI (print quality)
- **Color Scheme:** Professional blue/gray palette
- **Style:** Clean, modern, academic documentation standard

## Regenerating Diagrams

To regenerate all diagrams, run:

```bash
python generate_all_diagrams.py
```

Or generate individual diagrams:

```bash
python generate_architecture_diagram.py
python generate_gpa_flow.py
python generate_erd.py
python generate_mvvm_diagram.py
python generate_csv_export_flow.py
```

## Dependencies

Required Python packages (see `requirements.txt`):
- matplotlib >= 3.7.0
- numpy >= 1.24.0

Install with:
```bash
pip install -r requirements.txt
```

## Usage in Documentation

These images are designed to be included in:
- Technical documentation
- Academic reports
- System architecture presentations
- Developer onboarding materials
- Project proposals

All diagrams follow professional documentation standards suitable for academic submission.

