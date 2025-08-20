#!/bin/bash
# ==============================================================================
#
# Blazam Installation & Configuration Script
#
# This script installs Blazam, a web application, and configures it to run as
# a systemd service with a reverse proxy (user's choice of Nginx or Apache).
#
# It also includes an optional feature to send the installation script's
# own logs to a Seq server for centralized monitoring.
#
# ==============================================================================

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Configuration ---
readonly BLAZAM_RELEASE_TAG="BetaDev1"
readonly BLAZAM_ZIP_FILENAME="blazam-dev-beta-v1.4.0.2025.06.26.2119.zip"
readonly DOWNLOAD_URL="https://github.com/Blazam-App/BLAZAM/releases/download/${BLAZAM_RELEASE_TAG}/${BLAZAM_ZIP_FILENAME}"

# System and application settings
readonly INSTALL_DIR="/opt/blazam"
readonly DATA_DIR="/var/lib/blazam"
readonly APP_USER="blazamuser"
readonly SERVICE_NAME="blazam"
readonly DOTNET_EXECUTABLE="/usr/bin/dotnet"

# Network and Proxy settings
readonly BLAZAM_INTERNAL_PORT="5000"
readonly BLAZAM_INTERNAL_HTTPS_PORT="5001"
readonly SSL_CERT_PATH="/etc/ssl/certs/blazam-selfsigned.crt"
readonly SSL_KEY_PATH="/etc/ssl/private/blazam-selfsigned.key"


# --- Seq Logging Configuration ---
# Set ENABLE_SEQ_LOGGING to "true" to send this script's logs to a Seq server.
readonly ENABLE_SEQ_LOGGING="true"
readonly SEQ_SERVER_URL="http://logs.blazam.org:5341" # e.g., http://seq.example.com:5341
readonly SEQ_API_KEY="ZwXWKRu2lMrJ9qHaFTzx" # Optional: Your Seq Ingestion API Key

# --- Global Variables ---
DOMAIN_NAME=""
DB_TYPE=""
DB_CONN_STR=""
WEB_SERVER_CHOICE=""

# --- Helper Functions ---

send_to_seq() {
    # Fire-and-forget: run in the background to not delay the script.
    # Requires `jq` and `curl` to be installed.
    (
        local level=$1
        local message=$2
        local timestamp
        timestamp=$(date -u +"%Y-%m-%dT%H:%M:%S.%NZ")

        # Get the OS Version from /etc/os-release
        local os_version="unknown"
        if [ -f /etc/os-release ]; then
            os_version=$(source /etc/os-release && echo "$PRETTY_NAME")
        fi

        # Create a JSON payload in CLEF (Compact Log Event Format)
        local json_payload
        json_payload=$(jq -c -n \
            --arg t "$timestamp" \
            --arg l "$level" \
            --arg m "$message" \
            --arg host "$(hostname)" \
            --arg scriptName "$0" \
            --arg osVersion "$os_version" \
            '{ "@t": $t, "@l": $l, "@m": $m, "Properties": { "Host": $host, "ScriptName": $scriptName, "OSVersion": $osVersion } }')

        # Add API Key header if provided
        local api_key_header=""
        if [[ -n "${SEQ_API_KEY}" ]]; then
            api_key_header="-H \"X-Seq-ApiKey: ${SEQ_API_KEY}\""
        fi

        # Post the raw event to Seq
        curl -s -o /dev/null -X POST \
             -H "Content-Type: application/vnd.serilog.clef" \
             ${api_key_header} \
             --data-binary "$json_payload" \
             "${SEQ_SERVER_URL}/api/events/raw"
    ) &
}

log_info() {
    echo -e "\e[34m[INFO]\e[0m $1"
    if [[ "${ENABLE_SEQ_LOGGING}" == "true" ]]; then
        send_to_seq "Information" "$1"
    fi
}

log_warn() {
    echo -e "\e[33m[WARN]\e[0m $1"
    if [[ "${ENABLE_SEQ_LOGGING}" == "true" ]]; then
        send_to_seq "Warning" "$1"
    fi
}

log_error() {
    # Note: This will log the error and then the script will exit due to `set -e`
    echo -e "\e[31m[ERROR]\e[0m $1" >&2
    if [[ "${ENABLE_SEQ_LOGGING}" == "true" ]]; then
        send_to_seq "Error" "$1"
    fi
    exit 1
}

# --- Main Installation Functions ---

# 1. Run pre-flight checks to ensure the script can execute.
pre_flight_checks() {
    log_info "Running pre-flight checks..."
    if [ "$(id -u)" -ne 0 ]; then
        log_error "This script must be run as root. Please use sudo."
    fi
    if ! command -v wget &>/dev/null || ! command -v unzip &>/dev/null; then
        log_info "Installing 'wget' and 'unzip'..."
        apt-get update
        apt-get install -y wget unzip
    fi
}

# 2. Get necessary input from the user.
get_user_input() {
    log_info "Gathering user input..."

    # Prompt for Domain Name
    read -r -p "Enter the domain name or IP for Blazam (e.g., blazam.example.com): " DOMAIN_NAME
    if [ -z "${DOMAIN_NAME}" ]; then
        log_warn "No domain name entered. Defaulting to 'localhost'."
        DOMAIN_NAME="localhost"
    fi
    log_info "Using domain/IP: ${DOMAIN_NAME}"

    # Prompt for Web Server
    log_info "Please select the reverse proxy web server to use."
    PS3="Enter the number for your choice: "
    select choice in "Nginx" "Apache"; do
        case $choice in
            "Nginx")
                WEB_SERVER_CHOICE="Nginx"
                log_info "Nginx selected as the reverse proxy."
                break
                ;;
            "Apache")
                WEB_SERVER_CHOICE="Apache"
                log_info "Apache selected as the reverse proxy."
                break
                ;;
            *)
                echo "Invalid option $REPLY. Please try again."
                ;;
        esac
    done
}

# 3. Install system dependencies required for Blazam and the chosen web server.
install_dependencies() {
    log_info "Updating package lists and installing dependencies..."
    apt-get update
    
    local web_server_package=""
    if [[ "$WEB_SERVER_CHOICE" == "Nginx" ]]; then
        web_server_package="nginx"
    else
        web_server_package="apache2"
    fi

    apt-get install -y aspnetcore-runtime-8.0 "${web_server_package}" openssl jq libldap2 curl wget unzip

    if ! command -v $DOTNET_EXECUTABLE &> /dev/null; then
        log_error "dotnet executable not found. ASP.NET Core runtime installation may have failed."
    fi
}

# 4. Fix .NET's dependency on an older OpenLDAP library version.
fix_ldap_symlink() {
    log_info "Fixing LDAP library compatibility..."
    local link_name="libldap-2.5.so.0"
    local lib_dir="/usr/lib/x86_64-linux-gnu"
    local link_path="${lib_dir}/${link_name}"
    local target_lib_path
    target_lib_path=$(find "${lib_dir}" -name "libldap.so.*" -type f | sort -V | tail -n 1)
    if [[ -z "${target_lib_path}" ]]; then
        log_error "Could not find any libldap.so file in ${lib_dir}. Please ensure libldap2 is installed."
    fi
    log_info "Found target library: ${target_lib_path}"
    log_info "Creating symbolic link: ${link_path} -> ${target_lib_path}"
    ln -sf "${target_lib_path}" "${link_path}"
    log_info "LDAP symbolic link created successfully."
}

# 5. Create the application user, group, and necessary directories.
setup_user_and_dirs() {
    log_info "Setting up user, group, and directories..."
    if ! getent group "${APP_USER}" >/dev/null; then groupadd --system "${APP_USER}"; fi
    if ! id -u "${APP_USER}" >/dev/null; then useradd --system --no-create-home --shell /bin/false --gid "${APP_USER}" "${APP_USER}"; fi
    mkdir -p "${INSTALL_DIR}"
    mkdir -p "${DATA_DIR}"
    log_info "Created directories: ${INSTALL_DIR} and ${DATA_DIR}"
}

# 6. Download and extract the Blazam application files.
download_and_install_blazam() {
    log_info "Downloading Blazam from ${DOWNLOAD_URL}..."
    cd /tmp
    wget -q -O "${BLAZAM_ZIP_FILENAME}" "${DOWNLOAD_URL}"
    log_info "Extracting Blazam to ${INSTALL_DIR}..."
    unzip -oq "${BLAZAM_ZIP_FILENAME}" -d "${INSTALL_DIR}"
    log_info "Extraction complete."
}

# 7. Interactively configure the database connection.
configure_database() {
    log_info "Please select the database type Blazam will use."
    PS3="Enter the number for your choice: "
    select choice in "SQLite" "Microsoft SQL Server" "MySQL / MariaDB"; do
        case $choice in
            "SQLite") DB_TYPE="Sqlite"; DB_CONN_STR="Data Source=${DATA_DIR}/Blazam.db"; break;;
            "Microsoft SQL Server")
                DB_TYPE="SqlServer"
                read -r -p "Enter Server address/IP: " DB_SERVER
                read -r -p "Enter Database Name: " DB_NAME
                read -r -p "Enter User ID: " DB_USER
                read -r -s -p "Enter Password: " DB_PASS; echo ""
                DB_CONN_STR="Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASS};TrustServerCertificate=True"
                break;;
            "MySQL / MariaDB")
                DB_TYPE="MySql"
                read -r -p "Enter Server address/IP: " DB_SERVER
                read -r -p "Enter Database Name: " DB_NAME
                read -r -p "Enter User ID: " DB_USER
                read -r -s -p "Enter Password: " DB_PASS; echo ""
                DB_CONN_STR="server=${DB_SERVER};database=${DB_NAME};user=${DB_USER};password=${DB_PASS};"
                break;;
            *) echo "Invalid option $REPLY.";;
        esac
    done
}

# 8. Configure the appsettings.json file.
configure_blazam_appsettings() {
    log_info "Configuring Blazam application settings..."
    configure_database
    local appsettings_path="${INSTALL_DIR}/appsettings.json"
    local appsettings_example_path="${INSTALL_DIR}/appsettings.example.json"
    if [[ ! -f "$appsettings_example_path" ]]; then log_error "appsettings.example.json not found!"; fi
    cp "$appsettings_example_path" "$appsettings_path"
    local encryption_key; encryption_key=$(LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 32)
    local tmp_json; tmp_json=$(mktemp)
    
    # Use jq to modify the JSON file, converting port strings to numbers
    jq \
      --arg key "$encryption_key" \
      --arg httpport "$BLAZAM_INTERNAL_PORT" \
      --arg httpsport "$BLAZAM_INTERNAL_HTTPS_PORT" \
      --arg dbtype "$DB_TYPE" \
      --arg connstr "$DB_CONN_STR" \
      '.EncryptionKey = $key |
       .HTTPPort = $httpport |
       .HTTPSPort = $httpsport |
       .ListeningAddress = "localhost" |
       .DatabaseType = $dbtype |
       .ConnectionStrings.DBConnectionString = $connstr' \
      "$appsettings_path" > "$tmp_json" && mv "$tmp_json" "$appsettings_path"

    log_info "appsettings.json configured successfully."
}

# 9. Set the correct file ownership and permissions.
set_permissions() {
    log_info "Setting file permissions..."
    chown -R "${APP_USER}":"${APP_USER}" "${INSTALL_DIR}"
    chmod -R 750 "${INSTALL_DIR}"
    chown -R "${APP_USER}":"${APP_USER}" "${DATA_DIR}"
    chmod -R 750 "${DATA_DIR}"
    chown "${APP_USER}":"${APP_USER}" "${INSTALL_DIR}/appsettings.json"
    chmod 640 "${INSTALL_DIR}/appsettings.json"
    log_info "Permissions set."
}

# 10. Create and enable the systemd service for Blazam.
setup_systemd_service() {
    log_info "Creating and starting systemd service..."
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
Environment=ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
[Install]
WantedBy=multi-user.target
EOL
    systemctl daemon-reload
    systemctl enable "${SERVICE_NAME}.service"
    systemctl start "${SERVICE_NAME}.service"
    sleep 5
    if ! systemctl is-active --quiet "${SERVICE_NAME}"; then
        log_warn "Blazam service may have failed. Check logs: journalctl -u ${SERVICE_NAME}"
    else
        log_info "Blazam service started successfully."
    fi
}

# 11. Generate a shared self-signed SSL certificate.
generate_ssl_cert() {
    log_info "Generating self-signed SSL certificate for ${DOMAIN_NAME}..."
    log_warn "This certificate is for testing. Browsers will show a security warning."
    mkdir -p "$(dirname "${SSL_CERT_PATH}")" "$(dirname "${SSL_KEY_PATH}")"
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout "${SSL_KEY_PATH}" \
        -out "${SSL_CERT_PATH}" \
        -subj "/CN=${DOMAIN_NAME}"
    chmod 600 "${SSL_KEY_PATH}"
}

# 12a. Configure Nginx as a reverse proxy.
setup_nginx_reverse_proxy() {
    log_info "Configuring Nginx reverse proxy..."
    local nginx_config_file="/etc/nginx/sites-available/blazam"
    rm -f /etc/nginx/sites-enabled/default
    cat > "${nginx_config_file}" <<EOL
server {
    listen 80;
    server_name ${DOMAIN_NAME};
    location / { return 301 https://\$host\$request_uri; }
}
server {
    listen 443 ssl http2;
    server_name ${DOMAIN_NAME};
    ssl_certificate ${SSL_CERT_PATH};
    ssl_certificate_key ${SSL_KEY_PATH};
    ssl_protocols TLSv1.2 TLSv1.3;
    location / {
        proxy_pass http://localhost:${BLAZAM_INTERNAL_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOL
    if ! grep -q "map \$http_upgrade \$connection_upgrade" "/etc/nginx/nginx.conf"; then
        sed -i "/http {/a \    map \$http_upgrade \$connection_upgrade {\n        default upgrade;\n        ''      close;\n    }" "/etc/nginx/nginx.conf"
    fi
    ln -sf "${nginx_config_file}" "/etc/nginx/sites-enabled/blazam"
    if nginx -t; then
        log_info "Nginx configuration OK. Restarting Nginx..."
        systemctl restart nginx
    else
        log_error "Nginx configuration test failed."
    fi
}

# 12b. Configure Apache as a reverse proxy.
setup_apache_reverse_proxy() {
    log_info "Configuring Apache reverse proxy..."
    local apache_config_file="/etc/apache2/sites-available/blazam.conf"
    rm -f /etc/apache2/sites-enabled/000-default.conf
    cat > "${apache_config_file}" <<EOL
<VirtualHost *:80>
    ServerName ${DOMAIN_NAME}
    Redirect permanent / https://${DOMAIN_NAME}/
</VirtualHost>

<VirtualHost *:443>
    ServerName ${DOMAIN_NAME}
    
    # SSL Configuration
    SSLEngine on
    SSLCertificateFile ${SSL_CERT_PATH}
    SSLCertificateKeyFile ${SSL_KEY_PATH}

    # Proxy Configuration
    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:${BLAZAM_INTERNAL_PORT}/
    ProxyPassReverse / http://127.0.0.1:${BLAZAM_INTERNAL_PORT}/
    
    # Required for SignalR WebSockets
    RewriteEngine on
    RewriteCond %{HTTP:UPGRADE} ^WebSocket$ [NC]
    RewriteCond %{HTTP:CONNECTION} Upgrade$ [NC]
    RewriteRule /(.*) ws://127.0.0.1:${BLAZAM_INTERNAL_PORT}/\$1 [P,L]

    RequestHeader set "X-Forwarded-Proto" "https"
</VirtualHost>
EOL
    log_info "Enabling required Apache modules..."
    a2enmod proxy proxy_http ssl headers rewrite
    a2ensite blazam.conf
    
    if apache2ctl configtest; then
        log_info "Apache configuration OK. Restarting Apache..."
        systemctl restart apache2
    else
        log_error "Apache configuration test failed."
    fi
}

# 13. Configure UFW firewall to allow web traffic.
configure_firewall() {
    if ! command -v ufw &> /dev/null; then
        log_warn "UFW is not installed. Please configure your firewall for ports 80 and 443."
        return
    fi
    
    local ufw_profile=""
    if [[ "$WEB_SERVER_CHOICE" == "Nginx" ]]; then
        ufw_profile="Nginx Full"
    else
        ufw_profile="Apache Full"
    fi

    log_info "Configuring UFW firewall to allow '${ufw_profile}'..."
    ufw allow "${ufw_profile}"
    ufw allow OpenSSH
    if ! ufw status | grep -qw active; then
        yes | ufw enable
    fi
    ufw reload
    log_info "UFW reloaded."
}

# 14. Clean up temporary installation files.
cleanup() {
    log_info "Cleaning up downloaded files..."
    rm -f "/tmp/${BLAZAM_ZIP_FILENAME}"
}

# 15. Print a final summary with next steps.
print_summary() {
    local certbot_command=""
    local log_path=""
    if [[ "$WEB_SERVER_CHOICE" == "Nginx" ]]; then
        certbot_command="sudo certbot --nginx -d ${DOMAIN_NAME}"
        log_path="/var/log/nginx/"
    else
        certbot_command="sudo certbot --apache -d ${DOMAIN_NAME}"
        log_path="/var/log/apache2/"
    fi

    log_info "--------------------------------------------------------------------"
    log_info "Blazam installation with ${WEB_SERVER_CHOICE} reverse proxy completed!"
    log_info "Blazam should be accessible at: https://${DOMAIN_NAME}"
    log_warn "You will see a browser warning due to the self-signed SSL certificate."
    log_info ""
    log_info "Next Steps:"
    log_info " > Check Blazam service:   systemctl status ${SERVICE_NAME}"
    log_info " > View Blazam logs:       journalctl -u ${SERVICE_NAME} -f"
    log_info " > View ${WEB_SERVER_CHOICE} logs:     ${log_path}"
    log_info " > For production, get a valid SSL certificate with: ${certbot_command}"
    log_info "--------------------------------------------------------------------"
}

# --- Script Execution ---

main() {
    log_info "Starting Blazam installation script."
    pre_flight_checks
    get_user_input
    install_dependencies
    fix_ldap_symlink
    setup_user_and_dirs
    download_and_install_blazam
    configure_blazam_appsettings
    set_permissions
    setup_systemd_service
    
    generate_ssl_cert
    if [[ "$WEB_SERVER_CHOICE" == "Nginx" ]]; then
        setup_nginx_reverse_proxy
    else
        setup_apache_reverse_proxy
    fi
    
    configure_firewall
    cleanup
    print_summary
    log_info "Blazam installation script finished successfully."
    exit 0
}

# Run the main function to start the installation.
main