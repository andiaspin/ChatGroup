Imports System.Drawing.Text
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.Net

Public Class Form1
    Private Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (ByVal lpstrCommand As String, ByVal lpstrReturnString As String, ByVal uReturnLength As Integer, ByVal hwndCallback As Integer) As Integer
    Private clientnya As TCPcontroller
    Dim FileName As String
    Dim pfc As New PrivateFontCollection()
    Dim status As Byte
    Dim clientThread As System.Threading.Thread
    Dim client As New TcpClient
    Dim screen As Screen
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
    Sub startConnect()
        Try
            client = New TcpClient()
            client.Connect(IPAddress.Parse(TextBox3.Text), 8888)
            reader = New BinaryReader(client.GetStream())
            writer = New BinaryWriter(client.GetStream())
            writer.Write("USR  " & TextBox2.Text)
            username = TextBox2.Text
            clientThread = New System.Threading.Thread(AddressOf readSocketClient)
            clientThread.Start()
        Catch ex As Exception
            Exit Sub
        End Try
    End Sub

    Public Sub readSocketClient()
        While True
            Try
                message = reader.ReadString()
                If message.Substring(0, 3) = "MES" Then
                    updateMessage(message.Substring(4))

                End If
                If message = "err" Then
                    MsgBox("Koneksi Bermasalah", MsgBoxStyle.Information, "Informasi")
                    message = reader.ReadString()
                    End
                End If
            Catch e As Exception
                Exit While
            End Try
        End While
    End Sub

    Public Sub writeSocket(ByVal message As String)
        Try
            If client.Connected Then
                writer.Write(message)
            End If
        Catch ex As Exception
            Exit Sub
        End Try
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If Button3.Text = "SIGN IN" Then
            If TextBox2.Text = "" Or TextBox3.Text = "" Then
                MsgBox("Lengkapi Isian", MsgBoxStyle.Exclamation)
            Else
                clientnya = New TCPcontroller(TextBox3.Text, 8080)
                startConnect()
                Button3.Text = "SIGN OUT"
                Button3.FlatAppearance.MouseOverBackColor = Color.Red
                MsgBox("Anda Terhubung Sebagai " + TextBox2.Text, MsgBoxStyle.Information)
                TextBox2.Clear()
                TextBox3.Clear()
            End If

        Else
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
            MsgBox("Anda Sign Out", MsgBoxStyle.Information)
            Button3.Text = "SIGN IN"
            Button3.FlatAppearance.MouseOverBackColor = Color.Green
        End If

    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        If Button4.Text = "SIGN IN" Then
            MsgBox("Anda Harus Sign In Dulu", MessageBoxIcon.Exclamation)
        Else
            If TextBox1.Text = "" Then
                MsgBox("Pesan Tidak Boleh Kosong", MessageBoxIcon.Exclamation)
            Else
                writeSocket("MES:" & username & " : " & TextBox1.Text)
                TextBox1.Text = ""
                TextBox1.Focus()
            End If
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
        Dispose()

    End Sub
End Class
