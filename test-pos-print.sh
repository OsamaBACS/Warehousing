#!/bin/bash
# Quick test script for POS printer configurations

API_URL="${API_URL:-http://localhost:5036}"
TEST_DIR="${TEST_DIR:-wwwroot/test-prints}"

echo "=========================================="
echo "POS Printer Configuration Test Script"
echo "=========================================="
echo ""

# Check if Python 3 is available
if ! command -v python3 &> /dev/null; then
    echo "Warning: python3 not found. Install it with: sudo apt-get install python3"
    echo ""
fi

# Create test-prints directory if it doesn't exist
mkdir -p "$TEST_DIR"

echo "1. Testing ESC/POS generation..."
echo "   API URL: $API_URL"
echo ""

# Example test request
cat > /tmp/test-print-request.json << 'EOF'
{
  "title": "Test Receipt",
  "htmlContent": "<html><body style='font-family: Arial;'><div style='text-align: center;'><h1>Test Receipt</h1><hr><p><strong>Item 1:</strong> Product Name</p><p>Price: $10.00</p><p>Quantity: 2</p><p>Total: $20.00</p><hr><p>Thank you!</p></div></body></html>",
  "type": "order"
}
EOF

echo "2. Sending test request to API..."
echo ""

# Note: You'll need to add authentication token
RESPONSE=$(curl -s -X POST "$API_URL/api/Print/test-escpos" \
  -H "Content-Type: application/json" \
  -d @/tmp/test-print-request.json)

if [ $? -eq 0 ]; then
    echo "✓ Request successful!"
    echo ""
    echo "Response:"
    echo "$RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$RESPONSE"
    echo ""
    
    # Extract file path from response (if available)
    FILE_PATH=$(echo "$RESPONSE" | grep -oP '"filePath":\s*"[^"]*"' | cut -d'"' -f4)
    
    if [ -n "$FILE_PATH" ] && [ -f "$FILE_PATH" ]; then
        echo "3. ESC/POS file created: $FILE_PATH"
        echo ""
        
        # Use the Python viewer if available
        if command -v python3 &> /dev/null && [ -f "test-escpos-viewer.py" ]; then
            echo "4. Analyzing ESC/POS file..."
            echo ""
            python3 test-escpos-viewer.py "$FILE_PATH"
        else
            echo "4. View file with:"
            echo "   python3 test-escpos-viewer.py $FILE_PATH"
            echo ""
            echo "   Or view hex dump:"
            echo "   hexdump -C $FILE_PATH | less"
        fi
    else
        echo "3. File path not found in response or file doesn't exist"
        echo "   Check the API response above for the file path"
    fi
else
    echo "✗ Request failed!"
    echo "   Make sure:"
    echo "   1. The API is running on $API_URL"
    echo "   2. You have proper authentication (add token to script)"
    echo "   3. Your printer configuration is set to POS/Thermal with ESC/POS enabled"
fi

echo ""
echo "=========================================="
echo "Test complete!"
echo "=========================================="

