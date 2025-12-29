"""
Generate Three-Tier Architecture Diagram for Student Progress Tracker
"""
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Rectangle
import numpy as np

# Set up the figure with high DPI for print quality
fig, ax = plt.subplots(figsize=(16, 10.67), dpi=150)  # 2400x1600 at 150 DPI
ax.set_xlim(0, 10)
ax.set_ylim(0, 10)
ax.axis('off')

# Color scheme - professional blue/gray
colors = {
    'client': '#4A90E2',      # Blue
    'api': '#5B9BD5',         # Medium blue
    'database': '#7F8C8D',    # Gray
    'connection': '#34495E',  # Dark gray
    'text': '#2C3E50',        # Dark blue-gray
    'bg': '#FFFFFF'           # White
}

# Layer 1: .NET MAUI Mobile Client
client_box = FancyBboxPatch((1, 7), 8, 2.2, 
                            boxstyle="round,pad=0.1", 
                            edgecolor=colors['client'], 
                            facecolor=colors['client'], 
                            linewidth=2, alpha=0.2)
ax.add_patch(client_box)

ax.text(5, 9.5, '.NET MAUI Mobile Client (iOS/Android)', 
        ha='center', va='center', fontsize=16, fontweight='bold', color=colors['text'])

# Client components
components = [
    ('ViewModels\n(MVVM pattern)', 2.5, 8.2),
    ('Views\n(XAML pages)', 5, 8.2),
    ('ApiService\n(centralized HTTP client)', 7.5, 8.2)
]

for text, x, y in components:
    comp_box = FancyBboxPatch((x-0.8, y-0.3), 1.6, 0.6,
                             boxstyle="round,pad=0.05",
                             edgecolor=colors['client'],
                             facecolor='white',
                             linewidth=1.5)
    ax.add_patch(comp_box)
    ax.text(x, y, text, ha='center', va='center', fontsize=10, color=colors['text'])

# Layer 2: ASP.NET Core Web API
api_box = FancyBboxPatch((1, 4.2), 8, 2.2,
                         boxstyle="round,pad=0.1",
                         edgecolor=colors['api'],
                         facecolor=colors['api'],
                         linewidth=2, alpha=0.2)
ax.add_patch(api_box)

ax.text(5, 6.7, 'ASP.NET Core Web API (Azure App Service)', 
        ha='center', va='center', fontsize=16, fontweight='bold', color=colors['text'])

# API components
api_components = [
    ('Controllers\n(Terms, Courses,\nAssessments, Grades,\nIncome, Expenses,\nReports)', 2.5, 5.4),
    ('Models\n(Term, Course,\nAssessment, Grade,\nIncome, Expense)', 5, 5.4),
    ('DbContext\n(Entity Framework Core)', 7.5, 5.4)
]

for text, x, y in api_components:
    comp_box = FancyBboxPatch((x-0.8, y-0.4), 1.6, 0.8,
                             boxstyle="round,pad=0.05",
                             edgecolor=colors['api'],
                             facecolor='white',
                             linewidth=1.5)
    ax.add_patch(comp_box)
    ax.text(x, y, text, ha='center', va='center', fontsize=9, color=colors['text'])

# Layer 3: Azure SQL Database
db_box = FancyBboxPatch((1, 1.2), 8, 2.2,
                       boxstyle="round,pad=0.1",
                       edgecolor=colors['database'],
                       facecolor=colors['database'],
                       linewidth=2, alpha=0.2)
ax.add_patch(db_box)

ax.text(5, 3.7, 'Azure SQL Database', 
        ha='center', va='center', fontsize=16, fontweight='bold', color=colors['text'])

# Database tables
db_components = [
    ('Tables:\nTerms, Courses,\nAssessments,\nGrades, Income,\nExpenses', 5, 2.5)
]

for text, x, y in db_components:
    comp_box = FancyBboxPatch((x-1.2, y-0.5), 2.4, 1.0,
                             boxstyle="round,pad=0.05",
                             edgecolor=colors['database'],
                             facecolor='white',
                             linewidth=1.5)
    ax.add_patch(comp_box)
    ax.text(x, y, text, ha='center', va='center', fontsize=10, color=colors['text'])

# Connection arrows
# MAUI to API
arrow1 = FancyArrowPatch((5, 7), (5, 6.4),
                        arrowstyle='->', mutation_scale=20,
                        linewidth=2.5, color=colors['connection'],
                        zorder=3)
ax.add_patch(arrow1)
ax.text(5.5, 6.7, 'HTTPS/JSON\nRESTful API', 
        ha='left', va='center', fontsize=11, fontweight='bold', 
        color=colors['connection'], bbox=dict(boxstyle='round,pad=0.3', 
        facecolor='white', edgecolor=colors['connection'], linewidth=1.5))

# API to Database
arrow2 = FancyArrowPatch((5, 4.2), (5, 3.3),
                        arrowstyle='->', mutation_scale=20,
                        linewidth=2.5, color=colors['connection'],
                        zorder=3)
ax.add_patch(arrow2)
ax.text(5.5, 3.75, 'Entity Framework\nCore ORM', 
        ha='left', va='center', fontsize=11, fontweight='bold', 
        color=colors['connection'], bbox=dict(boxstyle='round,pad=0.3', 
        facecolor='white', edgecolor=colors['connection'], linewidth=1.5))

# Title
ax.text(5, 9.8, 'Student Progress Tracker - Three-Tier Architecture', 
        ha='center', va='top', fontsize=18, fontweight='bold', color=colors['text'])

plt.tight_layout()
plt.savefig('architecture_diagram.png', dpi=300, bbox_inches='tight', 
            facecolor='white', edgecolor='none', format='png')
print("Architecture diagram saved as architecture_diagram.png")

