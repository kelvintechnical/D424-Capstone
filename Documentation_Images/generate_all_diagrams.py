"""
Master script to generate all documentation diagrams
"""
import subprocess
import sys
import os

scripts = [
    'generate_architecture_diagram.py',
    'generate_gpa_flow.py',
    'generate_erd.py',
    'generate_mvvm_diagram.py',
    'generate_csv_export_flow.py'
]

print("=" * 60)
print("Generating Documentation Images for Student Progress Tracker")
print("=" * 60)
print()

for script in scripts:
    if os.path.exists(script):
        print(f"Running {script}...")
        try:
            result = subprocess.run([sys.executable, script], 
                                  capture_output=True, text=True, check=True)
            print(result.stdout)
        except subprocess.CalledProcessError as e:
            print(f"Error running {script}:")
            print(e.stderr)
            sys.exit(1)
    else:
        print(f"Warning: {script} not found!")

print()
print("=" * 60)
print("All diagrams generated successfully!")
print("=" * 60)
print()
print("Generated files:")
for script in scripts:
    output_file = script.replace('generate_', '').replace('.py', '.png')
    if os.path.exists(output_file):
        size = os.path.getsize(output_file)
        print(f"  [OK] {output_file} ({size:,} bytes)")

