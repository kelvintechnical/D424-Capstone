"""
Generate Entity Relationship Diagram (ERD) for Student Progress Tracker
CORRECTED VERSION - Includes all relationships and missing tables
"""
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch, Rectangle
import numpy as np

# Set up the figure - larger to fit all tables
fig, ax = plt.subplots(figsize=(20, 14), dpi=150)  # 3000x2100 at 150 DPI
ax.set_xlim(0, 16)
ax.set_ylim(0, 12)
ax.axis('off')

# Color scheme - darker line color for visibility
colors = {
    'table': '#3498DB',
    'pk': '#E74C3C',
    'fk': '#F39C12',
    'text': '#2C3E50',
    'line': '#34495E',  # Darker gray for better visibility
    'bg': '#FFFFFF'
}

def draw_table(ax, x, y, name, fields, pk_fields, fk_fields=None):
    """Draw a database table box"""
    if fk_fields is None:
        fk_fields = []
    
    # Calculate box size
    num_fields = len(fields)
    box_height = 0.4 + num_fields * 0.35
    box_width = 2.2
    
    # Table box
    table_box = FancyBboxPatch((x - box_width/2, y - box_height/2), 
                               box_width, box_height,
                               boxstyle="round,pad=0.05",
                               edgecolor=colors['table'],
                               facecolor='white',
                               linewidth=2)
    ax.add_patch(table_box)
    
    # Table name (header)
    header_box = Rectangle((x - box_width/2, y + box_height/2 - 0.4), 
                          box_width, 0.4,
                          facecolor=colors['table'],
                          edgecolor=colors['table'],
                          linewidth=2)
    ax.add_patch(header_box)
    ax.text(x, y + box_height/2 - 0.2, name, 
           ha='center', va='center', fontsize=11, 
           fontweight='bold', color='white')
    
    # Fields
    field_y = y + box_height/2 - 0.5
    for i, field in enumerate(fields):
        field_text = field
        if field in pk_fields:
            field_text = f"PK: {field}"
        elif field in fk_fields:
            field_text = f"FK: {field}"
        
        ax.text(x - box_width/2 + 0.1, field_y, field_text,
               ha='left', va='center', fontsize=8, color=colors['text'])
        field_y -= 0.35
    
    return x, y, box_width, box_height

# Draw tables with better positioning
# User table (AspNetUsers) - top center
user_x, user_y = 8, 10.5
user_fields = ['Id', 'Email', 'UserName', 'PasswordHash']
user_pk = ['Id']
draw_table(ax, user_x, user_y, 'AspNetUsers', user_fields, user_pk)

# Terms table
terms_x, terms_y = 2, 8
terms_fields = ['Id', 'UserId', 'Title', 'StartDate', 'EndDate']
terms_pk = ['Id']
terms_fk = ['UserId']
draw_table(ax, terms_x, terms_y, 'Terms', terms_fields, terms_pk, terms_fk)

# Courses table
courses_x, courses_y = 6, 8
courses_fields = ['Id', 'TermId', 'Title', 'InstructorName', 
                  'InstructorEmail', 'CreditHours', 'StartDate', 'EndDate', 'Status']
courses_pk = ['Id']
courses_fk = ['TermId']
draw_table(ax, courses_x, courses_y, 'Courses', courses_fields, courses_pk, courses_fk)

# Assessments table
assessments_x, assessments_y = 10, 8
assessments_fields = ['Id', 'CourseId', 'Name', 'Type', 'StartDate', 'DueDate']
assessments_pk = ['Id']
assessments_fk = ['CourseId']
draw_table(ax, assessments_x, assessments_y, 'Assessments', 
          assessments_fields, assessments_pk, assessments_fk)

# Grades table - CORRECTED: links to Course, not Assessment
grades_x, grades_y = 6, 5.5
grades_fields = ['Id', 'CourseId', 'LetterGrade', 'Percentage', 'CreditHours']
grades_pk = ['Id']
grades_fk = ['CourseId']
draw_table(ax, grades_x, grades_y, 'Grades', grades_fields, grades_pk, grades_fk)

# Income table
income_x, income_y = 2, 5.5
income_fields = ['Id', 'UserId', 'Source', 'Amount', 'Date']
income_pk = ['Id']
income_fk = ['UserId']
draw_table(ax, income_x, income_y, 'Income', income_fields, income_pk, income_fk)

# Categories table - ADDED
categories_x, categories_y = 10, 5.5
categories_fields = ['Id', 'UserId', 'Name', 'IsCustom']
categories_pk = ['Id']
categories_fk = ['UserId']
draw_table(ax, categories_x, categories_y, 'Categories', categories_fields, categories_pk, categories_fk)

# Expenses table
expenses_x, expenses_y = 6, 3
expenses_fields = ['Id', 'UserId', 'CategoryId', 'Description', 'Amount', 'Date']
expenses_pk = ['Id']
expenses_fk = ['UserId', 'CategoryId']
draw_table(ax, expenses_x, expenses_y, 'Expenses', expenses_fields, expenses_pk, expenses_fk)

# Draw relationships (crow's foot notation) - THICKER LINES
def draw_relationship(ax, from_x, from_y, to_x, to_y, label, side='right', linewidth=3):
    """Draw a relationship line with crow's foot notation"""
    # Main line - THICKER and DARKER
    ax.plot([from_x, to_x], [from_y, to_y], 
           color=colors['line'], linewidth=linewidth, zorder=1, alpha=0.8)
    
    # Crow's foot at "many" end
    if side == 'right':
        # Draw crow's foot at to_x, to_y
        foot_size = 0.2
        angle = np.arctan2(to_y - from_y, to_x - from_x)
        # Perpendicular direction
        perp_angle = angle + np.pi/2
        
        # Three lines for crow's foot
        end_x = to_x - 0.4 * np.cos(angle)
        end_y = to_y - 0.4 * np.sin(angle)
        
        # Left branch
        ax.plot([end_x, end_x - foot_size * np.cos(perp_angle)],
               [end_y, end_y - foot_size * np.sin(perp_angle)],
               color=colors['line'], linewidth=linewidth, zorder=2, alpha=0.8)
        # Right branch
        ax.plot([end_x, end_x + foot_size * np.cos(perp_angle)],
               [end_y, end_y + foot_size * np.sin(perp_angle)],
               color=colors['line'], linewidth=linewidth, zorder=2, alpha=0.8)
        # Center line
        ax.plot([end_x, to_x],
               [end_y, to_y],
               color=colors['line'], linewidth=linewidth, zorder=2, alpha=0.8)
        
        # Single line at "one" end
        start_x = from_x + 0.4 * np.cos(angle)
        start_y = from_y + 0.4 * np.sin(angle)
        ax.plot([from_x, start_x], [from_y, start_y],
               color=colors['line'], linewidth=linewidth, zorder=2, alpha=0.8)
    
    # Label
    mid_x = (from_x + to_x) / 2
    mid_y = (from_y + to_y) / 2
    ax.text(mid_x, mid_y, label, ha='center', va='center', fontsize=9,
           bbox=dict(boxstyle='round,pad=0.3', facecolor='white', 
                    edgecolor=colors['line'], linewidth=1.5))

# Relationships - ALL relationships included
# User (1) to Terms (many)
draw_relationship(ax, user_x - 0.3, user_y - 0.5, terms_x + 0.3, terms_y + 0.5, '1:M', 'right')

# Terms (1) to Courses (many)
draw_relationship(ax, terms_x + 1.1, terms_y, courses_x - 1.1, courses_y, '1:M', 'right')

# Courses (1) to Assessments (many)
draw_relationship(ax, courses_x + 1.1, courses_y, assessments_x - 1.1, assessments_y, '1:M', 'right')

# Courses (1) to Grades (many) - CORRECTED
draw_relationship(ax, courses_x, courses_y - 0.7, grades_x, grades_y + 0.7, '1:M', 'right')

# User (1) to Income (many)
draw_relationship(ax, user_x - 0.3, user_y - 0.5, income_x + 0.3, income_y + 0.5, '1:M', 'right')

# User (1) to Categories (many)
draw_relationship(ax, user_x + 0.3, user_y - 0.5, categories_x - 0.3, categories_y + 0.5, '1:M', 'right')

# User (1) to Expenses (many)
draw_relationship(ax, user_x, user_y - 0.7, expenses_x, expenses_y + 0.7, '1:M', 'right')

# Categories (1) to Expenses (many)
draw_relationship(ax, categories_x, categories_y - 0.7, expenses_x, expenses_y + 0.7, '1:M', 'right')

# Legend
legend_x, legend_y = 1, 2
legend_items = [
    ('PK: Primary Key', colors['pk']),
    ('FK: Foreign Key', colors['fk']),
    ('1:M One-to-Many Relationship', colors['line'])
]

for i, (text, color) in enumerate(legend_items):
    ax.text(legend_x, legend_y - i*0.3, text, ha='left', va='center', 
           fontsize=9, color=colors['text'])

# Title
ax.text(8, 11.5, 'Student Progress Tracker - Entity Relationship Diagram', 
        ha='center', va='top', fontsize=18, fontweight='bold', color=colors['text'])

plt.tight_layout()
plt.savefig('database_erd.png', dpi=300, bbox_inches='tight', 
            facecolor='white', edgecolor='none', format='png')
print("ERD diagram saved as database_erd.png")
