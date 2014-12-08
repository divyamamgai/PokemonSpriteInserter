﻿Imports System.IO
Imports System.Text

Module FunctionsModule

    Dim RomIdentifierOffset As String = "0000A0"
    Dim RomIdentifierHexValue As String = "504F4B454D4F4E204649524542505245"
    Dim RomIdentifierBytes As Integer = 16
    Dim MaxHexSize As Integer = 65536 ' 0xFFFF + 0x1 in Decimal

    Public Function ToDecimal(ByVal HexValue As String) As Integer
        ToDecimal = Convert.ToInt32(HexValue, 16)
    End Function

    Public Function ToHex(ByVal DecimalValue As Integer) As String
        ToHex = Hex(DecimalValue)
    End Function

    Public Function OffsetToPointer(ByVal Offset As String) As String
        Dim Pointer As String = "Null"
        If Offset.Length = 6 Then
            Pointer = Offset(4) + Offset(5) + Offset(2) + Offset(3) + Offset(0) + Offset(1) + "08"
        End If
        Return Pointer
    End Function

    Public Function PointerToOffset(ByVal Pointer As String) As String
        Dim Offset As String = "Null"
        If Pointer.Length = 8 Then
            Offset = Pointer(4) + Pointer(5) + Pointer(2) + Pointer(3) + Pointer(0) + Pointer(1)
        End If
        Return Offset
    End Function

    Public Function ValidateRom() As Boolean
        If Not String.Compare(ReadData(RomIdentifierOffset, RomIdentifierBytes), RomIdentifierHexValue) Then
            ValidateRom = True
        Else
            ValidateRom = False
        End If
    End Function

    Public Function ReadData(ByVal FromOffset As String, ByVal NumberOfBytes As Integer) As String
        Dim Data As String = ""
        Dim Buffer(NumberOfBytes - 1) As Byte
        Dim RomFileReadStream As FileStream
        RomFileReadStream = File.OpenRead(Form1.RomFilePath)
        RomFileReadStream.Seek(ToDecimal(FromOffset), SeekOrigin.Begin)
        RomFileReadStream.Read(Buffer, 0, NumberOfBytes)
        For x As Integer = 0 To Buffer.Length - 1
            Data += Buffer(x).ToString("X2")
        Next
        RomFileReadStream.Close()
        ReadData = Data
    End Function

    Public Function WriteData(ByVal AtOffset As String,
                              ByVal NumberOfBytes As Integer,
                              ByVal Data As String,
                              Optional ByVal Type As Integer = 0) As Boolean
        Dim RomFileWriteStream As FileStream
WriteDataTry:
        Try
            RomFileWriteStream = File.OpenWrite(Form1.RomFilePath)
            Dim WriteBuffer As Byte()
            WriteBuffer = New Byte(NumberOfBytes - 1) {}
            Dim i As Integer = 0
            Dim k As Integer = 0
            While i < NumberOfBytes
                If Type = 0 Then
                    WriteBuffer(i) = Convert.ToByte((Data(k) & Data(k + 1)), 16)
                    k = k + 2
                Else
                    WriteBuffer(i) = Convert.ToByte(Data, 16)
                End If
                i = i + 1
            End While
            RomFileWriteStream.Seek(ToDecimal(AtOffset), SeekOrigin.Begin)
            RomFileWriteStream.Write(WriteBuffer, 0, NumberOfBytes)
            RomFileWriteStream.Close()
            Return True
        Catch ex As Exception
            Form1.Log.Text += vbCrLf & "Rom File Is In Use! Prompting User To Try Again..."
            Dim DialogBoxResult As Integer = MessageBox.Show("The Rom File Is In Use. Please Close Any Program Using The File And Click Retry To Try Again." & vbCrLf & "[Exception.Message : " + ex.Message + "]", "Error!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation)
            If DialogBoxResult = DialogResult.Retry Then
                Form1.Log.Text += vbCrLf & "Trying Again To Write Data..."
                GoTo WriteDataTry
            Else
                Form1.Log.Text += vbCrLf & "Error! Aborted By User."
                Form1.BackButton.Enabled = True
            End If
        End Try
        Return False
    End Function

    Public Function SearchFreeSpace(ByVal FromOffset As Integer, ByVal NumberOfBytes As Integer, ByVal FreeSpaceString As String) As String
        Dim FreeSpaceByte As Byte = Convert.ToByte(FreeSpaceString, 16)
        Using RomFileBinaryReader As New BinaryReader(File.Open(Form1.RomFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.ASCII)
            Dim Buffer(MaxHexSize - 1) As Byte
            Dim MaxLoop As Integer = CInt(RomFileBinaryReader.BaseStream.Length) / MaxHexSize
            Dim MaxBuffer As Integer = 0
            Dim Match As Boolean = False
            RomFileBinaryReader.BaseStream.Position = FromOffset
            For i As Integer = 0 To MaxLoop - 1
                Buffer = RomFileBinaryReader.ReadBytes(MaxHexSize)
                If Buffer.Length < NumberOfBytes Then
                    Return "Null"
                End If
                MaxBuffer = If(Buffer.Length > NumberOfBytes, (Buffer.Length - NumberOfBytes), 1)
                Dim j As Integer = 0
                While j < MaxBuffer
                    If Buffer(j + (NumberOfBytes - 1)) = FreeSpaceByte Then
                        If Buffer(j) = FreeSpaceByte Then
                            Match = True
                            Dim k As Integer = j + (NumberOfBytes - 2)
                            While k > j
                                If Buffer(k) <> FreeSpaceByte Then
                                    Match = False
                                    Exit While
                                End If
                                k = k - 1
                            End While
                            If Match Then
                                Return ToHex(FromOffset + j + (MaxHexSize * i))
                            End If
                        End If
                    End If
                    j += NumberOfBytes
                End While
            Next
        End Using
        Return "Null"
    End Function

#Region "Validators"

    Public ZeroOffsetCheck As Boolean = False
    Public MaxLimit As Integer = 255

    Public Sub SetZeroOffsetCheckTrue(sender As Object, e As EventArgs)
        ZeroOffsetCheck = True
    End Sub

    Public Sub SetMaxLimitBytes(sender As Object, e As EventArgs)
        If Form1.RomFileLoaded = True Then
            MaxLimit = Form1.RomLength / 2
        End If
    End Sub

    Public Sub SetMaxLimitDefault(sender As Object, e As EventArgs)
        MaxLimit = 255
    End Sub

    Public Sub DigitValidator(sender As Object, e As KeyPressEventArgs)
        If (Microsoft.VisualBasic.Asc(e.KeyChar) < 48) Or (Microsoft.VisualBasic.Asc(e.KeyChar) > 57) Then
            e.Handled = True
        End If
        If (Microsoft.VisualBasic.Asc(e.KeyChar) = 8) Then
            e.Handled = False
        End If
    End Sub

    Public Sub SpaceValidator(sender As Object, e As KeyPressEventArgs)
        If Asc(e.KeyChar) = Keys.Space Then
            e.Handled = True
        End If
    End Sub

    Public Sub NonZeroValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
        If TextBoxItem.Text <> "" Then
            Dim TextBoxValue = Integer.Parse(TextBoxItem.Text)
            If TextBoxValue = 0 Then
                TextBoxItem.Text = TextBoxItem.Tag
                MessageBox.Show("Value cannot be zero!", "Zero Value - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    Public Sub NullValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
            If TextBoxItem.Text = "" Then
                TextBoxItem.Text = TextBoxItem.Tag
                MessageBox.Show("Value cannot be empty!", "Null Value - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
    End Sub

    Public Sub OffsetValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
            If TextBoxItem.Text <> "" Then
                If TextBoxItem.Text.Length < 6 Then
                    TextBoxItem.Text = TextBoxItem.Tag
                    MessageBox.Show("Offset value should atleast be of 6 characters.", "Offset - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Else
                    If Not System.Text.RegularExpressions.Regex.IsMatch(TextBoxItem.Text, "\A\b[0-9a-fA-F]+\b\Z") Then
                        TextBoxItem.Text = TextBoxItem.Tag
                        MessageBox.Show("Enter a valid hexadecimal offset value!", "Offset - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Else
                    If (ToDecimal(TextBoxItem.Text) > Form1.RomLength) _
                        And (Form1.RomFileLoaded = True) Then
                        TextBoxItem.Text = TextBoxItem.Tag
                        MessageBox.Show("Max limit for offset is 0x" + ToHex(Form1.RomLength) + ".", "Offset - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Else
                        If ZeroOffsetCheck = True Then
                            If ToDecimal(TextBoxItem.Text) = 0 Then
                                TextBoxItem.Text = TextBoxItem.Tag
                                MessageBox.Show("Offset cannot be zero!", "Offset - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            End If
                        End If
                    End If
                    End If
                End If
            End If
        ZeroOffsetCheck = False
    End Sub

    Public Sub HexValueValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
        If TextBoxItem.Text <> "" Then
            If TextBoxItem.Text.Length <> TextBoxItem.MaxLength Then
                TextBoxItem.Text = TextBoxItem.Tag
                MessageBox.Show("Value can only be of " + CStr(TextBoxItem.MaxLength) + " characters.", "Hex Value - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                If Not System.Text.RegularExpressions.Regex.IsMatch(TextBoxItem.Text, "\A\b[0-9a-fA-F]+\b\Z") Then
                    TextBoxItem.Text = TextBoxItem.Tag
                    MessageBox.Show("Enter a valid hexadecimal value!", "Hex Value - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        End If
    End Sub

    Public Sub MaxLimitValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
        If TextBoxItem.Text <> "" Then
            Dim TextBoxValue = Integer.Parse(TextBoxItem.Text)
            If TextBoxValue > MaxLimit Then
                TextBoxItem.Text = TextBoxItem.Tag
                MessageBox.Show("Max Limit is " + CStr(MaxLimit) + "!", "Max Limit - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    Public Sub PaletteDataValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
        If TextBoxItem.Text <> "" Then
            If TextBoxItem.Text.Length <> 64 Then
                TextBoxItem.Text = TextBoxItem.Tag
                MessageBox.Show("Palette Hex Data can only be of 64 characters.", "Palette Data - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                If Not System.Text.RegularExpressions.Regex.IsMatch(TextBoxItem.Text, "\A\b[0-9a-fA-F]+\b\Z") Then
                    TextBoxItem.Text = TextBoxItem.Tag
                    MessageBox.Show("Enter a valid hexadecimal palette hex data value!", "Palette Data - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        End If
    End Sub

    Public Sub ByteValidator(sender As Object, e As EventArgs)
        Dim TextBoxItem As TextBox = CType(sender, TextBox)
        If TextBoxItem.Text <> "" Then
            If TextBoxItem.Text.Length <> 2 Then
                TextBoxItem.Text = TextBoxItem.Tag
                MessageBox.Show("Byte value can only be of 2 characters!", "Byte Value - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                If Not System.Text.RegularExpressions.Regex.IsMatch(TextBoxItem.Text, "\A\b[0-9a-fA-F]+\b\Z") Then
                    TextBoxItem.Text = TextBoxItem.Tag
                    MessageBox.Show("Enter a valid hexadecimal byte value!", "Byte Value - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        End If
    End Sub

#End Region

End Module
