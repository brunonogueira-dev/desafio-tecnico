# ---------- build ----------
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---------- runtime ----------
FROM nginx:1.27-alpine AS final

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

# Script executado pelo entrypoint do nginx antes de subir o servidor:
# gera /config.js a partir da variável API_BASE_URL.
COPY docker/40-onibus-config.sh /docker-entrypoint.d/40-onibus-config.sh
RUN chmod +x /docker-entrypoint.d/40-onibus-config.sh

EXPOSE 80
