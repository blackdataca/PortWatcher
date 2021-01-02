### Port Watcher
# Defend your computer against hacker activities. 
Watch TCP IP and UDP ports and notify for inbound and outbound connection attempts. 
# Common ports are 
* TCP port 21 - FTP (File Transfer Protocol)
* TCP port 22 - SSH (Secure Shell)
* TCP port 23 - Telnet
* TCP port 25 - SMTP (Simple Mail Transfer Protocol)
* TCP and UDP port 3389 - RDP (Windows Remote Desktop)

![Screenshot](/img/Capture.PNG)


# Windows: XP+

# Usage example (Administrator rights not required. Can run many instances in any user account.)

```
wp.bat

PortWatcher.exe -p 21,22,23,3389 -e portwatcher@example.com 

```


# IDE: Visual Studio 2019 Community
# .NET Framework: 3.5

```
Usage

PortWatcher -p PortNumber [-e EmailAddress [-s SmtpSerever[:Port] [-user UserName -pass Password] [-ssl] ] ]

-p PortNumber,PortNumber,... = The IP or UDP port number either local or remote. No space.
-e EmailAddress = Email address to receive notification
-s SmtpServer:Port = Smtp server address and port. Default 127.0.0.1:25 if not specified
-user Smtp User Name = User name for Smtp server
-pass Smtp Password = Password for Smtp server
-ssl = Enable SSL for Smtp server

```


# States
State | Description
------------ | -------------
Closed	|	The TCP connection is closed.
CloseWait	|	The local endpoint of the TCP connection is waiting for a connection termination request from the local user.
Closing	|	The local endpoint of the TCP connection is waiting for an acknowledgement of the connection termination request sent previously.
DeleteTcb	|	The transmission control buffer (TCB) for the TCP connection is being deleted.
Established	|	The TCP handshake is complete. The connection has been established and data can be sent.
FinWait1	|	The local endpoint of the TCP connection is waiting for a connection termination request from the remote endpoint or for an acknowledgement of the connection termination request sent previously.
FinWait2	|	The local endpoint of the TCP connection is waiting for a connection termination request from the remote endpoint.
LastAck	|	The local endpoint of the TCP connection is waiting for the final acknowledgement of the connection termination request sent previously.
Listen	|	The local endpoint of the TCP connection is listening for a connection request from any remote endpoint.
SynReceived	|	The local endpoint of the TCP connection has sent and received a connection request and is waiting for an acknowledgment.
SynSent	|	The local endpoint of the TCP connection has sent the remote endpoint a segment header with the synchronize (SYN) control bit set and is waiting for a matching connection request.
TimeWait	|	The local endpoint of the TCP connection is waiting for enough time to pass to ensure that the remote endpoint received the acknowledgement of its connection termination request.
Unknown	|	The TCP connection state is unknown.