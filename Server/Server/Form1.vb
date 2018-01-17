Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.IO

Public Class Form1
    Private server As CPcontrolClass
    Dim log As String
    Dim ipserver As IPAddress
    Public Structure ClientData
        Public structSocket As TcpClient
        Public structThread As Thread
        Public structReader As BinaryReader
        Public structWriter As BinaryWriter
    End Structure

    Private TCPListener As TcpListener
    Private clientCollection As New Hashtable()
    Private usernameCollection As New Hashtable()
    Private Shared connectID As Long = 0
    Private serverThread As Thread
    Public Delegate Sub updateListboxDelegate(ByVal str As String)
    Dim clientThread As System.Threading.Thread
    Dim client As New TcpClient
    Dim reader As BinaryReader
    Dim writer As BinaryWriter
    Dim message As String
    Dim username As String
    Delegate Sub updateMessageDelegate(ByVal message As String)

    Sub updateMessageWork(ByVal message As String)
        RichTextBox1.AppendText(message + vbNewLine)
    End Sub
    Sub updateMessage(ByVal message As String)
        If RichTextBox1.InvokeRequired Then
            Invoke(New updateMessageDelegate(AddressOf updateMessageWork), message)
        Else
            RichTextBox1.AppendText(message + vbNewLine)
        End If
    End Sub

    Delegate Sub updateUserDelegate(ByVal message As String)
    Sub updateUserWork(ByVal message As String)
        Dim userlist() As String = Split(message, Chr(13))
        Dim i As Integer
        lbUser.Items.Clear()
        For i = 0 To userlist.Length - 1
            lbUser.Items.Add(userlist(i))
        Next
    End Sub

    Sub updateUser(ByVal message As String)
        If lbUser.InvokeRequired Then
            Invoke(New updateUserDelegate(AddressOf updateUserWork), message)
        End If
    End Sub

    Public Sub readSocket()
        Dim realId As Long = connectID
        Dim CData As New ClientData
        CData = CType(clientCollection(realId), ClientData)
        Dim message As String

        Dim found As Boolean = False

        While True
            If CData.structSocket.Connected Then
                Try
                    message = CData.structReader.ReadString()
                    If message.Substring(0, 3) = "MES" Then
                        For Each Client As ClientData In clientCollection.Values
                            If Client.structSocket.Connected Then
                                Client.structWriter.Write(message)
                            End If
                        Next
                    ElseIf message.Substring(0, 3) = "USR" Then
                        For Each user As String In usernameCollection.Values()
                            If user = message.Substring(4) Then
                                found = True
                            End If
                        Next
                        If found = True Then
                            message = "err"
                            For Each Client As ClientData In clientCollection.Values()
                                If Client.structSocket.Connected Then
                                    Client.structWriter.Write(message)
                                    Exit For
                                End If
                            Next
                            Try
                                Dim client As ClientData = CType(clientCollection(realId), ClientData)
                                client.structThread.Abort()
                            Catch e As Exception
                                SyncLock Me
                                    clientCollection.Remove(realId)
                                    usernameCollection.Remove(realId)
                                End SyncLock
                            End Try
                        Else
                            usernameCollection.Add(realId, message.Substring(4))
                            Dim userlist As String = ""
                            For Each user As String In usernameCollection.Values()
                                userlist = userlist & user & Chr(13)
                            Next
                            For Each Client As ClientData In clientCollection.Values()
                                If Client.structSocket.Connected Then
                                    Client.structWriter.Write("USR " & userlist)
                                End If
                            Next
                        End If
                    ElseIf message.Substring(0, 3) = "DIS" Then
                        usernameCollection.Remove(realId)
                        Dim userlist As String = ""
                        For Each user As String In usernameCollection.Values()
                            userlist = userlist & user & Chr(13)
                        Next

                        For Each Client As ClientData In clientCollection.Values()
                            If Client.structSocket.Connected Then
                                Client.structWriter.Write("USR " & userlist)
                            End If
                        Next
                    End If
                Catch e As Exception
                    Exit While
                End Try
            End If
        End While
        CloseTheThread(realId)
    End Sub
    Private Sub CloseTheThread(ByVal realId As Long)
        Try
            Dim client As ClientData = CType(clientCollection(realId), ClientData)
            client.structThread.Abort()
        Catch e As Exception
            SyncLock Me
                clientCollection.Remove(realId)
                usernameCollection.Remove(realId)
            End SyncLock
        End Try
    End Sub

    Private Sub OnlineReceived(ByVal sender As CPcontrolClass, ByVal Data As String)
    End Sub
    Public Sub readSocketClient()
        While True
            Try
                message = reader.ReadString()
                If message.Substring(0, 3) = "MES" Then
                    updateMessage(message.Substring(4))
                ElseIf message.Substring(0, 3) = "USR" Then
                    updateUser(message.Substring(4))
                ElseIf message.Substring(0, 3) = "DIS" Then
                    updateUser(message.Substring(4))
                End If
                If message = "err" Then
                    Exit Sub
                End If
            Catch e As Exception
                Exit While
            End Try
        End While
    End Sub

    Public Sub writeSocket(ByVal message As String)
        If client.Connected Then
            writer.Write(message)
        End If
    End Sub
    Sub startConnect()
        Try
            client = New TcpClient()
            client.Connect(ipserver, 8888)
            reader = New BinaryReader(client.GetStream())
            writer = New BinaryWriter(client.GetStream())

            writer.Write("USR  " + TextBox1.Text)
            username = TextBox1.Text
            clientThread = New System.Threading.Thread(AddressOf readSocketClient)
            clientThread.Start()
            server = New CPcontrolClass
            AddHandler server.MessageReceived, AddressOf OnlineReceived

            RichTextBox1.Visible = True
            lbUser.Visible = True



        Catch ex As Exception
            Exit Sub
        End Try
    End Sub
    Public Sub New()
        InitializeComponent()
    End Sub
    Public Sub waitingForClient()
        Dim CData As New ClientData
        While True
            CData.structSocket = TCPListener.AcceptTcpClient
            Interlocked.Increment(connectID)
            CData.structThread = New Thread(AddressOf readSocket)
            CData.structReader = New BinaryReader(CData.structSocket.GetStream())
            CData.structWriter = New BinaryWriter(CData.structSocket.GetStream())
            SyncLock Me
                clientCollection.Add(connectID, CData)
            End SyncLock
            CData.structThread.Start()
        End While
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If TextBox1.Text = "" Or TextBox3.Text = "" Then
            MsgBox("Lengkapi Isian", MsgBoxStyle.Exclamation)
        Else
            Dim addr As IPAddress = IPAddress.Parse(TextBox3.Text)
            ipserver = addr
            TCPListener = New TcpListener(addr, 8888)
            TCPListener.Start()
            serverThread = New Thread(AddressOf waitingForClient)
            serverThread.Start()
            startConnect()
            MsgBox("Koneksi Berhasil Sebagai " + TextBox1.Text, MsgBoxStyle.Information)
            TextBox1.Clear()
            TextBox3.Clear()
        End If

    End Sub

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        writeSocket("DIS:")
        If Not (clientThread Is Nothing) Then
            If clientThread.IsAlive Then clientThread.Abort()
        End If
        If Not (reader Is Nothing) Then
            reader.Close()
        End If
        If Not (writer Is Nothing) Then
            writer.Close()
        End If
        If Not (client Is Nothing) Then
            client.Close()
        End If
        Me.Dispose()

    End Sub
End Class
