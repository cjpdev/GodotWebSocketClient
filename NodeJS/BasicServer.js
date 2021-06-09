const fs = require('fs');
const https = require('https');
const webSocket = require('ws');

optionsHttpServer = {
  cert: fs.readFileSync('./cert/cert.pem'),
  key: fs.readFileSync('./cert/key.pem'),
};

const httpsServer = https.createServer(optionsHttpServer, function (req, res) {
  console.log('Client https connection.');
});

optionsSocket = {
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