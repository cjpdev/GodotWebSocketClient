/**
*
	Copyright (c) 2021 Chay Palton
	Permission is hereby granted, free of charge, to any person obtaining
	a copy of this software and associated documentation files (the "Software"),
	to deal in the Software without restriction, including without limitation
	the rights to use, copy, modify, merge, publish, distribute, sublicense,
	and/or sell copies of the Software, and to permit persons to whom the Software
	is furnished to do so, subject to the following conditions:
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
	EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
	OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
	CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
	TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
	OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Godot;
using System;

public class MainControl : Control
{
    // Optional: If you want to send a specific cert to the server.
    // Else Godot will use its CERT (CA) when doing TLS.
    private string cert = ""; //"res://cert.crt";

    [Export]
    string webSocketURL = "ws://some_server.com:8080"; // For secure SSL use "wss://"

    [Export]
    bool bVerifySsl = false;

    // Optional: This allow you to tell your server what protocol you are using.
    // Just gets added to the Upgrade HEADER request as;
    //
    //      Sec-WebSocket-Protocol: some-protocol
    //
    [Export]
    private String[] supportedProtocols = new string[1] {"some-protocol"};
    WebSocketClient webSocketClient = null;


    Button button = null;
    LineEdit lineEdit = null;
    TextEdit textEdit = null;
    int countTextEditLines = 0;

    public override void _Ready()
    {
        // UI

        button = GetNode<Button>("ButtonConnect");
        
        lineEdit = GetNode<LineEdit>("LineEdit");
        lineEdit.Editable = false;

        textEdit = GetNode<TextEdit>("TextEdit");
        textEdit.HighlightCurrentLine = true;

        // Sockets

        X509Certificate x509Certificate = new X509Certificate();

        webSocketClient = new WebSocketClient();
        
        // Who to handle the cert from the server handshaking.

        // Allow self-signed certificates, not validated with browsers (HTML5 export).
        // However, Godot allows none (CA) certificates.
        webSocketClient.VerifySsl = bVerifySsl;
        
        // If you need more security, use a certificate signed by a 
        // certificate authority (CA), for the server. Then
        // set webSocketClient.VerifySsl = true;

        if(webSocketURL.StartsWith("wss://") && cert.StartsWith("res://")) 
        {
            // Connect using your own CERT, again not validated with browsers (HTML5 export) 
            // There use their own CERT, this CERT is sent to the server.

            // Its upto your server to validate or not the CERT. And to require client
            // to provide a cert or not.

            GD.Print("Using x509 Certificate.");

            Error error = x509Certificate.Load(cert);

            if(error != Error.Ok)
            {
                GD.Print("Could not load cert.");
   
            } else {
                GD.Print("Loaded cert.");
                webSocketClient.TrustedSslCertificate = x509Certificate;
            }
        } 

        webSocketClient.Connect("connection_established", this, nameof(OnConnectionEstablished));
        webSocketClient.Connect("data_received", this, nameof(OnDataRecived));
        webSocketClient.Connect("server_close_request", this, nameof(OnServerCloseRequest));
        webSocketClient.Connect("connection_closed", this, nameof(OnConnectionClosed));
        webSocketClient.Connect("connection_error", this, nameof(OnConnectionError));

    }

    public override void _Process(float delta)
    {
       if (webSocketClient.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected || 
           webSocketClient.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connecting)
        {
            webSocketClient.Poll();
        }
    }

    public void OnButtonPressed()
    {
        if(!webSocketURL.StartsWith("ws"))
        {
            GD.Print("Not websocket address.");
            return;
        }

        if(webSocketClient.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connecting)
        {
            button.Text = "Connecting.";
            GD.Print(button.Text);
            return;
        }

        if(webSocketClient.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
        {
            button.Text = "Send message.";
            SendMessage(lineEdit.Text);
            GD.Print(button.Text);
            return;
        }

        Error error = webSocketClient.ConnectToUrl(webSocketURL, supportedProtocols, false);
        
        if(error != Error.Ok)
        {
            button.Text = "Connect.";
            webSocketClient.GetPeer(1).Close();
    
            GD.Print("Error connect to " + webSocketURL);

        } else {
            button.Text = "Starting connection";
            GD.Print("Starting socket connetion to " + webSocketURL);
        }
    }    

    public void SendMessage(string msg)
    {
        Error error = webSocketClient.GetPeer(1).PutPacket(msg.ToUTF8());
    }

    public void OnConnectionEstablished(string protocol)
    {
        // Example only: Send message to server to say hello.
        
        SendMessage("Just joined, Hi");

        button.Text = "Send message";
        lineEdit.Editable = true;
 
        GD.Print("Connection established.");
    }

    public void OnDataRecived()
    {
        // Example only: Handle reciving text from server.
        var packet = webSocketClient.GetPeer(1).GetPacket();
        bool isString = webSocketClient.GetPeer(1).WasStringPacket();

        if(isString)
        {
            string msg = packet.GetStringFromUTF8();
           
            // Example: Parse string to JSON   
            /*  
                // Try catch will not work for JSON.Parse(msg) errors.

                JSONParseResult parseResult = JSON.Parse(msg);
                parseResult.Error = Error.ParseError;

                if(parseResult.Error == Error.Ok)
                {
                    object obj = parseResult.Result;
                    GD.Print("Server sent you a JSON object " + obj.GetType().ToString());
                }
            */

            GD.Print("Server sent you text: " + msg);
            
            if(countTextEditLines > 0)
            {
                textEdit.Text += "\n"; 
            }

            textEdit.Text += msg;
            textEdit.CursorSetLine(textEdit.GetLineCount(), true);

            countTextEditLines++;
        }
    }

    public void OnServerCloseRequest (int code, string reason)
    {
        lineEdit.Editable = false;
        GD.Print("Close request, reason: " + reason);
    }

    public void OnConnectionClosed (bool wasCleanClose)
    {
        GD.Print("Connection closed. was clean cloase." + wasCleanClose.ToString());
    }

    public void OnConnectionError()
    {
        button.Text = "Connect.";
        lineEdit.Editable = false;
        GD.Print("Connection error.");
    }
}