# Testing POS Printer Configuration Without a Physical Printer

This guide explains how to test POS printer configurations on Linux Mint without a physical printer.

## Method 1: Using the Test Endpoint (Recommended)

The API includes a test endpoint that saves ESC/POS commands to files for inspection.

### Step 1: Use the Test Endpoint

When you print something from the application, the ESC/POS commands are automatically saved to:
- `wwwroot/test-prints/[filename].escpos` - Binary ESC/POS file
- `wwwroot/test-prints/[filename].hex` - Hex dump for inspection

Or you can call the test endpoint directly:

```bash
curl -X POST http://localhost:5036/api/Print/test-escpos \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "title": "Test Receipt",
    "htmlContent": "<html><body><h1>Test Receipt</h1><p>Item 1: $10.00</p></body></html>",
    "type": "order"
  }'
```

### Step 2: View the ESC/POS Commands

Use the Python viewer script to analyze the generated ESC/POS file:

```bash
# Make the script executable
chmod +x test-escpos-viewer.py

# View an ESC/POS file
python3 test-escpos-viewer.py wwwroot/test-prints/Test_Receipt_20241110_120000.escpos

# View with full hex dump
python3 test-escpos-viewer.py wwwroot/test-prints/Test_Receipt_20241110_120000.escpos --hex
```

The script will show:
- Command statistics
- Human-readable command sequence
- Text content that will be printed
- Hex dump for technical inspection

## Method 2: Set Up a CUPS Raw Queue (Advanced)

You can set up a CUPS printer queue that accepts raw ESC/POS commands and saves them to a file.

### Step 1: Install CUPS (if not already installed)

```bash
sudo apt-get update
sudo apt-get install cups cups-client
```

### Step 2: Create a Raw Printer Queue

```bash
# Create a directory for print jobs
mkdir -p ~/test-prints

# Add a raw printer queue
lpadmin -p TestPOS -E -v file:///home/YOUR_USERNAME/test-prints/pos-output.raw -i raw

# Set as default (optional)
lpoptions -d TestPOS
```

### Step 3: Test Printing

```bash
# Print an ESC/POS file to the test queue
lp -d TestPOS your-file.escpos

# The output will be saved to ~/test-prints/pos-output.raw
```

## Method 3: Use a Web-Based ESC/POS Viewer

You can use online tools to visualize ESC/POS commands:

1. **ESC/POS Web Viewer**: Upload your `.escpos` file to an online viewer
2. **Hex Editor**: Use a hex editor like `hexdump` or `xxd` to inspect the file:

```bash
# View hex dump
hexdump -C wwwroot/test-prints/Test_Receipt.escpos | less

# Or use xxd
xxd wwwroot/test-prints/Test_Receipt.escpos | less
```

## Method 4: Simulate with a Virtual Serial Port

For more advanced testing, you can simulate a serial/USB connection:

### Install socat (for virtual serial ports)

```bash
sudo apt-get install socat
```

### Create a virtual serial port pair

```bash
# Terminal 1: Create virtual serial ports
socat -d -d pty,raw,echo=0 pty,raw,echo=0

# This will output something like:
# 2024/11/10 12:00:00 socat[12345] N PTY is /dev/pts/2
# 2024/11/10 12:00:00 socat[12346] N PTY is /dev/pts/3

# Terminal 2: Monitor one end
cat /dev/pts/2 > test-output.escpos

# Terminal 3: Send data to the other end
cat your-file.escpos > /dev/pts/3
```

## Common ESC/POS Commands Reference

When viewing the output, you'll see commands like:

- `[INIT]` - Initialize printer
- `[ALIGN] Left/Center/Right` - Text alignment
- `[BOLD] ON/OFF` - Bold text
- `[FONT] Font A/B` - Font selection
- `[SIZE]` - Character size
- `[FEED]` - Feed paper lines
- `[CUT]` - Cut paper
- `[TEXT]` - Regular text content

## Troubleshooting

### Issue: Test endpoint returns 404

**Solution**: Make sure the API is running and the endpoint is accessible. Check the API logs.

### Issue: ESC/POS file is empty

**Solution**: 
1. Verify the printer configuration has `UseEscPos = true`
2. Check that `PrinterType` is set to "POS" or "Thermal"
3. Review the API logs for errors

### Issue: Python script shows encoding errors

**Solution**: The script uses UTF-8 with error handling. Some ESC/POS commands may contain binary data that can't be decoded as text, which is normal.

## Next Steps

1. Generate a test print from your application
2. Use the Python viewer to inspect the ESC/POS commands
3. Verify the commands match your printer configuration settings
4. Adjust the printer configuration in the admin panel as needed
5. Re-test until the output looks correct

## Additional Resources

- [ESC/POS Command Reference](https://reference.epson-biz.com/modules/ref_escpos/)
- [CUPS Documentation](https://www.cups.org/doc/man-lpadmin.html)
- [Python ESC/POS Library](https://github.com/python-escpos/python-escpos) (for creating test files)

