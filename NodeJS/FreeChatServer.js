const fs = require('fs');
const https = require('https');
const webSocket = require('ws');

options = {
  cert: fs.readFileSync('./cert/cert.pem'),
  key: fs.readFileSync('./cert/key.pem'),
  //rejectUnauthorized: true,
  //requestCert: true,
};

const httpsServer = https.createServer(options, function (req, res) {
  console.log('Client https connection.');
});

optionsSocket = {
  server: httpsServer,
  clientTracking: true
};

const wsServer = new webSocket.Server(optionsSocket);

wsServer.on('connection', function connection(wsClient) {

  console.log('Client socket connection.');
  console.log("Number of clients is: " + wsServer.clients.size);

  wsClient.on('message', function incoming(message) {
  
    wsServer.clients.forEach(function each(client) {

      if (client._readyState === webSocket.OPEN) {

          var newMsg = "";

          if(wsClient == client)
          {
              // echo back your message, good test
              newMsg = "[You said]: " + message;

          } else {
              // everyone else sees
              newMsg = "[Some one said]: " +  message;
          }

          client.send(newMsg);

          console.log("Currnet message:" + message);
      }   
    
    });
  });

  wsClient.send('NodeJS server says, Welcome to free for all chat.');

});

httpsServer.listen(8080);