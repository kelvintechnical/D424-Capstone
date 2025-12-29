"""
Generate GPA Calculation Data Flow Diagram (Sequence Diagram)
"""
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Rectangle
import numpy as np

# Set up the figure
fig, ax = plt.subplots(figsize=(12, 7.2), dpi=150)  # 1800x1080 at 150 DPI
ax.set_xlim(0, 10)
ax.set_ylim(0, 10)
ax.axis('off')

# Color scheme
colors = {
    'user': '#3498DB',
    'maui': '#9B59B6',
    'api': '#E74C3C',
    'db': '#27AE60',
    'text': '#2C3E50',
    'line': '#7F8C8D'
}

# Actors/Lifelines
actors = [
    ('User', 1.5, 9.5),
    ('MAUI App\n(GPAViewModel)', 3.5, 9.5),
    ('ApiService', 5.5, 9.5),
    ('GradesController', 7.5, 9.5),
    ('Azure SQL\nDatabase', 9.5, 9.5)
]

# Draw lifelines (vertical dashed lines)
lifeline_y_start = 9
lifeline_y_end = 0.5

for i, (name, x, y) in enumerate(actors):
    # Actor box
    box = FancyBboxPatch((x-0.6, y-0.3), 1.2, 0.6,
                         boxstyle="round,pad=0.05",
                         edgecolor=colors['text'],
                         facecolor='white',
                         linewidth=2)
    ax.add_patch(box)
    ax.text(x, y, name, ha='center', va='center', fontsize=9, 
            fontweight='bold', color=colors['text'])
    
    # Lifeline
    ax.plot([x, x], [lifeline_y_start, lifeline_y_end], 
            '--', color=colors['line'], linewidth=1.5, alpha=0.5)

# Messages (top to bottom)
y_positions = [8.5, 7.5, 6.5, 5.5, 4.5, 3.5, 2.5, 1.5, 0.8]

messages = [
    # (from_x, to_x, y, label, arrow_direction)
    (1.5, 3.5, y_positions[0], '1. User taps\n"View GPA"', '->'),
    (3.5, 5.5, y_positions[1], '2. GetGpaAsync(termId)', '->'),
    (5.5, 7.5, y_positions[2], '3. GET /api/reports/gpa/{termId}', '->'),
    (7.5, 9.5, y_positions[3], '4. Query via EF Core', '->'),
    (9.5, 7.5, y_positions[4], '5. Return courses with\ngrades & credit hours', '<-'),
    (7.5, 7.5, y_positions[5], '6. Calculate weighted GPA\nΣ(grade × credits) / Σ(credits)', 'self'),
    (7.5, 5.5, y_positions[6], '7. JSON response\nwith GPA data', '<-'),
    (5.5, 3.5, y_positions[7], '8. Return GPA data', '<-'),
    (3.5, 1.5, y_positions[8], '9. Display calculated GPA', '<-'),
]

for from_x, to_x, y, label, direction in messages:
    if direction == '->':
        arrow = FancyArrowPatch((from_x, y), (to_x, y),
                               arrowstyle='->', mutation_scale=15,
                               linewidth=2, color=colors['text'],
                               zorder=3)
        ax.add_patch(arrow)
        # Label above arrow
        ax.text((from_x + to_x) / 2, y + 0.15, label, 
               ha='center', va='bottom', fontsize=8,
               bbox=dict(boxstyle='round,pad=0.3', facecolor='white', 
                        edgecolor=colors['text'], linewidth=1, alpha=0.9))
    elif direction == '<-':
        arrow = FancyArrowPatch((to_x, y), (from_x, y),
                               arrowstyle='->', mutation_scale=15,
                               linewidth=2, color=colors['text'],
                               zorder=3)
        ax.add_patch(arrow)
        # Label above arrow
        ax.text((from_x + to_x) / 2, y + 0.15, label, 
               ha='center', va='bottom', fontsize=8,
               bbox=dict(boxstyle='round,pad=0.3', facecolor='white', 
                        edgecolor=colors['text'], linewidth=1, alpha=0.9))
    elif direction == 'self':
        # Self-call (loop)
        arc = mpatches.Arc((from_x, y), 0.8, 0.4, angle=0, 
                          theta1=0, theta2=180, linewidth=2, color=colors['text'])
        ax.add_patch(arc)
        arrow_head = FancyArrowPatch((from_x-0.4, y), (from_x-0.35, y-0.05),
                                    arrowstyle='->', mutation_scale=10,
                                    linewidth=2, color=colors['text'])
        ax.add_patch(arrow_head)
        ax.text(from_x + 0.5, y + 0.15, label, 
               ha='left', va='bottom', fontsize=8,
               bbox=dict(boxstyle='round,pad=0.3', facecolor='white', 
                        edgecolor=colors['text'], linewidth=1, alpha=0.9))

# Title
ax.text(5, 9.8, 'GPA Calculation - Data Flow Sequence Diagram', 
        ha='center', va='top', fontsize=16, fontweight='bold', color=colors['text'])

plt.tight_layout()
plt.savefig('gpa_calculation_flow.png', dpi=300, bbox_inches='tight', 
            facecolor='white', edgecolor='none', format='png')
print("GPA calculation flow diagram saved as gpa_calculation_flow.png")

