Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.IO

Public Class CPcontrolClass
    Public Event MessageReceived(ByVal sender As CPcontrolClass, ByVal Data As String)


    Public serverIP As IPAddress = IPAddress.Parse("127.0.0.1")
    Public serverPORT As Integer = 8080
    Public server As TcpListener

    Private comThread As Thread
    Public isListening As Boolean = True


    Private client As TcpClient
    Private clientData As StreamReader

    Public Sub New()
        server = New TcpListener(serverIP, serverPORT)
        server.Start()
        comThread = New Thread(New ThreadStart(AddressOf Listening))
        comThread.Start()
    End Sub
    Public Sub ipnya()
        Dim addrs As IPAddress
        Dim x As Byte = 0
        Do
            addrs = Dns.GetHostEntry(Dns.GetHostName).AddressList(x)
            x += 1
        Loop Until (addrs.AddressFamily = AddressFamily.InterNetwork)
    End Sub
    Private Sub Listening()

        Do Until isListening = False

            If server.Pending = True Then
                client = server.AcceptTcpClient
                clientData = New StreamReader(client.GetStream)
            End If

            Try
                RaiseEvent MessageReceived(Me, clientData.ReadLine)
                My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Beep)
            Catch ex As Exception

            End Try
            Thread.Sleep(100)
        Loop
    End Sub
End Class
