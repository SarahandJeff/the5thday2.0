#!/usr/bin/env python3
"""
Remove hireable fox (npcAnimalFox) from XNPCCore entitygroups.xml

This script removes all instances of 'npcAnimalFox' from entity groups
while preserving the wild fox (animalFox) entries.
"""

import re
import shutil
from pathlib import Path
from datetime import datetime


def remove_hireable_fox(xml_path: Path, dry_run: bool = False) -> tuple[int, list[str]]:
    """
    Remove npcAnimalFox entries from entitygroups.xml
    
    Args:
        xml_path: Path to entitygroups.xml
        dry_run: If True, don't write changes, just report what would be removed
        
    Returns:
        Tuple of (count of removals, list of removed lines)
    """
    if not xml_path.exists():
        raise FileNotFoundError(f"File not found: {xml_path}")
    
    # Read the file
    content = xml_path.read_text(encoding='utf-8')
    lines = content.splitlines(keepends=True)
    
    # Pattern to match npcAnimalFox entries (various formats)
    # Matches: "npcAnimalFox, .3" or "npcAnimalFox, 0.05" etc.
    pattern = re.compile(r'^\s*npcAnimalFox\s*,\s*[\d.]+\s*$', re.MULTILINE)
    
    removed_lines = []
    new_lines = []
    
    for line in lines:
        # Check if this line contains npcAnimalFox as an entity entry
        stripped = line.strip()
        if re.match(r'^npcAnimalFox\s*,', stripped):
            removed_lines.append(line.rstrip())
            continue
        new_lines.append(line)
    
    if not dry_run and removed_lines:
        # Create backup
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_path = xml_path.with_suffix(f'.xml.backup_npcfox_{timestamp}')
        shutil.copy2(xml_path, backup_path)
        print(f"Created backup: {backup_path.name}")
        
        # Write modified content
        xml_path.write_text(''.join(new_lines), encoding='utf-8')
        print(f"Modified: {xml_path.name}")
    
    return len(removed_lines), removed_lines


def main():
    import sys

    script_dir = Path(__file__).parent
    xml_path = script_dir / "Config" / "entitygroups.xml"

    # Check for --yes flag to skip confirmation
    auto_confirm = '--yes' in sys.argv or '-y' in sys.argv

    print("=" * 60)
    print("Remove Hireable Fox (npcAnimalFox) from Entity Groups")
    print("=" * 60)
    print(f"\nTarget file: {xml_path}")

    if not xml_path.exists():
        print(f"\nERROR: File not found: {xml_path}")
        return 1

    # First do a dry run to show what will be removed
    print("\n--- DRY RUN: Lines that will be removed ---")
    count, removed = remove_hireable_fox(xml_path, dry_run=True)

    if count == 0:
        print("No npcAnimalFox entries found.")
        return 0

    print(f"\nFound {count} npcAnimalFox entries:")
    for i, line in enumerate(removed, 1):
        print(f"  {i}. {line.strip()}")

    # Ask for confirmation (unless --yes flag)
    print(f"\nThis will remove {count} lines from entitygroups.xml")

    if not auto_confirm:
        response = input("Proceed? (y/n): ").strip().lower()
        if response != 'y':
            print("Aborted.")
            return 0
    else:
        print("Auto-confirmed with --yes flag")

    # Do the actual removal
    count, removed = remove_hireable_fox(xml_path, dry_run=False)
    print(f"\nSuccessfully removed {count} npcAnimalFox entries.")
    print("\nNote: animalFox (wild fox) entries were preserved.")

    return 0


if __name__ == "__main__":
    exit(main())

