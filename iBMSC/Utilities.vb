﻿Imports System.Text.RegularExpressions

Namespace Editor
	Public Module Functions
		Public Function WriteDecimalWithDot(v As Double) As String
			Static nfi As New Globalization.NumberFormatInfo With {
				.NumberDecimalSeparator = "."
			}
			Return v.ToString(nfi)
		End Function

		Public Function Add3Zeros(xNum As Integer) As String
			Dim xStr1 As String = "000" & xNum
			Return Mid(xStr1, Len(xStr1) - 2)
		End Function

		Public Function Add2Zeros(xNum As Integer) As String
			Dim xStr1 As String = "00" & xNum
			Return Mid(xStr1, Len(xStr1) - 1)
		End Function

		Public Function C10to36S(xStart As Integer) As Char
			If xStart < 10 Then Return CStr(xStart) Else Return Chr(xStart + 55)
		End Function
		Public Function C36to10S(xChar As Char) As Integer
			Dim xAsc As Integer = Asc(UCase(xChar))
			If xAsc >= 48 And xAsc <= 57 Then
				Return xAsc - 48
			ElseIf xAsc >= 65 And xAsc <= 90 Then
				Return xAsc - 55
			End If
			Return 0
		End Function
		Public Function C10to36(xStart As Long) As String
			If xStart < 0 Then xStart = 0
			If xStart > 1295 Then xStart = 1295
			Return C10to36S(xStart \ 36) & C10to36S(xStart Mod 36)
		End Function
		Public Function C36to10(xStart As String) As Integer
			xStart = Mid("00" & xStart, Len(xStart) + 1)
			Return (C36to10S(xStart.Chars(0)) * 36) + C36to10S(xStart.Chars(1))
		End Function

		Public Function EncodingToString(TextEncoding As Text.Encoding) As String
			Select Case TextEncoding.WebName
				Case "utf-7" : Return "UTF7"
				Case "utf-8" : Return "UTF8"
				Case "utf-16" : Return "Little Endian UTF16"
				Case "utf-16BE" : Return "Big Endian UTF16"
				Case "utf-32" : Return "Little Endian UTF32"
				Case "us-ascii" : Return "ASCII"
				Case "shift_jis" : Return "SJIS"
				Case "euc-kr" : Return "EUC-KR"
				Case Else
					If TextEncoding Is Text.Encoding.Default Then
						Return "System Ansi"
					Else
						Return "ANSI (" & TextEncoding.EncodingName & ")"
					End If
			End Select
		End Function

		''' <summary>
		''' Adjust the brightness of a color.
		''' </summary>
		''' <param name="cStart">Original Color.</param>
		''' <param name="iPercent">(-100 to 100) Brightness.</param>
		''' <param name="iTransparency">(0 - 1) Transparency.</param>
		Public Function AdjustBrightness(cStart As Color, iPercent As Single, iTransparency As Single) As Color
			Return If(cStart.A = 0,
				Color.FromArgb(0),
				Color.FromArgb(
					cStart.A * iTransparency,
					(cStart.R * (100 - Math.Abs(iPercent)) * 0.01) + (Math.Abs(CInt(iPercent >= 0) * iPercent) * 2.55),
					(cStart.G * (100 - Math.Abs(iPercent)) * 0.01) + (Math.Abs(CInt(iPercent >= 0) * iPercent) * 2.55),
					(cStart.B * (100 - Math.Abs(iPercent)) * 0.01) + (Math.Abs(CInt(iPercent >= 0) * iPercent) * 2.55)))
		End Function

		Public Function IdentifiertoLongNote(I As String) As Boolean
			Dim xI As Integer = Val(I)
			Return xI >= 50 And xI < 90
		End Function

		Public Function IdentifiertoHidden(I As String) As Boolean
			Dim xI As Integer = Val(I)
			Return (xI >= 30 And xI < 50) Or (xI >= 70 And xI < 90)
		End Function

		Public Function RandomFileName(extWithDot As String) As String
			Do
				Randomize()
				RandomFileName = Now.Ticks & Mid(Rnd(), 3) & extWithDot
			Loop While File.Exists(RandomFileName) Or Directory.Exists(RandomFileName)
		End Function

		''' <param name="xH">Hue (0-359)</param>
		''' <param name="xS">Saturation (0-1000)</param>
		''' <param name="xL">Lightness (0-1000)</param>
		''' <param name="xA">Alpha (0-255)</param>
		Public Function HSL2RGB(xH As Integer, xS As Integer, xL As Integer, Optional xA As Integer = 255) As Color
			If xH > 360 Or xS > 1000 Or xL > 1000 Or xA > 255 Then Return Color.Black

			'Dim xxH As Double = xH
			Dim xxS As Double = xS / 1000
			Dim xxB As Double = (xL - 500) / 500
			Dim xR As Double
			Dim xG As Double
			Dim xB As Double

			If xH < 60 Then
				xB = -1 : xR = 1 : xG = (xH - 30) / 30
			ElseIf xH < 120 Then
				xB = -1 : xG = 1 : xR = (90 - xH) / 30
			ElseIf xH < 180 Then
				xR = -1 : xG = 1 : xB = (xH - 150) / 30
			ElseIf xH < 240 Then
				xR = -1 : xB = 1 : xG = (210 - xH) / 30
			ElseIf xH < 300 Then
				xG = -1 : xB = 1 : xR = (xH - 270) / 30
			Else
				xG = -1 : xR = 1 : xB = (330 - xH) / 30
			End If

			xR = ((xR * xxS * (1 - Math.Abs(xxB))) + xxB + 1) * 255 / 2
			xG = ((xG * xxS * (1 - Math.Abs(xxB))) + xxB + 1) * 255 / 2
			xB = ((xB * xxS * (1 - Math.Abs(xxB))) + xxB + 1) * 255 / 2

			Return Color.FromArgb(xA, xR, xG, xB)
		End Function

		Public Function FontToString(xFont As Font) As String
			Return xFont.FontFamily.Name & "," & xFont.Size & "," & CInt(xFont.Style)
		End Function

		Public Function isFontInstalled(f As String) As Boolean
			Dim xFontCollection As New Text.InstalledFontCollection
			For Each ff As FontFamily In xFontCollection.Families
				If f.Equals(ff.Name, StringComparison.CurrentCultureIgnoreCase) Then Return True
			Next
			Return False
		End Function


		Public Function StringToFont(xStr As String, xDefault As Font) As Font
			Dim xLine() As String = Split(xStr, ",")
			If UBound(xLine) = 2 Then
				Dim xFontStyle As FontStyle = Val(xLine(2))
				Return New Font(xLine(0), Val(xLine(1)), xFontStyle, GraphicsUnit.Pixel)
			Else
				Return xDefault
			End If
		End Function

		Public Function ArrayToString(xInt() As Integer) As String
			Dim xStr As String = String.Empty
			For xI1 As Integer = 0 To UBound(xInt)
				xStr &= xInt(xI1).ToString & IIf(xI1 = UBound(xInt), String.Empty, ",")
			Next
			Return xStr
		End Function

		Public Function ArrayToString(xBool() As Boolean) As String
			Dim xStr As String = String.Empty
			For xI1 As Integer = 0 To UBound(xBool)
				xStr &= CInt(xBool(xI1)).ToString & IIf(xI1 = UBound(xBool), String.Empty, ",")
			Next
			Return xStr
		End Function

		Public Function ArrayToString(xColor() As Color) As String
			Dim xStr As String = String.Empty
			For xI1 As Integer = 0 To UBound(xColor)
				xStr &= xColor(xI1).ToArgb.ToString & IIf(xI1 = UBound(xColor), String.Empty, ",")
			Next
			Return xStr
		End Function

		Public Function StringToArrayInt(xStr As String) As Integer()
			Dim xL() As String = Split(xStr, ",")
			Dim xInt(UBound(xL)) As Integer
			For xI1 As Integer = 0 To UBound(xInt)
				xInt(xI1) = Val(xL(xI1))
			Next
			Return xInt
		End Function

		Public Function StringToArrayBool(xStr As String) As Boolean()
			Dim xL() As String = Split(xStr, ",")
			Dim xBool(UBound(xL)) As Boolean

			For xI1 As Integer = 0 To UBound(xBool)
				Dim value As String = xL(xI1).Trim()
				xBool(xI1) = (value = "True") Or (value = "1")
			Next

			Return xBool
		End Function


		Public Function GetDenominator(a As Double, Optional maxDenom As Long = &H7FFFFFFF) As Long
			Dim m00 As Long = 1
			Dim m01 As Long = 0
			Dim m10 As Long = 0
			Dim m11 As Long = 1
			Dim x As Double = a
			Dim ai As Long = Int(x)

			Do While (m10 * ai) + m11 <= maxDenom
				Dim t As Long
				t = (m00 * ai) + m01
				m01 = m00
				m00 = t
				t = (m10 * ai) + m11
				m11 = m10
				m10 = t

				If x = ai Then Exit Do
				x = 1 / (x - ai)

				If x > &H7FFFFFFFFFFFFFFF Then Exit Do
				ai = Int(x)
			Loop

			Return m10
		End Function


		Public Function IsBase36(str As String) As Boolean
			Static re As New Regex("^[A-Za-z0-9]+$")
			Return re.IsMatch(str)
		End Function

	End Module
End Namespace
