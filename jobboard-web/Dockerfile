# syntax=docker/dockerfile:1

FROM node:24-alpine AS build
WORKDIR /app

RUN corepack enable && corepack prepare pnpm@10.33.2 --activate

COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm run build

FROM node:24-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000
EXPOSE 3000

# Nitro's node-server preset output is fully self-contained - no node_modules
# or package.json needed at runtime.
COPY --from=build /app/.output ./.output

CMD ["node", ".output/server/index.mjs"]
