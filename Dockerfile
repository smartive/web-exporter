### Prepare deploy image (cacheable)
FROM microsoft/dotnet:2.2-runtime as deploy

WORKDIR /app
EXPOSE 80

RUN apt-get update && apt-get install -y gnupg2 && \
  curl -sL https://deb.nodesource.com/setup_10.x | bash - && \
  apt-get install -y nodejs && \
  ln -sf /usr/share/zoneinfo/Europe/Zurich /etc/localtime && \
  dpkg-reconfigure -f noninteractive tzdata && \
  mkdir -p /app/Data

CMD dotnet WebApp.dll

### Build node related things
FROM node:10 as nodejs

WORKDIR /app

COPY ./Frontend ./frontend
COPY ./WebApp ./webapp

RUN cd frontend && npm ci && npm run build
RUN cd webapp && npm ci

### Build Backend
FROM microsoft/dotnet:2.2-sdk AS build

WORKDIR /app

COPY . ./
COPY --from=nodejs /app/frontend/build ./WebApp/wwwroot

RUN ./build.sh

### Deploy
FROM deploy as final

COPY --from=build /app/artifacts .
COPY --from=nodejs /app/webapp/node_modules ./node_modules
