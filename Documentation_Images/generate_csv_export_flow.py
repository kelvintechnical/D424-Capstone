"""
Generate CSV Export Flow Diagram (Flowchart)
"""
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Rectangle, Polygon
import numpy as np

# Set up the figure (vertical orientation)
fig, ax = plt.subplots(figsize=(10, 12.5), dpi=150)  # 1500x1875 at 150 DPI
ax.set_xlim(0, 10)
ax.set_ylim(0, 12.5)
ax.axis('off')

# Color scheme
colors = {
    'start_end': '#27AE60',   # Green
    'process': '#3498DB',     # Blue
    'decision': '#E74C3C',    # Red
    'text': '#2C3E50',
    'arrow': '#7F8C8D',
    'error': '#E74C3C'
}

def draw_process(ax, x, y, text, width=2, height=0.6):
    """Draw a process box (rectangle)"""
    box = FancyBboxPatch((x - width/2, y - height/2), width, height,
                         boxstyle="round,pad=0.05",
                         edgecolor=colors['process'],
                         facecolor='white',
                         linewidth=2)
    ax.add_patch(box)
    ax.text(x, y, text, ha='center', va='center', 
           fontsize=9, color=colors['text'], fontweight='bold')
    return x, y

def draw_decision(ax, x, y, text, width=1.8, height=1.2):
    """Draw a decision diamond"""
    # Create diamond shape
    diamond = Polygon([(x, y + height/2), (x + width/2, y), 
                      (x, y - height/2), (x - width/2, y)],
                     edgecolor=colors['decision'],
                     facecolor='white',
                     linewidth=2)
    ax.add_patch(diamond)
    ax.text(x, y, text, ha='center', va='center', 
           fontsize=8.5, color=colors['text'], fontweight='bold')
    return x, y

def draw_start_end(ax, x, y, text, width=2.2, height=0.6):
    """Draw start/end oval"""
    # Create rounded rectangle (oval-like)
    box = FancyBboxPatch((x - width/2, y - height/2), width, height,
                         boxstyle="round,pad=0.1",
                         edgecolor=colors['start_end'],
                         facecolor=colors['start_end'],
                         linewidth=2, alpha=0.3)
    ax.add_patch(box)
    ax.text(x, y, text, ha='center', va='center', 
           fontsize=9, color=colors['text'], fontweight='bold')
    return x, y

def draw_arrow(ax, x1, y1, x2, y2, label=None, label_pos='right'):
    """Draw an arrow with optional label"""
    arrow = FancyArrowPatch((x1, y1), (x2, y2),
                           arrowstyle='->', mutation_scale=20,
                           linewidth=2, color=colors['arrow'],
                           zorder=3)
    ax.add_patch(arrow)
    if label:
        mid_x = (x1 + x2) / 2
        mid_y = (y1 + y2) / 2
        if label_pos == 'right':
            ax.text(mid_x + 0.3, mid_y, label, ha='left', va='center', 
                   fontsize=8, color=colors['text'],
                   bbox=dict(boxstyle='round,pad=0.2', facecolor='white', 
                            edgecolor=colors['arrow'], linewidth=1))
        else:
            ax.text(mid_x - 0.3, mid_y, label, ha='right', va='center', 
                   fontsize=8, color=colors['text'],
                   bbox=dict(boxstyle='round,pad=0.2', facecolor='white', 
                            edgecolor=colors['arrow'], linewidth=1))

# Flowchart elements (top to bottom)
y_start = 11.5
y_step = 0.9

# 1. Start
start_x, start_y = 5, y_start
draw_start_end(ax, start_x, start_y, 'START\nUser taps\n"Export Transcript"')

# 2. Check data exists
check_x, check_y = 5, y_start - y_step * 1.5
draw_decision(ax, check_x, check_y, 'Data\nexists?')

# 3. Show error (if no)
error_x, error_y = 2.5, check_y - y_step * 1.5
draw_process(ax, error_x, error_y, 'Show error\ndialog', 1.8, 0.6)

# 4. End (error path)
end_error_x, end_error_y = 2.5, error_y - y_step
draw_start_end(ax, end_error_x, end_error_y, 'END', 1.5, 0.5)

# 5. Call API service (if yes)
api_x, api_y = 5, check_y - y_step * 1.5
draw_process(ax, api_x, api_y, 'ViewModel calls\nApiService.ExportTranscriptAsync()', 2.8, 0.6)

# 6. Send GET request
get_x, get_y = 5, api_y - y_step
draw_process(ax, get_x, get_y, 'ApiService sends\nGET /api/reports/transcript/csv', 2.8, 0.6)

# 7. API queries database
query_x, query_y = 5, get_y - y_step
draw_process(ax, query_x, query_y, 'API queries database\nand formats CSV', 2.8, 0.6)

# 8. Return byte array
return_x, return_y = 5, query_y - y_step
draw_process(ax, return_x, return_y, 'API returns\nbyte array (CSV data)', 2.8, 0.6)

# 9. Save to cache
save_x, save_y = 5, return_y - y_step
draw_process(ax, save_x, save_y, 'MAUI app saves to\nFileSystem.CacheDirectory', 2.8, 0.6)

# 10. Generate filename
filename_x, filename_y = 5, save_y - y_step
draw_process(ax, filename_x, filename_y, 'Generate\ntimestamped filename', 2.8, 0.6)

# 11. Open share dialog
share_x, share_y = 5, filename_y - y_step
draw_process(ax, share_x, share_y, 'Open native\nShare dialog', 2.8, 0.6)

# 12. End (success)
end_x, end_y = 5, share_y - y_step
draw_start_end(ax, end_x, end_y, 'END\nUser can email/\nsave/share CSV', 2.2, 0.6)

# Draw arrows
# Start to check
draw_arrow(ax, start_x, start_y - 0.3, check_x, check_y + 0.6)

# Check to error (No)
draw_arrow(ax, check_x - 0.9, check_y, error_x, error_y + 0.3, 'No', 'left')
ax.text(check_x - 1.2, check_y, 'No', ha='right', va='center', 
       fontsize=8, fontweight='bold', color=colors['error'])

# Error to end
draw_arrow(ax, error_x, error_y - 0.3, end_error_x, end_error_y + 0.25)

# Check to API (Yes)
draw_arrow(ax, check_x + 0.9, check_y, api_x, api_y + 0.3, 'Yes', 'right')
ax.text(check_x + 1.2, check_y, 'Yes', ha='left', va='center', 
       fontsize=8, fontweight='bold', color=colors['start_end'])

# API to GET
draw_arrow(ax, api_x, api_y - 0.3, get_x, get_y + 0.3)

# GET to query
draw_arrow(ax, get_x, get_y - 0.3, query_x, query_y + 0.3)

# Query to return
draw_arrow(ax, query_x, query_y - 0.3, return_x, return_y + 0.3)

# Return to save
draw_arrow(ax, return_x, return_y - 0.3, save_x, save_y + 0.3)

# Save to filename
draw_arrow(ax, save_x, save_y - 0.3, filename_x, filename_y + 0.3)

# Filename to share
draw_arrow(ax, filename_x, filename_y - 0.3, share_x, share_y + 0.3)

# Share to end
draw_arrow(ax, share_x, share_y - 0.3, end_x, end_y + 0.3)

# Title
ax.text(5, 12.2, 'CSV Export Flow - Transcript Export Process', 
        ha='center', va='top', fontsize=16, fontweight='bold', color=colors['text'])

# Legend
legend_x, legend_y = 7.5, 10
legend_items = [
    ('Start/End', colors['start_end']),
    ('Process', colors['process']),
    ('Decision', colors['decision'])
]

for i, (text, color) in enumerate(legend_items):
    if i == 0:  # Start/End
        box = FancyBboxPatch((legend_x, legend_y - i*0.4), 0.3, 0.15,
                            boxstyle="round,pad=0.05",
                            facecolor=color, edgecolor=colors['text'], 
                            linewidth=1, alpha=0.3)
    elif i == 1:  # Process
        box = FancyBboxPatch((legend_x, legend_y - i*0.4), 0.3, 0.15,
                            boxstyle="round,pad=0.05",
                            facecolor='white', edgecolor=color, linewidth=1.5)
    else:  # Decision
        diamond = Polygon([(legend_x + 0.15, legend_y - i*0.4 + 0.075),
                          (legend_x + 0.3, legend_y - i*0.4),
                          (legend_x + 0.15, legend_y - i*0.4 - 0.075),
                          (legend_x, legend_y - i*0.4)],
                         facecolor='white', edgecolor=color, linewidth=1.5)
        ax.add_patch(diamond)
    
    if i != 2:
        ax.add_patch(box)
    ax.text(legend_x + 0.4, legend_y - i*0.4, text,
           ha='left', va='center', fontsize=8, color=colors['text'])

plt.tight_layout()
plt.savefig('csv_export_flow.png', dpi=300, bbox_inches='tight', 
            facecolor='white', edgecolor='none', format='png')
print("CSV export flow diagram saved as csv_export_flow.png")

