#!/bin/sh
# Injeta a URL da API em runtime, sem rebuild da imagem.
set -e

API_BASE_URL="${API_BASE_URL:-http://localhost:8080}"

cat > /usr/share/nginx/html/config.js <<EOF
window.__ONIBUS_ENV__ = { API_BASE_URL: "${API_BASE_URL}" };
EOF

echo "config.js gerado com API_BASE_URL=${API_BASE_URL}"
