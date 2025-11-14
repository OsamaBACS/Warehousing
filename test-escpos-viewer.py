#!/usr/bin/env python3
"""
ESC/POS Command Viewer and Validator
This script helps you visualize and validate ESC/POS commands without a physical printer.
"""

import sys
import os
import argparse
from pathlib import Path

# ESC/POS command constants
ESC = 0x1B
GS = 0x1D
LF = 0x0A
CR = 0x0D
HT = 0x09

def decode_escpos_commands(data):
    """Decode ESC/POS commands and return a human-readable representation"""
    i = 0
    commands = []
    text_buffer = []
    
    while i < len(data):
        byte = data[i]
        
        # ESC commands (0x1B)
        if byte == ESC:
            if text_buffer:
                commands.append(('TEXT', bytes(text_buffer).decode('utf-8', errors='ignore')))
                text_buffer = []
            
            if i + 1 < len(data):
                next_byte = data[i + 1]
                if next_byte == 0x40:  # ESC @ - Initialize printer
                    commands.append(('INIT', 'Initialize printer'))
                    i += 2
                    continue
                elif next_byte == 0x61:  # ESC a - Select justification
                    if i + 2 < len(data):
                        align = data[i + 2]
                        align_text = {0: 'Left', 1: 'Center', 2: 'Right'}.get(align, f'Unknown({align})')
                        commands.append(('ALIGN', align_text))
                        i += 3
                        continue
                elif next_byte == 0x64:  # ESC d - Print and feed n lines
                    if i + 2 < len(data):
                        lines = data[i + 2]
                        commands.append(('FEED', f'{lines} lines'))
                        i += 3
                        continue
                elif next_byte == 0x45:  # ESC E - Bold
                    if i + 2 < len(data):
                        bold = data[i + 2]
                        commands.append(('BOLD', 'ON' if bold == 1 else 'OFF'))
                        i += 3
                        continue
                elif next_byte == 0x4D:  # ESC M - Select character font
                    if i + 2 < len(data):
                        font = data[i + 2]
                        font_text = {0: 'Font A (12x24)', 1: 'Font B (9x17)', 48: 'Font A', 49: 'Font B'}.get(font, f'Font({font})')
                        commands.append(('FONT', font_text))
                        i += 3
                        continue
                elif next_byte == 0x64:  # ESC d - Feed lines
                    if i + 2 < len(data):
                        lines = data[i + 2]
                        commands.append(('FEED', f'{lines} lines'))
                        i += 3
                        continue
                else:
                    # Unknown ESC command
                    hex_str = ' '.join(f'{b:02X}' for b in data[i:min(i+5, len(data))])
                    commands.append(('ESC_CMD', f'Unknown: {hex_str}'))
                    i += 2
                    continue
        
        # GS commands (0x1D)
        elif byte == GS:
            if text_buffer:
                commands.append(('TEXT', bytes(text_buffer).decode('utf-8', errors='ignore')))
                text_buffer = []
            
            if i + 1 < len(data):
                next_byte = data[i + 1]
                if next_byte == 0x21:  # GS ! - Select character size
                    if i + 2 < len(data):
                        size = data[i + 2]
                        width = (size & 0xF0) >> 4
                        height = size & 0x0F
                        commands.append(('SIZE', f'Width: {width+1}x, Height: {height+1}x'))
                        i += 3
                        continue
                elif next_byte == 0x48:  # GS H - Select HRI barcode position
                    if i + 2 < len(data):
                        pos = data[i + 2]
                        pos_text = {0: 'None', 1: 'Above', 2: 'Below', 3: 'Above and Below'}.get(pos, f'Unknown({pos})')
                        commands.append(('BARCODE_POS', pos_text))
                        i += 3
                        continue
                elif next_byte == 0x68:  # GS h - Set barcode height
                    if i + 2 < len(data):
                        height = data[i + 2]
                        commands.append(('BARCODE_HEIGHT', f'{height} dots'))
                        i += 3
                        continue
                elif next_byte == 0x77:  # GS w - Set barcode width
                    if i + 2 < len(data):
                        width = data[i + 2]
                        commands.append(('BARCODE_WIDTH', f'{width}'))
                        i += 3
                        continue
                elif next_byte == 0x56:  # GS V - Cut paper
                    if i + 2 < len(data):
                        cut_mode = data[i + 2]
                        cut_text = {0: 'Full cut', 1: 'Partial cut', 48: 'Full cut', 49: 'Partial cut'}.get(cut_mode, f'Unknown({cut_mode})')
                        commands.append(('CUT', cut_text))
                        i += 3
                        continue
                else:
                    hex_str = ' '.join(f'{b:02X}' for b in data[i:min(i+5, len(data))])
                    commands.append(('GS_CMD', f'Unknown: {hex_str}'))
                    i += 2
                    continue
        
        # Line feed
        elif byte == LF or byte == CR:
            if text_buffer:
                commands.append(('TEXT', bytes(text_buffer).decode('utf-8', errors='ignore')))
                text_buffer = []
            commands.append(('NEWLINE', ''))
            i += 1
            continue
        
        # Horizontal tab
        elif byte == HT:
            if text_buffer:
                commands.append(('TEXT', bytes(text_buffer).decode('utf-8', errors='ignore')))
                text_buffer = []
            commands.append(('TAB', ''))
            i += 1
            continue
        
        # Regular text
        else:
            text_buffer.append(byte)
            i += 1
    
    # Flush remaining text
    if text_buffer:
        commands.append(('TEXT', bytes(text_buffer).decode('utf-8', errors='ignore')))
    
    return commands

def print_analysis(data, file_path):
    """Print a detailed analysis of the ESC/POS file"""
    print(f"\n{'='*80}")
    print(f"ESC/POS File Analysis: {file_path}")
    print(f"{'='*80}")
    print(f"File size: {len(data)} bytes")
    print(f"\nCommand Breakdown:\n")
    
    commands = decode_escpos_commands(data)
    
    # Group and count commands
    command_counts = {}
    for cmd_type, cmd_value in commands:
        if cmd_type not in command_counts:
            command_counts[cmd_type] = 0
        command_counts[cmd_type] += 1
    
    print("Command Statistics:")
    for cmd_type, count in sorted(command_counts.items()):
        print(f"  {cmd_type}: {count}")
    
    print(f"\n{'='*80}")
    print("Detailed Command Sequence:\n")
    
    line_num = 1
    current_line = []
    
    for cmd_type, cmd_value in commands:
        if cmd_type == 'TEXT':
            current_line.append(cmd_value)
        elif cmd_type == 'NEWLINE':
            if current_line:
                print(f"{line_num:4d} | {''.join(current_line)}")
                line_num += 1
                current_line = []
            else:
                print(f"{line_num:4d} | [Empty line]")
                line_num += 1
        elif cmd_type in ['INIT', 'ALIGN', 'BOLD', 'FONT', 'SIZE', 'FEED', 'CUT', 'TAB']:
            if current_line:
                print(f"{line_num:4d} | {''.join(current_line)}")
                line_num += 1
                current_line = []
            print(f"{line_num:4d} | [{cmd_type}] {cmd_value}")
            line_num += 1
        else:
            if current_line:
                print(f"{line_num:4d} | {''.join(current_line)}")
                line_num += 1
                current_line = []
            print(f"{line_num:4d} | [{cmd_type}] {cmd_value}")
            line_num += 1
    
    # Print remaining line
    if current_line:
        print(f"{line_num:4d} | {''.join(current_line)}")
    
    print(f"\n{'='*80}")
    print("Hex Dump (first 256 bytes):\n")
    
    # Print hex dump
    for i in range(0, min(256, len(data)), 16):
        hex_part = ' '.join(f'{b:02X}' for b in data[i:i+16])
        ascii_part = ''.join(chr(b) if 32 <= b < 127 else '.' for b in data[i:i+16])
        print(f"{i:04X}  {hex_part:<48}  {ascii_part}")

def main():
    parser = argparse.ArgumentParser(description='View and validate ESC/POS command files')
    parser.add_argument('file', help='Path to .escpos file')
    parser.add_argument('--hex', action='store_true', help='Show full hex dump')
    
    args = parser.parse_args()
    
    file_path = Path(args.file)
    if not file_path.exists():
        print(f"Error: File not found: {file_path}")
        sys.exit(1)
    
    with open(file_path, 'rb') as f:
        data = f.read()
    
    print_analysis(data, file_path)
    
    if args.hex:
        print(f"\n{'='*80}")
        print("Full Hex Dump:\n")
        for i in range(0, len(data), 16):
            hex_part = ' '.join(f'{b:02X}' for b in data[i:i+16])
            ascii_part = ''.join(chr(b) if 32 <= b < 127 else '.' for b in data[i:i+16])
            print(f"{i:04X}  {hex_part:<48}  {ascii_part}")

if __name__ == '__main__':
    main()

