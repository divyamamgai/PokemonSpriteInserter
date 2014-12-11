﻿Public Class PaletteConvert

    Public TempPaletteData As String = ""
    Public PaletteEditorGroupBox As GroupBox = Nothing
    Public PaletteNumberTextBox As TextBox = Nothing
    Public PaletteHexDataTextBox As TextBox = Nothing
    Public PaletteEditing As Boolean = True
    Public PaletteLabelColor As Color = Color.White

    Public Sub New(Optional ByVal DefaultGroupBox As GroupBox = Nothing,
                   Optional ByVal DefaultNumberTextBox As TextBox = Nothing,
                   Optional ByVal DefaultHexTextBox As TextBox = Nothing,
                   Optional ByVal DefaultPaletteLabelColor As Color = Nothing)
        If IsNothing(DefaultGroupBox) = False Then
            PaletteEditorGroupBox = DefaultGroupBox
        End If
        If IsNothing(DefaultNumberTextBox) = False Then
            PaletteNumberTextBox = DefaultNumberTextBox
        End If
        If IsNothing(DefaultHexTextBox) = False Then
            PaletteHexDataTextBox = DefaultHexTextBox
        End If
        If IsNothing(DefaultPaletteLabelColor) = False Then
            PaletteLabelColor = DefaultPaletteLabelColor
        End If
        AddHandler PaletteHexDataTextBox.TextChanged, AddressOf PaletteHexDataTextBoxTextChanged
    End Sub

    Public Function CheckNull() As Boolean
        If IsNothing(PaletteEditorGroupBox) = True Then
            Return True
        End If
        If IsNothing(PaletteNumberTextBox) = True Then
            Return True
        End If
        If IsNothing(PaletteHexDataTextBox) = True Then
            Return True
        End If
        Return False
    End Function

    Public Function ConvertDecimalToBinary(ByVal Number As Long, ByVal Length As Integer) As String
        Dim Result As String = ""
        Dim LeadingZero As String = ""
        Result = Convert.ToString(Number, 2)
        If Result.Length >= Length Then
            Return Result
        Else
            For i As Integer = 1 To Length - Result.Length
                LeadingZero += "0"
            Next
            Return LeadingZero + Result
        End If
    End Function

    Public Function ConvertBinaryToDecimal(ByVal Binary As String) As Integer
        Dim ResultDecimal As Integer = 0
        For i As Integer = 0 To Binary.Length - 1
            If Binary(i) = "1" Then
                ResultDecimal += Math.Pow(2, (Binary.Length - i - 1))
            End If
        Next
        Return ResultDecimal
    End Function

    Public Function ConvertColorHex(ByVal Color16 As String) As String
        Color16 = Right(Color16, 2) + Left(Color16, 2)
        Dim Result As String = ""
        Dim Red As String = Right(ConvertDecimalToBinary(ToDecimal(Color16), 15), 5)
        Dim Green As String = Left(Right(ConvertDecimalToBinary(ToDecimal(Color16), 15), 10), 5)
        Dim Blue As String = Left(ConvertDecimalToBinary(ToDecimal(Color16), 15), 5)
        Result += ToHex(Math.Round(ConvertBinaryToDecimal(Red) * 255 / 31), 2)
        Result += ToHex(Math.Round(ConvertBinaryToDecimal(Green) * 255 / 31), 2)
        Result += ToHex(Math.Round(ConvertBinaryToDecimal(Blue) * 255 / 31), 2)
        Return Result
    End Function

    Public Function ConvertColor16(ByVal ColorHex As String) As String
        Dim Result As String = ""
        Dim Red As String = ConvertDecimalToBinary(Math.Round(ToDecimal(Left(ColorHex, 2)) * 31 / 255), 2)
        Dim Green As String = ConvertDecimalToBinary(Math.Round(ToDecimal(Right(Left(ColorHex, 4), 2)) * 31 / 255), 2)
        Dim Blue As String = ConvertDecimalToBinary(Math.Round(ToDecimal(Right(ColorHex, 2)) * 31 / 255), 2)
        Result = ToHex(ConvertBinaryToDecimal(Blue + Green + Red), 4)
        Result = Right(Result, 2) + Left(Result, 2)
        Return Result
    End Function

    Public Function ReturnColor(ByVal ColorValue As String, Optional ByVal Is16 As Boolean = True) As Color
        Return ColorTranslator.FromHtml(If(Is16 = True, "#" & ConvertColorHex(ColorValue), "#" & ColorValue))
    End Function

    Public Sub UpdateTempPaletteData(sender As Object, ByVal e As EventArgs)
        If Not IsNothing(PaletteEditorGroupBox) = True Then
            TempPaletteData = ""
            For i As Integer = 0 To 15
                TempPaletteData += GetPaletteTextBox(i).Text
            Next
        End If
    End Sub

    Public Function GetPaletteBox(ByVal Index As Integer) As PaletteBox
        If CheckNull() = False Then
            Dim PaletteBoxContorls = PaletteEditorGroupBox.Controls.OfType(Of PaletteBox)()
            For Each PaletteBoxControl In PaletteBoxContorls
                If CInt(PaletteBoxControl.Tag) = Index Then
                    Return PaletteBoxControl
                End If
            Next
            Return Nothing
        Else
            Return Nothing
        End If
    End Function

    Public Function GetPaletteTextBox(ByVal Index As Integer) As TextBox
        If CheckNull() = False Then
            Dim PaletteTextBoxContorls = PaletteEditorGroupBox.Controls.OfType(Of TextBox)()
            For Each PaletteTextBoxContorl In PaletteTextBoxContorls
                If CInt(PaletteTextBoxContorl.Tag) = Index Then
                    Return PaletteTextBoxContorl
                End If
            Next
            Return Nothing
        Else
            Return Nothing
        End If
    End Function

    Public Function GetColor16Code(ByVal Index As Integer) As String
        If CheckNull() = False Then
            Dim Result = ""
            If Index < PaletteHexDataTextBox.MaxLength / 4 Then
                Result = PaletteHexDataTextBox.Text.Substring(Index * 4, 4)
            End If
            Return Result
        Else
            Return ""
        End If
    End Function

    Public Sub SetPaletteBox(sender As Object, ByVal e As EventArgs)
        If CheckNull() = False Then
            Dim TextBoxElement As TextBox = DirectCast(sender, TextBox)
            If TextBoxElement.Text <> "" Then
                If IsNothing(GetPaletteBox(CInt(TextBoxElement.Tag))) = False Then
                    GetPaletteBox(CInt(TextBoxElement.Tag)).BackColor = ReturnColor(TextBoxElement.Text, True)
                End If
            End If
        End If
    End Sub

    Public Sub PaletteHexDataTextBoxTextChanged(sender As Object, e As EventArgs)
        Try
            If PaletteHexDataTextBox.Text.Length = PaletteHexDataTextBox.MaxLength Then
                GeneratePaletteBox()
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Public Sub PaletteNullValidator(sender As Object, ByVal e As EventArgs)
        If CheckNull() = False Then
            Dim TextBoxElement As TextBox = DirectCast(sender, TextBox)
            If TextBoxElement.Text = "" Then
                If IsNothing(GetPaletteBox(CInt(TextBoxElement.Tag))) = False Then
                    TextBoxElement.Text = GetColor16Code(CInt(TextBoxElement.Tag))
                    GetPaletteBox(CInt(TextBoxElement.Tag)).BackColor = ReturnColor(TextBoxElement.Text, True)
                End If
            End If
        End If
    End Sub

    Public Sub SetPaletteColor(sender As Object, ByVal e As EventArgs)
        If (CheckNull() = False) And (PaletteEditing = True) Then
            Dim PictureBoxElement As PaletteBox = DirectCast(sender, PaletteBox)
            Dim PaletteColorDialog As New ColorDialog
            With PaletteColorDialog
                .FullOpen = True
                .AnyColor = True
            End With
            PaletteColorDialog.Color = PictureBoxElement.BackColor
            If PaletteColorDialog.ShowDialog() <> Windows.Forms.DialogResult.Cancel Then
                PictureBoxElement.BackColor = PaletteColorDialog.Color
                GetPaletteTextBox(CInt(PictureBoxElement.Tag)).Text = ConvertColor16(ToHex(PaletteColorDialog.Color.R, 2) + ToHex(PaletteColorDialog.Color.G, 2) + ToHex(PaletteColorDialog.Color.B, 2))
                UpdateTempPaletteData(sender, e)
            End If
        End If
    End Sub

    Public Sub GeneratePaletteBox()
        If (CheckNull() = False) And (PaletteHexDataTextBox.Text.Length = 64) Then
            TempPaletteData = PaletteHexDataTextBox.Text
            PaletteEditorGroupBox.Controls.Clear()
            Dim PaletteData As String = PaletteHexDataTextBox.Text
            Dim CountRow As Integer = 0
            Dim CountCol As Integer = 0
            For Count As Integer = 0 To 15
                If Count = 8 Then
                    CountRow = CountRow + 1
                    CountCol = 0
                ElseIf Count <> 0 Then
                    CountCol = CountCol + 1
                End If
                Dim TextBoxElement As New TextBox
                Dim PictureBoxElement As New PaletteBox
                With TextBoxElement
                    .Text = PaletteData.Substring(Count * 4, 4)
                    .Location = New Point(6 + CountCol * 66, 32 + CountRow * 52)
                    .Width = 60
                    .BorderStyle = BorderStyle.None
                    .Font = New Font("Calibri", 9, FontStyle.Bold)
                    .MaxLength = 4
                    .BackColor = PaletteLabelColor
                    .CharacterCasing = CharacterCasing.Upper
                    .TextAlign = HorizontalAlignment.Center
                    .Tag = Count
                End With
                With PictureBoxElement
                    .BackColor = ReturnColor(PaletteData.Substring(Count * 4, 4))
                    .Location = New Point(6 + CountCol * 66, 46 + CountRow * 52)
                    .Width = 60
                    .Height = 23
                    .Cursor = If(PaletteEditing = True, Cursors.Hand, Cursors.Arrow)
                    .Tag = Count
                    .BringToFront()
                End With
                AddHandler TextBoxElement.TextChanged, AddressOf SetPaletteBox
                AddHandler TextBoxElement.KeyPress, AddressOf HexInputValidator
                AddHandler TextBoxElement.Leave, AddressOf PaletteNullValidator
                AddHandler TextBoxElement.Leave, AddressOf UpdateTempPaletteData
                PaletteEditorGroupBox.Controls.Add(TextBoxElement)
                AddHandler PictureBoxElement.Click, AddressOf SetPaletteColor
                PaletteEditorGroupBox.Controls.Add(PictureBoxElement)
            Next
            Dim ApplyButton As New Button
            Dim ResetButton As New Button
            Dim ImportButton As New Button
            Dim ExportButton As New Button
            Dim DisplayIndexCheckBox As New CheckBox
            With ApplyButton
                .Text = "Apply"
                .Width = 85
                .Height = 26
                .Location = New Point(443, 140)
                .Enabled = PaletteEditing
            End With
            AddHandler ApplyButton.Click, Sub()
                                              PaletteHexDataTextBox.Text = ""
                                              For i As Integer = 0 To 15
                                                  PaletteHexDataTextBox.Text += GetPaletteTextBox(i).Text
                                              Next
                                          End Sub
            With ResetButton
                .Text = "Reset"
                .Width = 85
                .Height = 26
                .Location = New Point(352, 140)
                .Enabled = PaletteEditing
            End With
            AddHandler ResetButton.Click, Sub()
                                              GeneratePaletteBox()
                                          End Sub
            With ImportButton
                .Text = "Import"
                .Width = 85
                .Height = 26
                .Location = New Point(6, 140)
                .Enabled = PaletteEditing
            End With
            AddHandler ImportButton.Click, Sub()
                                               Dim PaletteImportDialog As FileDialog = New SaveFileDialog
                                               PaletteImportDialog.FileName = "Palette_" + PaletteNumberTextBox.Text
                                               PaletteImportDialog.Filter = "PAL Files (*.pal*)|*.pal"
                                               PaletteImportDialog.Title = "Import Palette"
                                               If PaletteImportDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
                                                   Dim PaletteImportData() As String = My.Computer.FileSystem.ReadAllText(PaletteImportDialog.FileName, System.Text.Encoding.ASCII).Split(New String() {vbCrLf}, StringSplitOptions.None)
                                                   Dim LineCount As Integer = 0
                                                   Dim CompatibleFileFlag As Boolean = True
                                                   If PaletteImportData.LongCount() < 19 Then
                                                       CompatibleFileFlag = False
                                                   Else
                                                       For Each PaletteImportDataLine In PaletteImportData
                                                           LineCount = LineCount + 1
                                                           Select Case LineCount
                                                               Case 1
                                                                   If String.Compare(PaletteImportDataLine, "JASC-PAL") = 0 Then
                                                                       CompatibleFileFlag = True
                                                                   Else
                                                                       CompatibleFileFlag = False
                                                                       Exit For
                                                                   End If
                                                               Case 2
                                                                   If String.Compare(PaletteImportDataLine, "0100") = 0 Then
                                                                       CompatibleFileFlag = True
                                                                   Else
                                                                       CompatibleFileFlag = False
                                                                       Exit For
                                                                   End If
                                                               Case 3
                                                                   If String.Compare(PaletteImportDataLine, "16") = 0 Then
                                                                       CompatibleFileFlag = True
                                                                   Else
                                                                       CompatibleFileFlag = False
                                                                       Exit For
                                                                   End If
                                                               Case Else
                                                                   Dim RGBColor() As String = PaletteImportDataLine.Split(" ")
                                                                   If RGBColor.Length = 3 Then
                                                                       Dim HexColor As String = ""
                                                                       For Each IndividualColor In RGBColor
                                                                           HexColor += ToHex(CInt(IndividualColor), 2)
                                                                       Next
                                                                       GetPaletteBox(LineCount - 4).BackColor = ReturnColor(HexColor, False)
                                                                       GetPaletteTextBox(LineCount - 4).Text = ConvertColor16(HexColor)
                                                                       If (LineCount - 4) = 15 Then
                                                                           CompatibleFileFlag = True
                                                                           Exit For
                                                                       End If
                                                                   Else
                                                                       CompatibleFileFlag = False
                                                                       Exit For
                                                                   End If
                                                           End Select
                                                       Next
                                                   End If
                                                   If CompatibleFileFlag = False Then
                                                       MessageBox.Show("The Palette file you provided seems to be corrupted or incompatible!" & vbCrLf & vbCrLf & " Please select a palette which has been genereated by this program or by program like Irfan View.", "Palette Import - Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                   End If
                                               End If
                                           End Sub
            With ExportButton
                .Text = "Export"
                .Width = 85
                .Height = 26
                .Location = New Point(97, 140)
            End With
            AddHandler ExportButton.Click, Sub()
                                               Dim PaletteFileData As String = ""
                                               PaletteFileData += "JASC-PAL" & vbCrLf
                                               PaletteFileData += "0100" & vbCrLf
                                               PaletteFileData += "16" & vbCrLf
                                               For i As Integer = 0 To CountRow * 8 + CountCol
                                                   PaletteFileData += CStr(GetPaletteBox(i).BackColor.R) + " " + CStr(GetPaletteBox(i).BackColor.G) + " " + CStr(GetPaletteBox(i).BackColor.B) + "" & vbCrLf
                                               Next
                                               Dim PaletteExportDialog As FileDialog = New SaveFileDialog
                                               PaletteExportDialog.FileName = "Palette_" + PaletteNumberTextBox.Text
                                               PaletteExportDialog.Filter = "PAL Files (*.pal*)|*.pal"
                                               PaletteExportDialog.Title = "Export Palette"
                                               If PaletteExportDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
                                                   My.Computer.FileSystem.WriteAllText(PaletteExportDialog.FileName, PaletteFileData, False)
                                               End If
                                           End Sub
            With DisplayIndexCheckBox
                .Text = "Display Palette Index"
                .Location = New Point(200, 142)
                .Width = 200
                .Checked = PaletteBoxIndexDisplayFlag
            End With
            AddHandler DisplayIndexCheckBox.CheckedChanged, Sub(sender As Object, e As EventArgs)
                                                                If DisplayIndexCheckBox.CheckState = CheckState.Checked Then
                                                                    PaletteBoxIndexDisplayFlag = True
                                                                    GeneratePaletteBox()
                                                                Else
                                                                    PaletteBoxIndexDisplayFlag = False
                                                                    GeneratePaletteBox()
                                                                End If
                                                            End Sub
            PaletteEditorGroupBox.Controls.Add(ApplyButton)
            PaletteEditorGroupBox.Controls.Add(ResetButton)
            PaletteEditorGroupBox.Controls.Add(ImportButton)
            PaletteEditorGroupBox.Controls.Add(ExportButton)
            PaletteEditorGroupBox.Controls.Add(DisplayIndexCheckBox)
        End If
    End Sub

End Class