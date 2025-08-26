#!/bin/bash

# =================================================================================
# Script to fetch a remote SSL certificate and add it to the system's trust store.
# Must be run with sudo privileges to modify system certificate stores.
# =================================================================================

# --- Configuration ---
# The address of your LDAP server (Domain Controller).
SERVER_ADDRESS="dc-blazam-org.ether.net"
# The LDAPS port, typically 636.
SERVER_PORT="636"
# --- End Configuration ---

# Check for root/sudo privileges
if [ "$EUID" -ne 0 ]; then
  echo "!!! This script must be run as root or with sudo. !!!"
  exit 1
fi

# Check if openssl is installed
if ! command -v openssl &> /dev/null; then
    echo "!!! openssl could not be found. Please install it first. !!!"
    exit 1
fi

echo "--> Fetching the SSL certificate from $SERVER_ADDRESS:$SERVER_PORT..."

# Use openssl to connect and extract the server's certificate in PEM format.
# The </dev/null is used to prevent openssl from waiting for stdin.
# 2>/dev/null suppresses connection errors from printing to the console.
CERT_CONTENT=$(openssl s_client -connect $SERVER_ADDRESS:$SERVER_PORT </dev/null 2>/dev/null | openssl x509 -outform PEM)

if [ -z "$CERT_CONTENT" ]; then
    echo "!!! Failed to retrieve certificate from $SERVER_ADDRESS:$SERVER_PORT. !!!"
    echo "!!! Please check the address, port, and network connectivity. !!!"
    exit 1
fi

# Create a temporary file for the certificate
CERT_FILE=$(mktemp)
echo "$CERT_CONTENT" > "$CERT_FILE"
echo "--> Certificate successfully fetched and stored in temp file: $CERT_FILE"

# --- OS-Specific Installation ---
CERT_PATH=""
UPDATE_COMMAND=""
CERT_FILENAME="$SERVER_ADDRESS.crt"

# Check for Debian/Ubuntu
if [ -f /etc/debian_version ]; then
    echo "--> Detected Debian-based OS (Ubuntu, Debian, Mint)."
    CERT_PATH="/usr/local/share/ca-certificates/$CERT_FILENAME"
    UPDATE_COMMAND="update-ca-certificates"

# Check for Red Hat/CentOS
elif [ -f /etc/redhat-release ]; then
    echo "--> Detected Red Hat-based OS (CentOS, RHEL, Fedora)."
    CERT_PATH="/etc/pki/ca-trust/source/anchors/$CERT_FILENAME"
    UPDATE_COMMAND="update-ca-trust"
else
    echo "!!! Unsupported Linux distribution. !!!"
    echo "Please manually install the certificate from the temp file: $CERT_FILE"
    exit 1
fi

echo "--> Installing certificate to $CERT_PATH..."
cp "$CERT_FILE" "$CERT_PATH"

if [ $? -ne 0 ]; then
    echo "!!! Failed to copy certificate to system store. Check permissions. !!!"
    rm "$CERT_FILE"
    exit 1
fi

echo "--> Updating system certificate store..."
$UPDATE_COMMAND

if [ $? -eq 0 ]; then
    echo "==================================================================="
    echo "SUCCESS: Certificate for $SERVER_ADDRESS has been installed and trusted."
    echo "You should now be able to establish a secure LDAP connection."
    echo "==================================================================="
else
    echo "!!! Failed to update certificate store. !!!"
fi

# Clean up the temporary file
rm "$CERT_FILE"