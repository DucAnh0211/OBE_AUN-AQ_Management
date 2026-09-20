FROM node:24-alpine AS build
WORKDIR /src/apps/web

COPY apps/web/package.json apps/web/package-lock.json ./
RUN npm ci

WORKDIR /src
COPY apps/web/ apps/web/
COPY shared/frontend/ shared/frontend/
COPY modules/common/curriculum/frontend/ modules/common/curriculum/frontend/
COPY modules/sv4/accreditation/frontend/ modules/sv4/accreditation/frontend/
COPY modules/sv5/reporting/frontend/ modules/sv5/reporting/frontend/

ARG VITE_API_URL=""
ENV VITE_API_URL=${VITE_API_URL}

WORKDIR /src/apps/web
RUN npm run build

FROM nginx:alpine AS final
COPY apps/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/apps/web/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=6 \
    CMD wget --quiet --tries=1 --spider http://127.0.0.1/ || exit 1
