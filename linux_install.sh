#!/bin/bash

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Configuration ---
BLAZAM_RELEASE_TAG="Release-v1.4.0.2025.05.15.2236" # Check for the latest release tag
BLAZAM_ZIP_FILENAME="blazam-stable-v1.4.0.2025.05.15.2236.zip" # Filename from the release
DOWNLOAD_URL="https://github.com/Blazam-App/BLAZAM/releases/download/${BLAZAM_RELEASE_TAG}/${BLAZAM_ZIP_FILENAME}"

INSTALL_DIR="/opt/blazam"
DATA_DIR="/var/lib/blazam"
APP_USER="blazamuser"
SERVICE_NAME="blazam"
DOTNET_EXECUTABLE="/usr/bin/dotnet"

BLAZAM_INTERNAL_PORT="5000" # Port Blazam will listen on internally
NGINX_CONFIG_FILE="/etc/nginx/sites-available/blazam"
NGINX_ENABLED_CONFIG_FILE="/etc/nginx/sites-enabled/blazam"

SSL_CERT_PATH="/etc/ssl/certs/blazam-selfsigned.crt"
SSL_KEY_PATH="/etc/ssl/private/blazam-selfsigned.key"

# --- Helper Functions ---
log_info() {
    echo "[INFO] $1"
}

log_warn() {
    echo "[WARN] $1"
}

log_error() {
    echo "[ERROR] $1" >&2
    exit 1
}
# --- Database Configuration Function ---
configure_database() {
    # Make DB_TYPE and DB_CONN_STR available to the rest of the script
    export DB_TYPE=""
    export DB_CONN_STR=""

    log_info "Please select the database type Blazam will use."
    log_warn "SQLite is recommended for simple, self-contained deployments."

    # PS3 is the prompt for the select menu
    PS3="Enter the number for your choice: "

    select choice in "SQLite" "Microsoft SQL Server" "MySQL / MariaDB" "PostgreSQL"; do
        case $choice in
            "SQLite")
                DB_TYPE="Sqlite"
                DB_CONN_STR="Data Source=${DATA_DIR}/Blazam.db"
                log_info "SQLite selected. Database will be created in ${DATA_DIR}/Blazam.db"
                break
                ;;
            "Microsoft SQL Server")
                DB_TYPE="SqlServer"
                read -r -p "Enter Server address/IP: " DB_SERVER
                read -r -p "Enter Database Name: " DB_NAME
                read -r -p "Enter User ID: " DB_USER
                read -r -s -p "Enter Password: " DB_PASS
                echo "" # Newline after password input
                DB_CONN_STR="Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASS};TrustServerCertificate=True"
                break
                ;;
            "MySQL / MariaDB")
                # Blazam uses the Pomelo provider, which identifies as "MySql"
                DB_TYPE="MySql"
                read -r -p "Enter Server address/IP: " DB_SERVER
                read -r -p "Enter Database Name: " DB_NAME
                read -r -p "Enter User ID: " DB_USER
                read -r -s -p "Enter Password: " DB_PASS
                echo "" # Newline after password input
                DB_CONN_STR="server=${DB_SERVER};database=${DB_NAME};user=${DB_USER};password=${DB_PASS};"
                break
                ;;
            "PostgreSQL")
                DB_TYPE="Postgres"
                read -r -p "Enter Server address/IP: " DB_SERVER
                read -r -p "Enter Database Name: " DB_NAME
                read -r -p "Enter Username: " DB_USER
                read -r -s -p "Enter Password: " DB_PASS
                echo "" # Newline after password input
                DB_CONN_STR="Host=${DB_SERVER};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS};"
                break
                ;;
            *)
                echo "Invalid option $REPLY. Please try again."
                ;;
        esac
    done

    log_warn "Connection details have been configured but NOT tested."
    log_warn "If the Blazam service fails to start, check the logs for database connection errors: journalctl -u ${SERVICE_NAME}"
}

# --- Pre-flight Checks ---
if [ "$(id -u)" -ne 0 ]; then
  log_error "This script must be run as root. Please use sudo."
fi

# --- Main Installation ---
log_info "Starting Blazam Installation Script with Nginx Reverse Proxy..."

# 1. Prompt for Domain Name
read -r -p "Enter the domain name or IP address for Blazam (e.g., blazam.example.com or your_server_ip): " DOMAIN_NAME
if [ -z "${DOMAIN_NAME}" ]; then
    log_warn "No domain name entered. Using 'localhost'. This is fine for local testing."
    log_warn "If accessing externally, you'll need to edit ${NGINX_CONFIG_FILE} and use a proper domain/IP."
    DOMAIN_NAME="localhost"
fi
log_info "Using domain/IP: ${DOMAIN_NAME}"

# 2. Update System and Install Dependencies
log_info "Updating package lists..."
apt-get update

log_info "Installing ASP.NET Core Runtime 8.0, unzip, Nginx, OpenSSL, and JQs..."
apt-get install -y aspnetcore-runtime-8.0 unzip nginx openssl jq libldap2
if ! command -v $DOTNET_EXECUTABLE &> /dev/null; then
    log_error "dotnet executable not found at $DOTNET_EXECUTABLE. ASP.NET Core runtime installation might have failed or it's in a different path."
fi
if ! command -v nginx &> /dev/null; then
    log_error "Nginx installation failed or not found."
fi

#!/bin/bash

# ==============================================================================
# Create Blazam LDAP Compatibility Symlink
#
# This script fixes a common issue where .NET applications look for an older
# version of the OpenLDAP library (libldap-2.5.so.0) on modern Linux systems
# that have a newer version installed.
#
# It automatically detects the most recent version of libldap.so.* installed
# in /usr/lib/x86_64-linux-gnu and creates a symbolic link to it.
#
# USAGE: Run with sudo: sudo ./create_ldap_symlink.sh
# ==============================================================================

# --- Configuration ---
# The name of the symbolic link the application is looking for.
LINK_NAME="libldap-2.5.so.0"

# The directory where system libraries are typically stored.
LIB_DIR="/usr/lib/x86_64-linux-gnu"

# The full path for the symbolic link we need to create.
LINK_PATH="${LIB_DIR}/${LINK_NAME}"



log_info "Searching for the latest libldap library in ${LIB_DIR}..."

# Find the real library file (not a symlink) with the highest version number.
# - The `find` command locates all files named "libldap.so.*".
# - The `-type f` flag ensures we only get actual files, not other symlinks.
# - `sort -V` sorts the results by version number (handles numbers like 2.0.200 correctly).
# - `tail -n 1` gets the last item from the sorted list, which is the newest version.
TARGET_LIB_PATH=$(find "${LIB_DIR}" -name "libldap.so.*" -type f | sort -V | tail -n 1)

# Check if we found a library file. If not, exit with an error.
if [[ -z "${TARGET_LIB_PATH}" ]]; then
  log_error "Error: Could not find any libldap.so file in ${LIB_DIR}."
  log_error "Please ensure libldap2 is installed ('sudo apt install libldap2')."
  exit 1
fi

log_info "Found target library: ${TARGET_LIB_PATH}"

# Check if the link already exists and points to the correct target.
if [[ -L "${LINK_PATH}" ]] && [[ "$(readlink "${LINK_PATH}")" == "${TARGET_LIB_PATH}" ]]; then
  log_info "Success: Symbolic link already exists and is correct. Nothing to do."
  ls -l "${LINK_PATH}"
fi

log_info "Creating/updating symbolic link..."

# Create the symbolic link.
# - `ln` is the command to create links.
# - `-s` creates a symbolic (or "soft") link.
# - `-f` (force) will overwrite the link if it already exists but points to the wrong file.
ln -sf "${TARGET_LIB_PATH}" "${LINK_PATH}"

# Verify that the link was created successfully.
if [[ $? -eq 0 ]]; then
  log_info "Success: Symbolic link has been created."
  log_info "Details:"
  ls -l "${LINK_PATH}"
else
    log_error "Error: Failed to create symbolic link."
  exit 1
fi

# 3. Create Application User and Group (Corrected)
# Create application group if it doesn't exist
if ! getent group "${APP_USER}" >/dev/null 2>&1; then
    log_info "Creating system group '${APP_USER}'..."
    groupadd --system "${APP_USER}"
else
    log_info "Group '${APP_USER}' already exists."
fi

# Create application user if it doesn't exist
if ! id -u "${APP_USER}" >/dev/null 2>&1; then
    log_info "Creating system user '${APP_USER}'..."
    useradd --system --no-create-home --shell /bin/false --gid "${APP_USER}" "${APP_USER}"
else
    log_info "User '${APP_USER}' already exists."
fi

# 4. Create Application and Data Directories
log_info "Creating installation directory '${INSTALL_DIR}'..."
mkdir -p "${INSTALL_DIR}"

# <--- MODIFIED: Create data directory
log_info "Creating data directory '${DATA_DIR}' for persistent data..."
mkdir -p "${DATA_DIR}"

# 5. Download and Extract Blazam
log_info "Downloading Blazam from ${DOWNLOAD_URL}..."
cd /tmp
wget -q -O "${BLAZAM_ZIP_FILENAME}" "${DOWNLOAD_URL}"

log_info "Extracting Blazam to ${INSTALL_DIR}..."
unzip -o "${BLAZAM_ZIP_FILENAME}" -d "${INSTALL_DIR}"

# 6. Set Permissions
log_info "Setting permissions for application directory: ${INSTALL_DIR}..."
chown -R "${APP_USER}":"${APP_USER}" "${INSTALL_DIR}"
chmod -R 750 "${INSTALL_DIR}"

# <--- MODIFIED: Set permissions for data directory
log_info "Setting permissions for data directory: ${DATA_DIR}..."
chown -R "${APP_USER}":"${APP_USER}" "${DATA_DIR}"
chmod -R 750 "${DATA_DIR}"


# 7. Configure appsettings.json
log_info "Starting interactive database configuration..."

# Call the function to get user's DB choice
configure_database

APPSETTINGS_PATH="${INSTALL_DIR}/appsettings.json"
APPSETTINGS_EXAMPLE_PATH="${INSTALL_DIR}/appsettings.example.json"

if [ -f "$APPSETTINGS_EXAMPLE_PATH" ]; then
    log_info "Creating appsettings.json from example..."
    cp "$APPSETTINGS_EXAMPLE_PATH" "$APPSETTINGS_PATH"
else
    log_error "appsettings.example.json not found! Cannot configure application."
fi

log_info "Generating new 32-character encryption key..."
ENCRYPTION_KEY=$(LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 32)

log_info "Applying settings to appsettings.json..."
# Use a temporary file for the jq output to safely overwrite the original
TMP_JSON=$(mktemp)

# Use jq to modify the JSON file with the variables set in the configure_database function
jq \
  --arg key "$ENCRYPTION_KEY" \
  --arg httpport "$BLAZAM_INTERNAL_PORT" \
  --arg listenaddr "localhost" \
  --arg dbtype "$DB_TYPE" \
  --arg connstr "$DB_CONN_STR" \
  '.EncryptionKey = $key |
   .HTTPPort = $httpport |
   .HTTPSPort = "0" |
   .ListeningAddress = $listenaddr |
   .DatabaseType = $dbtype |
   .ConnectionStrings.DBConnectionString = $connstr' \
  "$APPSETTINGS_PATH" > "$TMP_JSON" && mv "$TMP_JSON" "$APPSETTINGS_PATH"

log_info "Setting permissions on appsettings.json..."
# Ensure the new config file is owned by the app user and has secure permissions
chown "${APP_USER}":"${APP_USER}" "$APPSETTINGS_PATH"
chmod 640 "$APPSETTINGS_PATH"

# 8. Create Blazam Systemd Service File
log_info "Creating systemd service file for ${SERVICE_NAME}..."
cat > "/etc/systemd/system/${SERVICE_NAME}.service" <<EOL
[Unit]
Description=Blazam Web Application
After=network.target

[Service]
WorkingDirectory=${INSTALL_DIR}
ExecStart=${DOTNET_EXECUTABLE} ${INSTALL_DIR}/BLAZAM.dll --urls="http://localhost:${BLAZAM_INTERNAL_PORT}"
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=${SERVICE_NAME}
User=${APP_USER}
Group=${APP_USER}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
# For ASP.NET Core to understand it's behind a proxy
Environment=ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

[Install]
WantedBy=multi-user.target
EOL

# 9. Reload Systemd, Enable and Start Blazam Service
log_info "Reloading systemd daemon..."
systemctl daemon-reload

log_info "Enabling ${SERVICE_NAME} service to start on boot..."
systemctl enable "${SERVICE_NAME}.service"

log_info "Starting ${SERVICE_NAME} service..."
systemctl start "${SERVICE_NAME}.service"

# Check Blazam service status briefly
log_info "Waiting a few seconds for Blazam to initialize..."
sleep 5
if ! systemctl is-active --quiet "${SERVICE_NAME}"; then
    log_warn "Blazam service (${SERVICE_NAME}) does not seem to be active. Check logs for errors: journalctl -u ${SERVICE_NAME}"
fi

# 10. Generate Self-Signed SSL Certificate
log_info "Generating self-signed SSL certificate for ${DOMAIN_NAME}..."
log_warn "This is a self-signed certificate, suitable for testing. Browsers will show a warning."
log_warn "For production, replace ${SSL_CERT_PATH} and ${SSL_KEY_PATH} with a valid certificate (e.g., from Let's Encrypt)."

mkdir -p "$(dirname "${SSL_CERT_PATH}")" "$(dirname "${SSL_KEY_PATH}")"

openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout "${SSL_KEY_PATH}" \
    -out "${SSL_CERT_PATH}" \
    -subj "/CN=${DOMAIN_NAME}"

chmod 600 "${SSL_KEY_PATH}" # Secure the private key

# 111. Configure Nginx
log_info "Configuring Nginx reverse proxy..."

# Remove default Nginx site if it exists
rm -f /etc/nginx/sites-enabled/default

cat > "${NGINX_CONFIG_FILE}" <<EOL
server {
    listen 80;
    server_name ${DOMAIN_NAME};

    # Redirect all HTTP traffic to HTTPS
    location / {
        return 301 https://\$host\$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name ${DOMAIN_NAME};

    ssl_certificate ${SSL_CERT_PATH};
    ssl_certificate_key ${SSL_KEY_PATH};

    # Modern SSL configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers off;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 1d;
    ssl_session_tickets off;

    # HSTS (Highly Recommended for production after confirming HTTPS works correctly)
    # add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;

    # Max file upload size (adjust as needed)
    client_max_body_size 100M;

    location / {
        proxy_pass http://localhost:${BLAZAM_INTERNAL_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade; # Handles "keep-alive" and "Upgrade"
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header X-Forwarded-Host \$server_name;
        proxy_read_timeout 90s;
    }
}
EOL

# Add map directive to nginx.conf if it's not there for WebSocket support
NGINX_CONF_PATH="/etc/nginx/nginx.conf"
if ! grep -q "map \$http_upgrade \$connection_upgrade" "${NGINX_CONF_PATH}"; then
    log_info "Adding 'map' directive to ${NGINX_CONF_PATH} for WebSocket support..."
    # Correctly insert the multi-line block into the http block
    sed -i "/http {/a \ \ \ \ map \$http_upgrade \$connection_upgrade {\n        default upgrade;\n        ''      close;\n    }" "${NGINX_CONF_PATH}"
else
    log_info "'map' directive already exists in ${NGINX_CONF_PATH}."
fi

# Enable the Nginx site
if [ ! -L "${NGINX_ENABLED_CONFIG_FILE}" ]; then
    ln -s "${NGINX_CONFIG_FILE}" "${NGINX_ENABLED_CONFIG_FILE}"
else
    log_info "Nginx site symlink already exists."
fi

# Test Nginx configuration and restart
log_info "Testing Nginx configuration..."
if nginx -t; then
    log_info "Nginx configuration test successful. Restarting Nginx..."
    systemctl restart nginx
else
    log_error "Nginx configuration test failed. Please check ${NGINX_CONFIG_FILE} and Nginx error logs."
fi

# 12. Firewall Configuration (UFW example)
log_info "--------------------------------------------------------------------"
log_info "Configuring Firewall (UFW)..."
if command -v ufw &> /dev/null; then
    log_info "Allowing Nginx Full profile (HTTP & HTTPS) through UFW..."
    ufw allow 'Nginx Full'
    ufw allow OpenSSH # Ensure SSH access is not accidentally blocked
    if ! ufw status | grep -qw active; then
        log_info "UFW is inactive. Enabling UFW..."
        yes | ufw enable # Auto-confirm enable
    fi
    ufw reload
    log_info "UFW Status:"
    ufw status verbose
else
    log_warn "UFW is not installed. Please configure your firewall manually to allow traffic on ports 80 and 443."
fi
log_info "--------------------------------------------------------------------"

# 13. Cleanup
log_info "Cleaning up downloaded files..."
rm -f "/tmp/${BLAZAM_ZIP_FILENAME}"

log_info "--------------------------------------------------------------------"
log_info "Blazam installation with Nginx reverse proxy completed!"
log_info "Blazam should be accessible at: https://${DOMAIN_NAME}"
log_info "(You will likely see a browser warning due to the self-signed SSL certificate)"
log_info ""
log_info "To check Blazam service: systemctl status ${SERVICE_NAME}"
log_info "Blazam logs: journalctl -u ${SERVICE_NAME} -f"
log_info "Nginx logs: /var/log/nginx/access.log and /var/log/nginx/error.log"
log_info ""
log_warn "PRODUCTION RECOMMENDATIONS:"
log_warn "1. Replace the self-signed SSL certificate with one from Let's Encrypt."
log_warn "   Consider using 'certbot': sudo apt install certbot python3-certbot-nginx; sudo certbot --nginx -d ${DOMAIN_NAME}"
log_warn "2. Review and harden Nginx security settings (e.g., enable HSTS after confirming HTTPS)."
log_info "--------------------------------------------------------------------"

exit 0