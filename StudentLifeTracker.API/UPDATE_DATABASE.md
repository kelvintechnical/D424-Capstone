# Database Schema Update Instructions

## Problem
The `Courses` table is missing the columns: `CreditHours`, `CurrentGrade`, and `LetterGrade` that are defined in the Course model.

## Solution Options

### Option 1: Run SQL Script (Quick Fix)
Run the `add_missing_columns.sql` script against your database:

1. Open SQL Server Management Studio or Azure Data Studio
2. Connect to your database server (LocalDB in this case)
3. Open the `add_missing_columns.sql` file
4. Execute it against the `StudentLifeTrackerDb` database

This will add the missing columns without affecting existing data.

### Option 2: Switch to Migrations (Recommended for Production)
For better database version control, consider switching from `EnsureCreated()` to migrations:

1. Install EF Core tools: `dotnet tool install --global dotnet-ef`
2. Create initial migration: `dotnet ef migrations add InitialCreate`
3. Update Program.cs to use `context.Database.Migrate()` instead of `EnsureCreated()`

Note: This option requires careful handling if you have existing data.













