﻿'Copyright (C) 2016 Wei Gao, Dival Banerjee
'
'This program is free software; you can redistribute it and/or
'modify it under the terms of the GNU General Public License
'as published by the Free Software Foundation; either version 2
'of the License, or (at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
'
'You should have received a copy of the GNU General Public License
'along with this program; if not, write to the Free Software
'Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

Public Class Form1

    Structure Robot
        Dim seg1 As Segment
        Dim seg2 As Segment

        Sub MoveTo(x As Integer, y As Integer)
            Console.WriteLine("moving")
            ' I N V E R S E K I N E M A T I C S S S S S S
            Dim seg1Angle As Double
            Dim seg2Angle As Double

            Dim theta As Double
            ' nan = t r i g g e r e d
            ' still need to figure out how to calculate theta correctly
            theta = Math.Acos(Math.Sqrt((x - seg1.origin.X) ^ 2 + (y - seg1.origin.Y) ^ 2) / (2 * seg1.length))
            seg1Angle = Math.Atan2(y - seg1.origin.Y, x - seg1.origin.X) - theta
            seg2Angle = 2 * theta
            Console.WriteLine(theta & " " & seg1Angle & " " & seg2Angle)
            seg1.moveTo(seg1Angle)
            seg2.setOrigin(seg1.origin.X, seg1.origin.Y)
            seg2.moveTo(seg2Angle + seg1Angle)
            seg1.RecalcPos()
            seg2.RecalcPos()
        End Sub
    End Structure

    Structure Segment
        Dim origin As Point
        Dim [end] As Point
        Dim length As Single
        Dim angle As Double

        Sub setOrigin(x As Integer, y As Integer)
            origin = New Point(x, y)
        End Sub

        Sub setLength(scaleLen As Single)
            length = scaleLen
        End Sub

        Sub moveTo(angle As Double)
            Me.angle = angle
            RecalcPos()
        End Sub

        Sub RecalcPos()
            [end] = New Point(origin.X + length * Math.Cos(angle), origin.Y + length * Math.Sin(angle))
        End Sub

        Sub Draw(g As Graphics, p As Pen)
            g.DrawLine(p, origin, [end])
        End Sub
    End Structure

    Dim seg1 As Segment
    Dim seg2 As Segment
    Dim arm As Robot

    Private Sub TransmitString(str As String)
        ' treat str as a "register" of bytes
        ' 
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PortsListBox.SelectedIndexChanged
        If PortsListBox.SelectedIndex >= 0 Then
            ActiveSerialPort.PortName = PortsListBox.Items(PortsListBox.SelectedIndex)
        End If
        'MsgBox(ActiveSerialPort.PortName)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles ConnectButton.Click
        If PortsListBox.SelectedIndex = -1 Then
            MsgBox("Please select a COM port.", MsgBoxStyle.Exclamation, "No port selected")
        Else
            Dim portName As String = PortsListBox.Items(PortsListBox.SelectedIndex)
            Try
                ActiveSerialPort.PortName = portName
                ActiveSerialPort.Open()
                If ActiveSerialPort.IsOpen Then

                    StatusTextBox.Text = "Connected"
                    MsgBox("Success!")

                End If
            Catch uAccEx As UnauthorizedAccessException
                ' Serial Port is already open by a different process.
                MsgBox("That port is in use by another application.", MsgBoxStyle.Exclamation, "Port busy/access denied")
            Catch argOutOfRangeEx As ArgumentOutOfRangeException
                ' Something is not configured correctly. (Parity, Data/Stop Bits, BaudRate, or timeout invalid)
                MsgBox("The port is not configured correctly. Check the parity, data bits, stop bits, baud rate, or timeout.", MsgBoxStyle.Exclamation, "Misconfigured Port")
            Catch argEx As ArgumentException ' more general than the one above, I guess
                ' more general; port doesn't begin with COM or "file type of port not supported"
                MsgBox("The port is not supported or does not begin with 'COM'.", MsgBoxStyle.Exclamation, "Port not supported")
            Catch ioEx As System.IO.IOException
                ' something goofed while working with the port
                MsgBox("Something went wrong while opening the port. Check your connection to the device.", MsgBoxStyle.Exclamation, "Disconnected")
            Catch InvOpEx As InvalidOperationException
                ' The port was already opened by this process.
                MsgBox("That port is already open here.", MsgBoxStyle.Exclamation, "Already open")
            End Try
        End If
    End Sub

    Private Sub RefreshButton_Click(sender As Object, e As EventArgs) Handles RefreshButton.Click
        PortsListBox.Items.Clear()
        For Each sp As String In My.Computer.Ports.SerialPortNames
            PortsListBox.Items.Add(sp)
        Next
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        ActiveSerialPort.Close()
        If Not ActiveSerialPort.IsOpen Then
            StatusTextBox.Text = "Not Connected"
        End If
    End Sub

    Private Sub SendTestButton_Click(sender As Object, e As EventArgs) Handles SendTestButton.Click
        If ActiveSerialPort.IsOpen() Then
            ActiveSerialPort.Write("VBTEST!")  ' write 8 chars to the stream, so that arduino can verify
        Else
            MsgBox("The device port is closed. Check your connection and reconnect if necessary.", MsgBoxStyle.Exclamation, "Port closed")
        End If
    End Sub

    Sub Redraw(g As Graphics)
        'Dim g As Graphics = PictureBox1.CreateGraphics()
        Dim p As New Pen(Color.Red, 1)
        g.DrawRectangle(p, ScaleReal(25), ScaleReal(50), ScaleReal(25), ScaleReal(25))
        For i As Integer = 1 To 5
            g.DrawEllipse(p, ScaleReal(75 / 2) - ScaleReal(i * 4), ScaleReal(75 / 3) - ScaleReal(i * 4), ScaleReal(i * 8), ScaleReal(i * 8))
        Next
        p.Color = Color.Blue
        seg1.Draw(g, p)
        seg2.Draw(g, p)
    End Sub

    Function ScaleReal(real As Single) As Integer
        Return real * PictureBox1.Width / 75
    End Function

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint
        Redraw(e.Graphics)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        seg1.setOrigin(PictureBox1.Width / 2, PictureBox1.Height * 3 / 4)
        seg1.setLength(ScaleReal(25))
        seg1.RecalcPos()
        seg2.setOrigin(seg1.origin.X, seg1.origin.Y)
        seg2.setLength(seg1.length)
        seg2.RecalcPos()
        arm.seg1 = seg1
        arm.seg2 = seg2
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        'seg1.moveTo(seg1.angle + Math.PI / 360)
        PictureBox1.Refresh()
    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        Dim angle As Double = Math.Atan2(e.Y - seg1.origin.Y, e.X - seg1.origin.X)
        Console.WriteLine(angle)

        arm.MoveTo(e.X - seg1.origin.X, e.Y - seg1.origin.Y)
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub
End Class
