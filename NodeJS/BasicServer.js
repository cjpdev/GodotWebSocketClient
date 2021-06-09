//ES6 
'use strict';

import fs  from 'fs';
import https from 'https';
import webSocket from 'ws';

const optionsHttpServer = {
  cert: fs.readFileSync('./cert/cert.pem'),
  key: fs.readFileSync('./cert/key.pem'),
};

const httpsServer = https.createServer(optionsHttpServer, function (req, res) {
  console.log('Client https connection.');
});

const optionsSocket = {
  server: httpsServer
};

const wsServer = new webSocket.Server(optionsSocket);

wsServer.on('connection', function connection(wsClient) {

  console.log('Client socket connection.');

  wsClient.on('message', function incoming(message) {

    console.log('Godot client says, : %s', message);

  });

  wsClient.send('NodeJS server says, Hi.');

});

httpsServer.listen(8080);