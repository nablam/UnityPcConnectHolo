﻿//https://docs.microsoft.com/en-us/windows/uwp/networking/sockets
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

#if !UNITY_EDITOR
using System.Threading.Tasks;  
#endif

public class TcpServerUWPonly : MonoBehaviour {

    // Every protocol typically has a standard port number. For example, HTTP is typically 80, FTP is 20 and 21, etc.
    // For this example, we'll choose an arbitrary port number.
    static string PortNumber = "50202";

    //protected override void OnNavigatedTo(NavigationEventArgs e)
    //{
    //    this.StartServer();
    //    this.StartClient();
    //}

#if !UNITY_EDITOR
    private async void StartServer()
    {
        try
        {
            var streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();

            // The ConnectionReceived event is raised when connections are received.
            streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

            // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
            await streamSocketListener.BindServiceNameAsync(StreamSocketAndListenerPage.PortNumber);

            this.serverListBox.Items.Add("server is listening...");
        }
        catch (Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            this.serverListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
    }

    private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
    {
        string request;
        using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
        {
            request = await streamReader.ReadLineAsync();
        }

        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));

        // Echo the request back as the response.
        using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
        {
            using (var streamWriter = new StreamWriter(outputStream))
            {
                await streamWriter.WriteLineAsync(request);
                await streamWriter.FlushAsync();
            }
        }

        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));

        sender.Dispose();

        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("server closed its socket"));
    }

    private async void StartClient()
    {
        try
        {
            // Create the StreamSocket and establish a connection to the echo server.
            using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
            {
                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var hostName = new Windows.Networking.HostName("localhost");

                this.clientListBox.Items.Add("client is trying to connect...");

                await streamSocket.ConnectAsync(hostName, StreamSocketAndListenerPage.PortNumber);

                this.clientListBox.Items.Add("client connected");

                // Send a request to the echo server.
                string request = "Hello, World!";
                using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(request);
                        await streamWriter.FlushAsync();
                    }
                }

                this.clientListBox.Items.Add(string.Format("client sent the request: \"{0}\"", request));

                // Read data from the echo server.
                string response;
                using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                {
                    using (StreamReader streamReader = new StreamReader(inputStream))
                    {
                        response = await streamReader.ReadLineAsync();
                    }
                }

                this.clientListBox.Items.Add(string.Format("client received the response: \"{0}\" ", response));
            }

            this.clientListBox.Items.Add("client closed its socket");
        }
        catch (Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            this.clientListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
    }
#endif
}

