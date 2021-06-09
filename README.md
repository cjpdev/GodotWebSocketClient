
# Godot WebSocketClient C# with a NodeJS server

This is a simple example using the Godot WebSocketClient to connect to a NodeJS socket server. 

### Example: Basic socket server using NodeJS: 

This is a none secure server (no SSL\TLS), the WebSocketClient must connect using the protocal "ws://".

```javascript
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', function connection(ws) {

  ws.on('message', function incoming(message) {
    console.log('Godot sent you: %s', message);
  });

  ws.send('NodeJS server says, Hello'); 
});
```

### Example: NodeJS secure server SSL\TLS 

This is a secure server, so the WebSocketClient must connect using the protocal "wss://". 
 * The NodeJS server will send a certificate to the Godot client.

```javascript
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
```

## Certificate
If you do not have a certificate authority (CA), for the server, than you can create a self signed cert. However, these are not valid for any browsers.
But Godot will allow them if you set "webSocketClient.VerifySsl = false";

  * If you don't provide your own cert in the client, Godot will use its (CA) which is from a valid (CA).

* Create self signed cert.
```
openssl req -newkey rsa:2048 -nodes -keyout key.pem -x509 -days 365 -out cert.pem
```
* Convert to crt binary format (maybe needed? if you are going to provide your own cert.)
```
openssl x509 -outform der -in cert.pem -out cert.crt
```

### Example: NodeJS Even more secure server SSL\TLS 

  * "rejectUnauthorized",  If not false the server automatically reject _***clients***_ with invalid certificates.
    * Self signed certificates are always invalid.   
  * "requestCert", If true the server will request a certificate from clients that connect,
    * and attempt to verify the certificate, if rejectUnauthorized is true.

```javascript

optionsHttpServer = {
  cert: fs.readFileSync('./cert/cert.pem'),
  key: fs.readFileSync('./cert/key.pem'),
  rejectUnauthorized: true,
  requestCert: true,
};
```

## Example: Free for all chat server.

A very basic chat server, every client that connects to the server can chat with every one else. 
The client sending the message recive their message back from the server as an echo test.
* Works with the my GodotWebSocketClient example:

<img src="https://github.com/cjpdev/GodotWebSocketClient/blob/main/image.png" alt="modile app" width="400"/>

```javascript
//ES6 
'use strict';

import fs  from 'fs';
import https from 'https';
import webSocket from 'ws';

const options = {
  cert: fs.readFileSync('./cert/cert.pem'),
  key: fs.readFileSync('./cert/key.pem'),
  //rejectUnauthorized: true,
  //requestCert: true,
};

const httpsServer = https.createServer(options, function (req, res) {
  console.log('Client https connection.');
});

const optionsSocket = {
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
```
