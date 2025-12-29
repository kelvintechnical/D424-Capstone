"""
Generate MVVM Pattern Diagram for MAUI App
"""
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Rectangle
import numpy as np

# Set up the figure
fig, ax = plt.subplots(figsize=(12.5, 8.75), dpi=150)  # 1875x1312 at 150 DPI
ax.set_xlim(0, 10)
ax.set_ylim(0, 10)
ax.axis('off')

# Color scheme
colors = {
    'view': '#3498DB',      # Blue
    'viewmodel': '#9B59B6',  # Purple
    'model': '#E74C3C',     # Red
    'service': '#27AE60',   # Green
    'text': '#2C3E50',
    'arrow': '#7F8C8D',
    'binding': '#F39C12'    # Orange for bindings
}

# Layer 1: View Layer
view_box = FancyBboxPatch((0.5, 7.5), 9, 2,
                          boxstyle="round,pad=0.1",
                          edgecolor=colors['view'],
                          facecolor=colors['view'],
                          linewidth=2, alpha=0.15)
ax.add_patch(view_box)

ax.text(5, 9.2, 'View Layer (XAML Pages)', 
        ha='center', va='center', fontsize=14, fontweight='bold', color=colors['text'])

# View components
views = [
    ('GPAPage.xaml\n(binds to\nGPAViewModel)', 2, 8.2),
    ('TermsPage.xaml\n(binds to\nTermsViewModel)', 5, 8.2),
    ('CoursesPage.xaml\n(binds to\nCoursesViewModel)', 8, 8.2)
]

for text, x, y in views:
    view_comp = FancyBboxPatch((x-0.7, y-0.35), 1.4, 0.7,
                              boxstyle="round,pad=0.05",
                              edgecolor=colors['view'],
                              facecolor='white',
                              linewidth=1.5)
    ax.add_patch(view_comp)
    ax.text(x, y, text, ha='center', va='center', fontsize=9, color=colors['text'])

# Layer 2: ViewModel Layer
viewmodel_box = FancyBboxPatch((0.5, 4.5), 9, 2.5,
                               boxstyle="round,pad=0.1",
                               edgecolor=colors['viewmodel'],
                               facecolor=colors['viewmodel'],
                               linewidth=2, alpha=0.15)
ax.add_patch(viewmodel_box)

ax.text(5, 6.7, 'ViewModel Layer (Business Logic & Commands)', 
        ha='center', va='center', fontsize=14, fontweight='bold', color=colors['text'])

# ViewModel components
viewmodels = [
    ('GPAViewModel\n• ExportGpaReportCommand\n• ExportTranscriptCommand', 2, 5.6),
    ('TermsViewModel\n• LoadTermsCommand\n• AddTermCommand', 5, 5.6),
    ('CoursesViewModel\n• LoadCoursesCommand\n• DeleteCourseCommand', 8, 5.6)
]

for text, x, y in viewmodels:
    vm_comp = FancyBboxPatch((x-0.7, y-0.4), 1.4, 0.8,
                             boxstyle="round,pad=0.05",
                             edgecolor=colors['viewmodel'],
                             facecolor='white',
                             linewidth=1.5)
    ax.add_patch(vm_comp)
    ax.text(x, y, text, ha='center', va='center', fontsize=8.5, color=colors['text'])

# Layer 3: Model/Service Layer
model_box = FancyBboxPatch((0.5, 1.5), 9, 2.5,
                           boxstyle="round,pad=0.1",
                           edgecolor=colors['model'],
                           facecolor=colors['model'],
                           linewidth=2, alpha=0.15)
ax.add_patch(model_box)

ax.text(5, 3.7, 'Model/Service Layer', 
        ha='center', va='center', fontsize=14, fontweight='bold', color=colors['text'])

# Model/Service components
models = [
    ('ApiService\n(HTTP\nCommunication)', 2.5, 2.7),
    ('Models\n(Term, Course,\nAssessment, etc.)', 5, 2.7),
    ('Services\n(Data Access)', 7.5, 2.7)
]

for text, x, y in models:
    model_comp = FancyBboxPatch((x-0.7, y-0.35), 1.4, 0.7,
                                boxstyle="round,pad=0.05",
                                edgecolor=colors['model'],
                                facecolor='white',
                                linewidth=1.5)
    ax.add_patch(model_comp)
    ax.text(x, y, text, ha='center', va='center', fontsize=9, color=colors['text'])

# Data binding arrows (View <-> ViewModel)
binding_arrows = [
    (2, 7.5, 2, 6.2),  # GPAPage to GPAViewModel
    (5, 7.5, 5, 6.2),  # TermsPage to TermsViewModel
    (8, 7.5, 8, 6.2),  # CoursesPage to CoursesViewModel
]

for x1, y1, x2, y2 in binding_arrows:
    # Two-way binding (double arrow)
    # Up arrow
    arrow1 = FancyArrowPatch((x1, y1), (x2, y2),
                            arrowstyle='<->', mutation_scale=20,
                            linewidth=2.5, color=colors['binding'],
                            zorder=3, linestyle='-')
    ax.add_patch(arrow1)

# ViewModel to Service arrows
service_arrows = [
    (2, 4.5, 2.5, 3.2),  # GPAViewModel to ApiService
    (5, 4.5, 5, 3.2),    # TermsViewModel to Models
    (8, 4.5, 7.5, 3.2),  # CoursesViewModel to Services
]

for x1, y1, x2, y2 in service_arrows:
    arrow = FancyArrowPatch((x1, y1), (x2, y2),
                           arrowstyle='->', mutation_scale=20,
                           linewidth=2, color=colors['arrow'],
                           zorder=3)
    ax.add_patch(arrow)

# Labels
ax.text(2.5, 5.85, 'Two-Way\nData Binding', 
        ha='center', va='center', fontsize=8, fontstyle='italic',
        color=colors['binding'], rotation=90)

ax.text(5.5, 3.85, 'Calls', 
        ha='center', va='center', fontsize=9, fontweight='bold',
        color=colors['arrow'])

# Observable properties update flow
ax.text(8.5, 6.5, 'Observable\nProperties\nUpdate', 
        ha='left', va='center', fontsize=8, fontstyle='italic',
        color=colors['viewmodel'],
        bbox=dict(boxstyle='round,pad=0.3', facecolor='white', 
                 edgecolor=colors['viewmodel'], linewidth=1.5))

# Title
ax.text(5, 9.7, 'Student Progress Tracker - MVVM Pattern Architecture', 
        ha='center', va='top', fontsize=16, fontweight='bold', color=colors['text'])

# Legend
legend_x, legend_y = 0.7, 0.8
legend_items = [
    ('View Layer', colors['view']),
    ('ViewModel Layer', colors['viewmodel']),
    ('Model/Service Layer', colors['model']),
    ('Two-Way Data Binding', colors['binding']),
    ('Service Calls', colors['arrow'])
]

for i, (text, color) in enumerate(legend_items):
    rect = Rectangle((legend_x, legend_y - i*0.25), 0.2, 0.15,
                    facecolor=color, edgecolor=colors['text'], linewidth=1)
    ax.add_patch(rect)
    ax.text(legend_x + 0.3, legend_y - i*0.25 + 0.075, text,
           ha='left', va='center', fontsize=8, color=colors['text'])

plt.tight_layout()
plt.savefig('mvvm_pattern.png', dpi=300, bbox_inches='tight', 
            facecolor='white', edgecolor='none', format='png')
print("MVVM pattern diagram saved as mvvm_pattern.png")

