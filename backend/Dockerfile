### build the backend
FROM mcr.microsoft.com/vscode/devcontainers/javascript-node:14

RUN wget https://github.com/Yelp/dumb-init/releases/download/v1.2.5/dumb-init_1.2.5_amd64.deb
RUN dpkg -i dumb-init_*.deb

ENV NODE_ENV production
WORKDIR /usr/src/app

# copy app 
COPY --chown=node:node . .

# install dependencies
RUN npm ci --only=production

EXPOSE 8080

USER node 

# start server
CMD ["dumb-init", "node", "server.js"]

STOPSIGNAL SIGQUIT
