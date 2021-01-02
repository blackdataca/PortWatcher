### Port Watcher
# Defend your computer against hacker activities. 
Watch TCP IP and UDP ports and notify for connections attempts. 
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