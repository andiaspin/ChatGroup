Imports System.IO
Imports System.Net
Imports System.Net.Sockets


Public Class TCPcontroller

    Public client As TcpClient
    Public clientData As StreamWriter

    Public Sub New(ByVal Host As String, ByVal Port As Integer)
        On Error Resume Next

        client = New TcpClient(Host, Port)
        clientData = New StreamWriter(client.GetStream)
    End Sub

    Public Sub send(ByVal Data As String)
        On Error Resume Next
        clientData.Write(Data & vbCrLf)
        clientData.Flush()
    End Sub
End Class



