Imports iBMSC.Editor

Public Class MainWindow
	'Public Structure MARGINS
	'    Public Left As Integer
	'    Public Right As Integer
	'    Public Top As Integer
	'    Public Bottom As Integer
	'End Structure

	'<System.Runtime.InteropServices.DllImport("dwmapi.dll")> _
	'Public Shared Function DwmIsCompositionEnabled(ByRef en As Integer) As Integer
	'End Function
	'<System.Runtime.InteropServices.DllImport("dwmapi.dll")> _
	'Public Shared Function DwmExtendFrameIntoClientArea( hwnd As IntPtr, ByRef margin As MARGINS) As Integer
	'End Function
	Public Declare Function SendMessage Lib "user32.dll" Alias "SendMessageA" (hwnd As IntPtr, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer
	Public Declare Function ReleaseCapture Lib "user32.dll" Alias "ReleaseCapture" () As Integer

	'Private Declare Auto Function GetWindowLong Lib "user32" ( hWnd As IntPtr,  nIndex As Integer) As Integer
	'Private Declare Auto Function SetWindowLong Lib "user32" ( hWnd As IntPtr,  nIndex As Integer,  dwNewLong As Integer) As Integer
	'Private Declare Function SetWindowPos Lib "user32.dll" ( hWnd As IntPtr,  hWndInsertAfter As IntPtr,  x As Integer,  y As Integer,  cx As Integer,  cy As Integer,  wFlags As Integer) As Integer
	'<DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
	'Private Shared Function SetWindowText( hwnd As IntPtr,  lpString As String) As Boolean
	'End Function

	'Private Const GWL_STYLE As Integer = -16
	'Private Const WS_CAPTION As Integer = &HC00000
	'Private Const SWP_NOSIZE As Integer = &H1
	'Private Const SWP_NOMOVE As Integer = &H2
	'Private Const SWP_NOZORDER As Integer = &H4
	'Private Const SWP_NOACTIVATE As Integer = &H10
	'Private Const SWP_FRAMECHANGED As Integer = &H20
	'Private Const SWP_REFRESH As Integer = SWP_NOZORDER Or SWP_NOSIZE Or SWP_NOMOVE Or SWP_NOACTIVATE Or SWP_FRAMECHANGED


	Private MeasureLength(999) As Double
	Private MeasureBottom(999) As Double

	Public Function MeasureUpper(idx As Integer) As Double
		Return MeasureBottom(idx) + MeasureLength(idx)
	End Function

	Private Notes() As Note = {New Note(niBPM, -1, 1200000, 0, False)}
	Private mColumn(999) As Integer  '0 = no column, 1 = 1 column, etc.
	Private GreatestVPosition As Double    '+ 2000 = -VS.Minimum

	Private VSValue As Integer = 0 'Store value before ValueChange event
	Private HSValue As Integer = 0 'Store value before ValueChange event

	'Dim SortingMethod As Integer = 1
	Private MiddleButtonMoveMethod As Integer = 0
	Private TextEncoding As System.Text.Encoding = System.Text.Encoding.UTF8
	Private DispLang As String = String.Empty     'Display Language
	Private Recent() As String = {String.Empty, String.Empty, String.Empty, String.Empty, String.Empty}
	Private NTInput As Boolean = True
	Private ShowFileName As Boolean = False

	Private BeepWhileSaved As Boolean = True
	Private BPMx1296 As Boolean = False
	Private STOPx1296 As Boolean = False

	Private IsInitializing As Boolean = True
	Private FirstMouseEnter As Boolean = True

	Private WAVMultiSelect As Boolean = True
	Private WAVChangeLabel As Boolean = True
	Private WAVEmptyfill As Boolean = True
	Private BeatChangeMode As Integer = 0

	'Dim FloatTolerance As Double = 0.0001R
	Private BMSGridLimit As Double = 1.0R

	Private LnObj As Integer = 0    '0 for none, 1-1295 for 01-ZZ

	'IO
	Private FileName As String = "Untitled.bms"
	'Dim TitlePath As New Drawing2D.GraphicsPath
	Private InitPath As String = String.Empty
	Private IsSaved As Boolean = True
	Public Structure Chartinfo
		Public Shared Fultitle As String
		Public Shared Allartist As String
	End Structure
	'Variables for Drag/Drop
	Private DDFileName() As String = Array.Empty(Of String)()
	Private SupportedFileExtension() As String = {".bms", ".bme", ".bml", ".pms", ".txt", ".sm", ".ibmsc"}
	Private SupportedAudioExtension() As String = {".wav", ".mp3", ".ogg", ".flac"}
	Private SupportedImageExtension() As String = {".bmp", ".png", ".jpg", ".jpeg", ".gif", ".mpg", ".mpeg", ".avi", ".m1v", ".m2v", ".m4v", ".mp4", ".webm", ".wmv"}

	'Variables for theme
	'Dim SaveTheme As Boolean = True

	'Variables for undo/redo
	Private sUndo(99) As UndoRedo.LinkedURCmd
	Private sRedo(99) As UndoRedo.LinkedURCmd
	Private sI As Integer = 0

	'Variables for select tool
	Private DisableVerticalMove As Boolean = False
	Private KMouseOver As Integer = -1   'Mouse is on which note (for drawing green outline)
	Private LastMouseDownLocation As PointF = New Point(-1, -1)          'Mouse is clicked on which point (location for display) (for selection box)
	Private pMouseMove As PointF = New Point(-1, -1)          'Mouse is moved to which point   (location for display) (for selection box)
	'Dim KMouseDown As Integer = -1   'Mouse is clicked on which note (for moving)
	Private deltaVPosition As Double = 0   'difference between mouse and VPosition of K
	Private bAdjustLength As Boolean     'If adjusting note length instead of moving it
	Private bAdjustUpper As Boolean      'true = Adjusting upper end, false = adjusting lower end
	Private bAdjustSingle As Boolean     'true if there is only one note to be adjusted
	Private tempY As Integer
	Private tempV As Integer
	Private tempX As Integer
	Private tempH As Integer
	Private MiddleButtonLocation As New Point(0, 0)
	Private MiddleButtonClicked As Boolean = False
	Private MouseMoveStatus As New Point(0, 0)  'mouse is moved to which point (For Status Panel)
	'Dim uCol As Integer         'temp variables for undo, original enabled columnindex
	'Dim uVPos As Double         'temp variables for undo, original vposition
	'Dim uPairWithI As Double    'temp variables for undo, original note length
	Private uAdded As Boolean       'temp variables for undo, if undo command is added
	'Dim uNote As Note           'temp variables for undo, original note
	Private SelectedNotes(-1) As Note        'temp notes for undo
	Private ctrlPressed As Boolean = False          'Indicates if the CTRL key is pressed while mousedown
	Private DuplicatedSelectedNotes As Boolean = False     'Indicates if duplicate notes of select/unselect note

	'Variables for write tool
	Private ShouldDrawTempNote As Boolean = False
	Private SelectedColumn As Integer = -1
	Private TempVPosition As Double = -1.0#
	Private TempLength As Double = 0.0#

	'Variables for post effects tool
	Private vSelStart As Double = 192.0#
	Private vSelLength As Double = 0.0#
	Private vSelHalf As Double = 0.0#
	Private vSelMouseOverLine As Integer = 0  '0 = nothing, 1 = start, 2 = half, 3 = end
	Private vSelAdjust As Boolean = False
	Private vSelK() As Note = Array.Empty(Of Note)()
	Private vSelPStart As Double = 192.0#
	Private vSelPLength As Double = 0.0#
	Private vSelPHalf As Double = 0.0#

	'Variables for Full-Screen Mode
	Private isFullScreen As Boolean = False
	Private previousWindowState As FormWindowState = FormWindowState.Normal
	Private previousWindowPosition As New Rectangle(0, 0, 0, 0)

	'Variables misc
	Private menuVPosition As Double = 0.0#
	Private tempResize As Integer = 0

	'----AutoSave Options
	Private PreviousAutoSavedFileName As String = String.Empty
	Private AutoSaveInterval As Integer = 120000

	'----ErrorCheck Options
	Private ErrorCheck As Boolean = True

	'----Header Options
	Private hWAV(1295) As String
	Private hBMP(1295) As String
	Private hBPM(1295) As Long   'x10000
	Private hSTOP(1295) As Long
	Private hBMSCROLL(1295) As Long

	'----Grid Options
	Private gSnap As Boolean = True
	Private gShowGrid As Boolean = True 'Grid
	Private gShowSubGrid As Boolean = True 'Sub
	Private gShowBG As Boolean = True 'BG Color
	Private gShowMeasureNumber As Boolean = True 'Measure Label
	Private gShowVerticalLine As Boolean = True 'Vertical
	Private gShowMeasureBar As Boolean = True 'Measure Barline
	Private gShowC As Boolean = True 'Column Caption
	Private gDivide As Integer = 16
	Private gSub As Integer = 4
	Private gSlash As Integer = 192
	Private gxHeight As Single = 1.0!
	Private gxWidth As Single = 1.0!
	Private gWheel As Integer = 96
	Private gPgUpDn As Integer = 384

	Private gDisplayBGAColumn As Boolean = True
	Private gSCROLL As Boolean = True
	Private gSTOP As Boolean = True
	Private gBPM As Boolean = True
	'Dim gA8 As Boolean = False
	Private iPlayer As Integer = 0
	Private gColumns As Integer = 82

	'----Visual Options
	Private vo As New visualSettings()

	Public Sub setVO(xvo As visualSettings)
		vo = xvo
	End Sub

	'----Preview Options
	Public Structure PlayerArguments
		Public Path As String
		Public aBegin As String
		Public aHere As String
		Public aStop As String
		Public Sub New(xPath As String, xBegin As String, xHere As String, xStop As String)
			Path = xPath
			aBegin = xBegin
			aHere = xHere
			aStop = xStop
		End Sub
	End Structure

	Public pArgs() As PlayerArguments = {New PlayerArguments("<apppath>\uBMplay.exe",
														 "-P -N0 ""<filename>""",
														 "-P -N<measure> ""<filename>""",
														 "-S"),
									 New PlayerArguments("<apppath>\o2play.exe",
														 "-P -N0 ""<filename>""",
														 "-P -N<measure> ""<filename>""",
														 "-S"),
									 New PlayerArguments("java.exe -jar <apppath>\beatoraja.jar",
														 "-a -s""<filename>""",
														 "none1",
														 "none1"
														 )}

	Public CurrentPlayer As Integer = 0
	Private PreviewOnClick As Boolean = True
	Private PreviewErrorCheck As Boolean = False
	Private Rscratch As Boolean = False
	Private ClickStopPreview As Boolean = True
	Private pTempFileNames() As String = Array.Empty(Of String)()

	'----Split Panel Options
	Private PanelWidth() As Single = {0, 100, 0}
	Private PanelhBMSCROLL() As Integer = {0, 0, 0}
	Private PanelVScroll() As Integer = {0, 0, 0}
	Private spLock() As Boolean = {False, False, False}
	Private spDiff() As Integer = {0, 0, 0}
	Private PanelFocus As Integer = 1 '0 = Left, 1 = Middle, 2 = Right
	Private spMouseOver As Integer = 1

	Private AutoFocusMouseEnter As Boolean = False
	Private FirstClickDisabled As Boolean = True
	Private tempFirstMouseDown As Boolean = False

	Private spMain() As Panel = Array.Empty(Of Panel)()

	'----Find Delete Replace Options
	Private fdriMesL As Integer
	Private fdriMesU As Integer
	Private fdriLblL As Integer
	Private fdriLblU As Integer
	Private fdriValL As Integer
	Private fdriValU As Integer
	Private fdriCol() As Integer


	Public Sub New()
		InitializeComponent()
		Initialize()
	End Sub

	''' <summary>
	''' 
	''' </summary>
	''' <param name="xHPosition">Original horizontal position.</param>
	''' <param name="xHSVal">HS.Value</param>


	Private Function HorizontalPositiontoDisplay(xHPosition As Integer, xHSVal As Long) As Integer
		Return (xHPosition * gxWidth) - (xHSVal * gxWidth)
	End Function

	''' <summary>
	''' 
	''' </summary>
	''' <param name="xVPosition">Original vertical position.</param>
	''' <param name="xVSVal">VS.Value</param>
	''' <param name="xTHeight">Height of the panel. (DisplayRectangle, but not ClipRectangle)</param>


	Private Function NoteRowToPanelHeight(xVPosition As Double, xVSVal As Long, xTHeight As Integer) As Integer
		Return xTHeight - CInt((xVPosition + xVSVal) * gxHeight) - 1
	End Function

	Public Function MeasureAtDisplacement(xVPos As Double) As Integer
		'Return Math.Floor((xVPos + FloatTolerance) / 192)
		'Return Math.Floor(xVPos / 192)
		Dim xI1 As Integer
		For xI1 = 1 To 999
			If xVPos < MeasureBottom(xI1) Then Exit For
		Next
		Return xI1 - 1
	End Function

	Private Function GetMaxVPosition() As Double
		Return MeasureUpper(999)
	End Function

	Private Function SnapToGrid(xVPos As Double) As Double
		Dim xOffset As Double = MeasureBottom(MeasureAtDisplacement(xVPos))
		Dim xRatio As Double = 192.0R / gDivide
		Return (Math.Floor((xVPos - xOffset) / xRatio) * xRatio) + xOffset
	End Function

	Private Sub CalculateGreatestVPosition()
		'If K Is Nothing Then Exit Sub
		Dim xI1 As Integer
		GreatestVPosition = 0

		If NTInput Then
			For xI1 = UBound(Notes) To 0 Step -1
				If Notes(xI1).VPosition + Notes(xI1).Length > GreatestVPosition Then GreatestVPosition = Notes(xI1).VPosition + Notes(xI1).Length
			Next
		Else
			For xI1 = UBound(Notes) To 0 Step -1
				If Notes(xI1).VPosition > GreatestVPosition Then GreatestVPosition = Notes(xI1).VPosition
			Next
		End If

		Dim xI2 As Integer = -CInt(IIf(GreatestVPosition + 2000 > GetMaxVPosition(), GetMaxVPosition, GreatestVPosition + 2000))
		MainPanelScroll.Minimum = xI2
		LeftPanelScroll.Minimum = xI2
		RightPanelScroll.Minimum = xI2
	End Sub


	Private Sub SortByVPositionInsertion() 'Insertion Sort
		If UBound(Notes) <= 0 Then Exit Sub
		Dim xNote As Note
		Dim xI1 As Integer
		Dim xI2 As Integer
		For xI1 = 2 To UBound(Notes)
			xNote = Notes(xI1)
			For xI2 = xI1 - 1 To 1 Step -1
				If Notes(xI2).VPosition > xNote.VPosition Then
					Notes(xI2 + 1) = Notes(xI2)
					'                    If KMouseDown = xI2 Then KMouseDown += 1
					If xI2 = 1 Then
						Notes(xI2) = xNote
						'                       If KMouseDown = xI1 Then KMouseDown = xI2
						Exit For
					End If
				Else
					Notes(xI2 + 1) = xNote
					'                    If KMouseDown = xI1 Then KMouseDown = xI2 + 1
					Exit For
				End If
			Next
		Next

	End Sub

	Private Sub SortByVPositionQuick(xMin As Integer, xMax As Integer) 'Quick Sort
		Dim xNote As Note
		Dim iHi As Integer
		Dim iLo As Integer
		Dim xI1 As Integer

		' If min >= max, the list contains 0 or 1 items so it is sorted.
		If xMin >= xMax Then Exit Sub

		' Pick the dividing value.
		xI1 = CInt((xMax - xMin) / 2) + xMin
		xNote = Notes(xI1)

		' Swap it to the front.
		Notes(xI1) = Notes(xMin)

		iLo = xMin
		iHi = xMax
		Do
			' Look down from hi for a value < med_value.
			Do While Notes(iHi).VPosition >= xNote.VPosition
				iHi -= 1
				If iHi <= iLo Then Exit Do
			Loop
			If iHi <= iLo Then
				Notes(iLo) = xNote
				Exit Do
			End If

			' Swap the lo and hi values.
			Notes(iLo) = Notes(iHi)

			' Look up from lo for a value >= med_value.
			iLo += 1
			Do While Notes(iLo).VPosition < xNote.VPosition
				iLo += 1
				If iLo >= iHi Then Exit Do
			Loop
			If iLo >= iHi Then
				iLo = iHi
				Notes(iHi) = xNote
				Exit Do
			End If

			' Swap the lo and hi values.
			Notes(iHi) = Notes(iLo)
		Loop

		' Sort the two sublists.
		SortByVPositionQuick(xMin, iLo - 1)
		SortByVPositionQuick(iLo + 1, xMax)
	End Sub

	Private Sub SortByVPositionQuick3(xMin As Integer, xMax As Integer)
		Dim xxMin As Integer
		Dim xxMax As Integer
		Dim xxMid As Integer
		Dim xNote As Note
		Dim xNoteMid As Note
		Dim xI1 As Integer
		Dim xI2 As Integer
		Dim xI3 As Integer

		'If xMax = 0 Then
		'    xMin = LBound(K1)
		'    xMax = UBound(K1)
		'End If
		xxMin = xMin
		xxMax = xMax
		xxMid = xMax - xMin + 1
		xI1 = CInt(Int(xxMid * Rnd())) + xMin
		xI2 = CInt(Int(xxMid * Rnd())) + xMin
		xI3 = CInt(Int(xxMid * Rnd())) + xMin
		xxMid = If(Notes(xI1).VPosition <= Notes(xI2).VPosition And Notes(xI2).VPosition <= Notes(xI3).VPosition,
		xI2,
		If(Notes(xI2).VPosition <= Notes(xI1).VPosition And Notes(xI1).VPosition <= Notes(xI3).VPosition, xI1, xI3))
		xNoteMid = Notes(xxMid)
		Do
			Do While Notes(xxMin).VPosition < xNoteMid.VPosition And xxMin < xMax
				xxMin += 1
			Loop
			Do While xNoteMid.VPosition < Notes(xxMax).VPosition And xxMax > xMin
				xxMax -= 1
			Loop
			If xxMin <= xxMax Then
				xNote = Notes(xxMin)
				Notes(xxMin) = Notes(xxMax)
				Notes(xxMax) = xNote
				xxMin += 1
				xxMax -= 1
			End If
		Loop Until xxMin > xxMax
		If xxMax - xMin < xMax - xxMin Then
			If xMin < xxMax Then SortByVPositionQuick3(xMin, xxMax)
			If xxMin < xMax Then SortByVPositionQuick3(xxMin, xMax)
		Else
			If xxMin < xMax Then SortByVPositionQuick3(xxMin, xMax)
			If xMin < xxMax Then SortByVPositionQuick3(xMin, xxMax)
		End If
	End Sub


	Private Sub UpdateMeasureBottom()
		MeasureBottom(0) = 0.0#
		For xI1 As Integer = 0 To 998
			MeasureBottom(xI1 + 1) = MeasureBottom(xI1) + MeasureLength(xI1)
		Next
	End Sub

	Private Function PathIsValid(sPath As String) As Boolean
		Return File.Exists(sPath) Or Directory.Exists(sPath)
	End Function

	Public Function PrevCodeToReal(InitStr As String) As String
		Dim xFileName As String = IIf(Not PathIsValid(FileName),
									IIf(InitPath = String.Empty, My.Application.Info.DirectoryPath, InitPath),
									ExcludeFileName(FileName)) _
									& "\___TempBMS.bms"
		Dim xMeasure As Integer = MeasureAtDisplacement(Math.Abs(PanelVScroll(PanelFocus)))
		Dim xS1 As String = Replace(InitStr, "<apppath>", My.Application.Info.DirectoryPath)
		Dim xS2 As String = Replace(xS1, "<measure>", xMeasure)
		Dim xS3 As String = Replace(xS2, "<filename>", xFileName)
		Return xS3
	End Function

	Private Sub SetFileName(xFileName As String)
		FileName = xFileName.Trim
		InitPath = ExcludeFileName(FileName)
		SetIsSaved(IsSaved)
	End Sub

	Private Sub SetIsSaved(isSaved As Boolean)
		'pttl.Refresh()
		'pIsSaved.Visible = Not xBool
		Dim xVersion As String = My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor &
						 IIf(My.Application.Info.Version.Build = 0, String.Empty, "." & My.Application.Info.Version.Build)
		Text = IIf(isSaved, String.Empty, "*") & GetFileName(FileName) & " - " & My.Application.Info.Title & " " & xVersion
		Me.IsSaved = isSaved
	End Sub

	Private Sub PreviewNote(xFileLocation As String, bStop As Boolean)
		If bStop Then
			StopPlaying()
		End If
		Play(xFileLocation)
	End Sub

	Private Sub AddNote(note As Note,
		   Optional xSelected As Boolean = False,
		   Optional OverWrite As Boolean = True,
		   Optional SortAndUpdatePairing As Boolean = True)

		If note.VPosition < 0 Or note.VPosition >= GetMaxVPosition() Then Exit Sub

		Dim xI1 As Integer = 1

		If OverWrite Then
			Do While xI1 <= UBound(Notes)
				If Notes(xI1).VPosition = note.VPosition And
				Notes(xI1).ColumnIndex = note.ColumnIndex Then
					RemoveNote(xI1)
				Else
					xI1 += 1
				End If
			Loop
		End If

		ReDim Preserve Notes(UBound(Notes) + 1)
		note.Selected = note.Selected And nEnabled(note.ColumnIndex)
		Notes(UBound(Notes)) = note

		If TBWavIncrease.Checked Then
			If IsColumnSound(note.ColumnIndex) Then
				IncreaseCurrentWav()
			End If
			If IsColumnImage(note.ColumnIndex) Then
				IncreaseCurrentBmp()
			End If
		End If

		If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()
		CalculateTotalPlayableNotes()
	End Sub

	Private Sub RemoveNote(I As Integer, Optional SortAndUpdatePairing As Boolean = True)
		KMouseOver = -1
		Dim xI2 As Integer

		If TBWavIncrease.Checked Then
			If IsColumnSound(Notes(I).ColumnIndex) Then
				If Notes(I).Value = LWAV.SelectedIndex * 10000 Then
					DecreaseCurrentWav()
				End If
			Else
				If Notes(I).Value = LBMP.SelectedIndex * 10000 Then
					DecreaseCurrentBmp()
				End If
			End If
		End If

		For xI2 = I + 1 To UBound(Notes)
			Notes(xI2 - 1) = Notes(xI2)
		Next
		ReDim Preserve Notes(UBound(Notes) - 1)
		If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()

	End Sub

	Private Sub AddNotesFromClipboard(Optional xSelected As Boolean = True, Optional SortAndUpdatePairing As Boolean = True)
		Dim xStrLine() As String = Split(Clipboard.GetText, vbCrLf)

		Dim xI1 As Integer
		For xI1 = 0 To UBound(Notes)
			Notes(xI1).Selected = False
		Next

		Dim xVS As Long = PanelVScroll(PanelFocus)
		Dim xTempVP As Double
		Dim xKbu() As Note = Notes

		If xStrLine(0) = "iBMSC Clipboard Data" Then
			If NTInput Then ReDim Preserve Notes(0)

			'paste
			Dim xStrSub() As String
			For xI1 = 1 To UBound(xStrLine)
				If xStrLine(xI1).Trim = String.Empty Then Continue For
				xStrSub = Split(xStrLine(xI1), " ")
				xTempVP = Val(xStrSub(1)) + MeasureBottom(MeasureAtDisplacement(-xVS) + 1)
				If UBound(xStrSub) = 5 And xTempVP >= 0 And xTempVP < GetMaxVPosition() Then
					ReDim Preserve Notes(UBound(Notes) + 1)
					With Notes(UBound(Notes))
						.ColumnIndex = Val(xStrSub(0))
						.VPosition = xTempVP
						.Value = Val(xStrSub(2))
						.LongNote = CBool(Val(xStrSub(3)))
						.Hidden = CBool(Val(xStrSub(4)))
						.Landmine = CBool(Val(xStrSub(5)))
						.Selected = xSelected
					End With
				End If
			Next

			'convert
			If NTInput Then
				ConvertBMSE2NT()

				For xI1 = 1 To UBound(Notes)
					Notes(xI1 - 1) = Notes(xI1)
				Next
				ReDim Preserve Notes(UBound(Notes) - 1)

				Dim xKn() As Note = Notes
				Notes = xKbu

				Dim xIStart As Integer = Notes.Length
				ReDim Preserve Notes(UBound(Notes) + xKn.Length)

				For xI1 = xIStart To UBound(Notes)
					Notes(xI1) = xKn(xI1 - xIStart)
				Next
			End If

		ElseIf xStrLine(0) = "iBMSC Clipboard Data xNT" Then
			If Not NTInput Then ReDim Preserve Notes(0)

			'paste
			Dim xStrSub() As String
			For xI1 = 1 To UBound(xStrLine)
				If xStrLine(xI1).Trim = String.Empty Then Continue For
				xStrSub = Split(xStrLine(xI1), " ")
				xTempVP = Val(xStrSub(1)) + MeasureBottom(MeasureAtDisplacement(-xVS) + 1)
				If UBound(xStrSub) = 5 And xTempVP >= 0 And xTempVP < GetMaxVPosition() Then
					ReDim Preserve Notes(UBound(Notes) + 1)
					With Notes(UBound(Notes))
						.ColumnIndex = Val(xStrSub(0))
						.VPosition = xTempVP
						.Value = Val(xStrSub(2))
						.Length = Val(xStrSub(3))
						.Hidden = CBool(Val(xStrSub(4)))
						.Landmine = CBool(Val(xStrSub(5)))
						.Selected = xSelected
					End With
				End If
			Next

			'convert
			If Not NTInput Then
				ConvertNT2BMSE()

				For xI1 = 1 To UBound(Notes)
					Notes(xI1 - 1) = Notes(xI1)
				Next
				ReDim Preserve Notes(UBound(Notes) - 1)

				Dim xKn() As Note = Notes
				Notes = xKbu

				Dim xIStart As Integer = Notes.Length
				ReDim Preserve Notes(UBound(Notes) + xKn.Length)

				For xI1 = xIStart To UBound(Notes)
					Notes(xI1) = xKn(xI1 - xIStart)
				Next
			End If

		ElseIf xStrLine(0) = "BMSE ClipBoard Object Data Format" Then
			If NTInput Then ReDim Preserve Notes(0)

			'paste
			For xI1 = 1 To UBound(xStrLine)
				' zdr: holy crap this is obtuse
				Dim posStr = Mid(xStrLine(xI1), 5, 7)
				Dim vPos = Val(posStr) + MeasureBottom(MeasureAtDisplacement(-xVS) + 1)

				Dim bmsIdent = Mid(xStrLine(xI1), 1, 3)
				Dim lineCol = BMSEChannelToColumnIndex(bmsIdent)

				Dim Value = Val(Mid(xStrLine(xI1), 12)) * 10000

				Dim attribute = Mid(xStrLine(xI1), 4, 1)

				Dim validCol = Len(xStrLine(xI1)) > 11 And lineCol > 0
				Dim inRange = vPos >= 0 And vPos < GetMaxVPosition()
				If validCol And inRange Then
					ReDim Preserve Notes(UBound(Notes) + 1)

					With Notes(UBound(Notes))
						.ColumnIndex = lineCol
						.VPosition = vPos
						.Value = Value
						.LongNote = attribute = "2"
						.Hidden = attribute = "1"
						.Selected = xSelected And nEnabled(.ColumnIndex)
					End With
				End If
			Next

			'convert
			If NTInput Then
				ConvertBMSE2NT()

				For xI1 = 1 To UBound(Notes)
					Notes(xI1 - 1) = Notes(xI1)
				Next
				ReDim Preserve Notes(UBound(Notes) - 1)

				Dim xKn() As Note = Notes
				Notes = xKbu

				Dim xIStart As Integer = Notes.Length
				ReDim Preserve Notes(UBound(Notes) + xKn.Length)

				For xI1 = xIStart To UBound(Notes)
					Notes(xI1) = xKn(xI1 - xIStart)
				Next
			End If
		End If

		If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()
		CalculateTotalPlayableNotes()
	End Sub

	Private Sub CopyNotes(Optional Unselect As Boolean = True)
		Dim xStrAll As String = "iBMSC Clipboard Data" & IIf(NTInput, " xNT", String.Empty)
		Dim xI1 As Integer
		Dim MinMeasure As Double = 999

		For xI1 = 1 To UBound(Notes)
			If Notes(xI1).Selected And MeasureAtDisplacement(Notes(xI1).VPosition) < MinMeasure Then MinMeasure = MeasureAtDisplacement(Notes(xI1).VPosition)
		Next
		MinMeasure = MeasureBottom(MinMeasure)

		If Not NTInput Then
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).Selected Then
					xStrAll &= vbCrLf & Notes(xI1).ColumnIndex.ToString & " " &
								   (Notes(xI1).VPosition - MinMeasure).ToString & " " &
									Notes(xI1).Value.ToString & " " &
							   CInt(Notes(xI1).LongNote).ToString & " " &
							   CInt(Notes(xI1).Hidden).ToString & " " &
							   CInt(Notes(xI1).Landmine).ToString
					Notes(xI1).Selected = Not Unselect
				End If
			Next

		Else
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).Selected Then
					xStrAll &= vbCrLf & Notes(xI1).ColumnIndex.ToString & " " &
								   (Notes(xI1).VPosition - MinMeasure).ToString & " " &
									Notes(xI1).Value.ToString & " " &
									Notes(xI1).Length.ToString & " " &
							   CInt(Notes(xI1).Hidden).ToString & " " &
							   CInt(Notes(xI1).Landmine).ToString
					Notes(xI1).Selected = Not Unselect
				End If
			Next
		End If

		Clipboard.SetText(xStrAll)
	End Sub

	Private Sub RemoveNotes(Optional SortAndUpdatePairing As Boolean = True)
		If UBound(Notes) = 0 Then Exit Sub

		KMouseOver = -1
		Dim xI1 As Integer = 1
		Dim xI2 As Integer
		Do
			If Notes(xI1).Selected Then
				For xI2 = xI1 + 1 To UBound(Notes)
					Notes(xI2 - 1) = Notes(xI2)
				Next
				ReDim Preserve Notes(UBound(Notes) - 1)
				xI1 = 0
			End If
			xI1 += 1
		Loop While xI1 < UBound(Notes) + 1
		If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()
		CalculateTotalPlayableNotes()
	End Sub

	Private Function EnabledColumnIndexToColumnArrayIndex(cEnabled As Integer) As Integer
		Dim xI1 As Integer = 0
		Do
			If xI1 >= gColumns Then Exit Do
			If Not nEnabled(xI1) Then cEnabled += 1
			If xI1 >= cEnabled Then Exit Do
			xI1 += 1
		Loop
		Return cEnabled
	End Function

	Private Function ColumnArrayIndexToEnabledColumnIndex(cReal As Integer) As Integer
		Dim xI1 As Integer
		For xI1 = 0 To cReal - 1
			If Not nEnabled(xI1) Then cReal -= 1
		Next
		Return cReal
	End Function

	Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
		If pTempFileNames IsNot Nothing Then
			For Each xStr In pTempFileNames
				File.Delete(xStr)
			Next
		End If
		If PreviousAutoSavedFileName <> String.Empty Then File.Delete(PreviousAutoSavedFileName)
	End Sub

	Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
		If Not IsSaved Then
			Dim xStr = Strings.Messages.SaveOnExit
			If e.CloseReason = CloseReason.WindowsShutDown Then xStr = Strings.Messages.SaveOnExit1
			If e.CloseReason = CloseReason.TaskManagerClosing Then xStr = Strings.Messages.SaveOnExit2

			Dim xResult = MsgBox(xStr, MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question, Text)

			If xResult = MsgBoxResult.Yes Then
				If ExcludeFileName(FileName) = String.Empty Then
					Dim xDSave As New SaveFileDialog With {
					.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
								Strings.FileType.BMS & "|*.bms|" &
								Strings.FileType.BME & "|*.bme|" &
								Strings.FileType.BML & "|*.bml|" &
								Strings.FileType.PMS & "|*.pms|" &
								Strings.FileType.TXT & "|*.txt|" &
								Strings.FileType._all & "|*.*",
					.DefaultExt = "bms",
					.InitialDirectory = InitPath
				}

					If xDSave.ShowDialog = Forms.DialogResult.Cancel Then e.Cancel = True : Exit Sub
					SetFileName(xDSave.FileName)
				End If
				Dim xStrAll = SaveBMS()
				My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
				NewRecent(FileName)
				If BeepWhileSaved Then Beep()
			End If

			If xResult = MsgBoxResult.Cancel Then e.Cancel = True
		End If

		If Not e.Cancel Then
			'If SaveTheme Then
			'    My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\Skin.cff", SaveSkinCFF, False, System.Text.Encoding.Unicode)
			'Else
			'    My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\Skin.cff", "", False, System.Text.Encoding.Unicode)
			'End If
			'
			'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\PlayerArgs.cff", SavePlayerCFF, False, System.Text.Encoding.Unicode)
			'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\Config.cff", SaveCFF, False, System.Text.Encoding.Unicode)
			'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\PreConfig.cff", "", False, System.Text.Encoding.Unicode)
			SaveSettings(My.Application.Info.DirectoryPath & "\iBMSC.Settings.xml", False)
		End If
	End Sub

	Private Function FilterFileBySupported(xFile() As String, xFilter() As String) As String()
		Dim xPath(-1) As String
		For xI1 As Integer = 0 To UBound(xFile)
			If My.Computer.FileSystem.FileExists(xFile(xI1)) And Array.IndexOf(xFilter, Path.GetExtension(xFile(xI1))) <> -1 Then
				ReDim Preserve xPath(UBound(xPath) + 1)
				xPath(UBound(xPath)) = xFile(xI1)
			End If

			If My.Computer.FileSystem.DirectoryExists(xFile(xI1)) Then
				Dim xFileNames() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(xFile(xI1)).GetFiles()
				For Each xStr As FileInfo In xFileNames
					If Array.IndexOf(xFilter, xStr.Extension) = -1 Then Continue For
					ReDim Preserve xPath(UBound(xPath) + 1)
					xPath(UBound(xPath)) = xStr.FullName
				Next
			End If
		Next

		Return xPath
	End Function

	Private Sub InitializeNewBMS()
		'ReDim K(0)
		'With K(0)
		' .ColumnIndex = niBPM
		' .VPosition = -1
		' .LongNote = False
		' .Selected = False
		' .Value = 1200000
		'End With

		THTitle.Text = String.Empty
		THArtist.Text = String.Empty
		THGenre.Text = String.Empty
		THBPM.Value = 120
		If CHPlayer.SelectedIndex = -1 Then CHPlayer.SelectedIndex = 0
		CHRank.SelectedIndex = 2
		THPlayLevel.Text = String.Empty
		THSubTitle.Text = String.Empty
		THSubArtist.Text = String.Empty
		THStageFile.Text = String.Empty
		THBanner.Text = String.Empty
		THBackBMP.Text = String.Empty
		CHDifficulty.SelectedIndex = 0
		THExRank.Text = String.Empty
		THTotal.Text = String.Empty
		THComment.Text = String.Empty
		'THLnType.Text = "1"
		CHLnObj.SelectedIndex = 0

		THLandMine.Text = String.Empty
		THMissBMP.Text = String.Empty
		TExpansion.Text = String.Empty

		THPreview.Text = String.Empty
		CHLnmode.SelectedIndex = 0

		LBeat.Items.Clear()
		For xI1 As Integer = 0 To 999
			MeasureLength(xI1) = 192.0R
			MeasureBottom(xI1) = xI1 * 192.0R
			Dim unused = LBeat.Items.Add(Add3Zeros(xI1) & ": 1 ( 4 / 4 )")
		Next
	End Sub

	Private Sub InitializeOpenBMS()
		CHPlayer.SelectedIndex = 0
		'THLnType.Text = ""
	End Sub

	Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
			DDFileName = FilterFileBySupported(CType(e.Data.GetData(DataFormats.FileDrop), String()), SupportedFileExtension)
		Else
			e.Effect = DragDropEffects.None
		End If
		RefreshPanelAll()
	End Sub

	Private Sub Form1_DragLeave(sender As Object, e As EventArgs) Handles MyBase.DragLeave
		ReDim DDFileName(-1)
		RefreshPanelAll()
	End Sub

	Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
		ReDim DDFileName(-1)
		If Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Return

		Dim xOrigPath = CType(e.Data.GetData(DataFormats.FileDrop), String())
		Dim xPath = FilterFileBySupported(xOrigPath, SupportedFileExtension)
		If xPath.Length > 0 Then
			Dim xProg As New fLoadFileProgress(xPath, IsSaved)
			Dim unused = xProg.ShowDialog(Me)
		End If

		RefreshPanelAll()
	End Sub

	Private Sub setFullScreen(value As Boolean)
		If value Then
			If WindowState = FormWindowState.Minimized Then Exit Sub

			SuspendLayout()
			previousWindowPosition.Location = Location
			previousWindowPosition.Size = Size
			previousWindowState = WindowState

			WindowState = FormWindowState.Normal
			FormBorderStyle = FormBorderStyle.None
			WindowState = FormWindowState.Maximized
			ToolStripContainer1.TopToolStripPanelVisible = False

			ResumeLayout()
			isFullScreen = True
		Else
			SuspendLayout()
			FormBorderStyle = FormBorderStyle.Sizable
			ToolStripContainer1.TopToolStripPanelVisible = True
			WindowState = FormWindowState.Normal

			WindowState = previousWindowState
			If WindowState = FormWindowState.Normal Then
				Location = previousWindowPosition.Location
				Size = previousWindowPosition.Size
			End If

			ResumeLayout()
			isFullScreen = False
		End If
	End Sub

	Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
		Select Case e.KeyCode
			Case Keys.F11
				setFullScreen(Not isFullScreen)
		End Select
	End Sub

	Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Friend Sub ReadFile(xPath As String)
		Select Case LCase(Path.GetExtension(xPath))
			Case ".bms", ".bme", ".bml", ".pms", ".txt"
				OpenBMS(My.Computer.FileSystem.ReadAllText(xPath, TextEncoding))
				ClearUndo()
				NewRecent(xPath)
				SetFileName(xPath)
				SetIsSaved(True)

			Case ".sm"
				If OpenSM(My.Computer.FileSystem.ReadAllText(xPath, TextEncoding)) Then Return
				InitPath = ExcludeFileName(xPath)
				ClearUndo()
				SetFileName("Untitled.bms")
				SetIsSaved(False)

			Case ".ibmsc"
				OpeniBMSC(xPath)
				InitPath = ExcludeFileName(xPath)
				NewRecent(xPath)
				SetFileName("Imported_" & GetFileName(xPath))
				SetIsSaved(False)

			Case ".bmson"
				Dim unused = MsgBox(Strings.fImportBMSON.Message)
		End Select
	End Sub


	Public Function GCD(NumA As Double, NumB As Double) As Double
		Dim xNMax As Double = NumA
		Dim xNMin As Double = NumB
		If NumA < NumB Then
			xNMax = NumB
			xNMin = NumA
		End If
		Do While xNMin >= BMSGridLimit
			GCD = xNMax - (Math.Floor(xNMax / xNMin) * xNMin)
			xNMax = xNMin
			xNMin = GCD
		Loop
		GCD = xNMax
	End Function
	Public Shared Function GCD(NumA As Double, NumB As Double, res As Integer) As Double
		Dim xNMax As Double = NumA
		Dim xNMin As Double = NumB
		Dim minLimit = 192.0R / res
		If NumA < NumB Then
			xNMax = NumB
			xNMin = NumA
		End If
		Do While xNMin >= minLimit
			GCD = xNMax - (Math.Floor(xNMax / xNMin) * xNMin)
			xNMax = xNMin
			xNMin = GCD
		Loop
		GCD = xNMax
	End Function

	<DllImport("user32.dll")> Private Shared Function LoadCursorFromFile(fileName As String) As IntPtr
	End Function
	Public Shared Function ActuallyLoadCursor(path As String) As Cursor
		Return New Cursor(LoadCursorFromFile(path))
	End Function

	Private Sub Unload()
		Audio.Finalize()
	End Sub

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		'On Error Resume Next
		TopMost = True
		SuspendLayout()
		Visible = False

		'POBMP.Dispose()
		'POBGA.Dispose()

		'Me.MaximizedBounds = Screen.GetWorkingArea(Me)
		'Me.Visible = False
		SetFileName(FileName)
		'Me.ShowCaption = False
		'SetWindowText(Me.Handle.ToInt32, FileName)

		InitializeNewBMS()
		'nBeatD.SelectedIndex = 4

		Try
			Dim xTempFileName As String = RandomFileName(".cur")
			My.Computer.FileSystem.WriteAllBytes(xTempFileName, My.Resources.CursorResizeDown, False)
			Dim xDownCursor As Cursor = ActuallyLoadCursor(xTempFileName)
			My.Computer.FileSystem.WriteAllBytes(xTempFileName, My.Resources.CursorResizeLeft, False)
			Dim xLeftCursor As Cursor = ActuallyLoadCursor(xTempFileName)
			My.Computer.FileSystem.WriteAllBytes(xTempFileName, My.Resources.CursorResizeRight, False)
			Dim xRightCursor As Cursor = ActuallyLoadCursor(xTempFileName)
			File.Delete(xTempFileName)

			POWAVResizer.Cursor = xDownCursor
			POBMPResizer.Cursor = xDownCursor
			POBeatResizer.Cursor = xDownCursor
			POExpansionResizer.Cursor = xDownCursor

			POptionsResizer.Cursor = xLeftCursor

			SpL.Cursor = xRightCursor
			SpR.Cursor = xLeftCursor
		Catch ex As Exception

		End Try

		spMain = New Panel() {PMainInL, PMainIn, PMainInR}

		Dim xI1 As Integer

		sUndo(0) = New UndoRedo.NoOperation
		sUndo(1) = New UndoRedo.NoOperation
		sRedo(0) = New UndoRedo.NoOperation
		sRedo(1) = New UndoRedo.NoOperation
		sI = 0

		LWAV.Items.Clear()
		LBMP.Items.Clear()
		For xI1 = 1 To 1295
			Dim unused2 = LWAV.Items.Add(C10to36(xI1) & ":")
			Dim unused1 = LBMP.Items.Add(C10to36(xI1) & ":")
		Next
		LWAV.SelectedIndex = 0
		LBMP.SelectedIndex = 0
		CHPlayer.SelectedIndex = 0

		CalculateGreatestVPosition()
		TBLangRefresh_Click(TBLangRefresh, Nothing)
		TBThemeRefresh_Click(TBThemeRefresh, Nothing)

		POHeaderPart2.Visible = False
		POGridPart2.Visible = False
		POWaveFormPart2.Visible = False

		If My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\iBMSC.Settings.xml") Then
			LoadSettings(My.Application.Info.DirectoryPath & "\iBMSC.Settings.xml")
			'Else
			'---- Settings for first-time start-up ---------------------------------------------------------------------------
			'Me.LoadLocale(My.Application.Info.DirectoryPath & "\Data\chs.Lang.xml")
			'-----------------------------------------------------------------------------------------------------------------
		End If
		'On Error GoTo 0
		SetIsSaved(True)

		Dim xStr() As String = Environment.GetCommandLineArgs
		'Dim xStr() As String = {Application.ExecutablePath, "C:\Users\User\Desktop\yang run xuan\SoFtwArES\Games\O2Mania\music\SHOOT!\shoot! -NM-.bms"}

		If xStr.Length = 2 Then
			ReadFile(xStr(1))
			If LCase(Path.GetExtension(xStr(1))) = ".ibmsc" AndAlso GetFileName(xStr(1)).StartsWith("AutoSave_", True, Nothing) Then GoTo 1000
		End If

		'pIsSaved.Visible = Not IsSaved
		IsInitializing = False

		If Process.GetProcessesByName(Process.GetCurrentProcess.ProcessName).Length > 1 Then GoTo 1000
		Dim xFiles() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(My.Application.Info.DirectoryPath).GetFiles("AutoSave_*.IBMSC")
		If xFiles Is Nothing OrElse xFiles.Length = 0 Then GoTo 1000

		'Me.TopMost = True
		If MsgBox(Replace(Strings.Messages.RestoreAutosavedFile, "{}", xFiles.Length), MsgBoxStyle.YesNo Or MsgBoxStyle.MsgBoxSetForeground) = MsgBoxResult.Yes Then
			For Each xF As FileInfo In xFiles
				'MsgBox(xF.FullName)
				Dim unused = Process.Start(Application.ExecutablePath, """" & xF.FullName & """")
			Next
		End If

		For Each xF As FileInfo In xFiles
			ReDim Preserve pTempFileNames(UBound(pTempFileNames) + 1)
			pTempFileNames(UBound(pTempFileNames)) = xF.FullName
		Next

1000:
		IsInitializing = False
		POStatusRefresh()
		ResumeLayout()

		tempResize = WindowState
		TopMost = False
		WindowState = tempResize

		Visible = True
	End Sub

	Private Sub UpdatePairing()
		Dim i As Integer, j As Integer

		If NTInput Then
			For i = 0 To UBound(Notes)
				Notes(i).HasError = False
				Notes(i).LNPair = 0
				If Notes(i).Length < 0 Then Notes(i).Length = 0
			Next

			For i = 1 To UBound(Notes)
				If Notes(i).Length <> 0 Then
					For j = i + 1 To UBound(Notes)
						If Notes(j).VPosition > Notes(i).VPosition + Notes(i).Length Then Exit For
						If Notes(j).ColumnIndex = Notes(i).ColumnIndex Then Notes(j).HasError = True
					Next
				Else
					For j = i + 1 To UBound(Notes)
						If Notes(j).VPosition > Notes(i).VPosition Then Exit For
						If Notes(j).ColumnIndex = Notes(i).ColumnIndex Then Notes(j).HasError = True
					Next

					If Notes(i).Value \ 10000 = LnObj AndAlso Not IsColumnNumeric(Notes(i).ColumnIndex) Then
						For j = i - 1 To 1 Step -1
							If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
							If Notes(j).Hidden Then Continue For

							If Notes(j).Length <> 0 OrElse Notes(j).Value \ 10000 = LnObj Then
								Notes(i).HasError = True
							Else
								Notes(i).LNPair = j
								Notes(j).LNPair = i
							End If
							Exit For
						Next
						If j = 0 Then
							Notes(i).HasError = True
						End If
					End If
				End If
			Next

		Else
			For i = 0 To UBound(Notes)
				Notes(i).HasError = False
				Notes(i).LNPair = 0
			Next

			For i = 1 To UBound(Notes)
				If IsColumnSound(Notes(i).ColumnIndex) AndAlso Notes(i).ColumnIndex < niB Then
					If Notes(i).LongNote Then
						'LongNote: If overlapping a note, then error.
						'          Else if already matched by a LongNote below, then match it.
						'          Otherwise match anything above.
						'              If ShortNote above then error on above.
						'          If nothing above then error.
						For j = i - 1 To 1 Step -1
							If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
							If Notes(j).VPosition = Notes(i).VPosition Then
								Notes(i).HasError = True
								GoTo EndSearch
							ElseIf Notes(j).LongNote And Notes(j).LNPair = i Then
								Notes(i).LNPair = j
								GoTo EndSearch
							Else
								Exit For
							End If
						Next

						For j = i + 1 To UBound(Notes)
							If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
							Notes(i).LNPair = j
							Notes(j).LNPair = i
							If Not Notes(j).LongNote AndAlso Notes(j).Value \ 10000 <> LnObj Then
								Notes(j).HasError = True
							End If
							Exit For
						Next

						If j = UBound(Notes) + 1 Then
							Notes(i).HasError = True
						End If
EndSearch:

					ElseIf Notes(i).Value \ 10000 = LnObj Then
						'LnObj: Match anything below.
						'           If matching a LongNote not matching back, then error on below.
						'           If overlapping a note, then error.
						'           If mathcing a LnObj below, then error on below.
						'       If nothing below, then error.
						For j = i - 1 To 1 Step -1
							If Notes(i).ColumnIndex <> Notes(j).ColumnIndex Then Continue For
							If Notes(j).LNPair <> 0 And Notes(j).LNPair <> i Then
								Notes(j).HasError = True
							End If
							Notes(i).LNPair = j
							Notes(j).LNPair = i
							If Notes(i).VPosition = Notes(j).VPosition Then
								Notes(i).HasError = True
							End If
							If Notes(j).Value \ 10000 = LnObj Then
								Notes(j).HasError = True
							End If
							Exit For
						Next

						If j = 0 Then
							Notes(i).HasError = True
						End If
					Else
						'ShortNote: If overlapping a note, then error.
						For j = i - 1 To 1 Step -1
							If Notes(j).VPosition < Notes(i).VPosition Then Exit For
							If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
							Notes(i).HasError = True
							Exit For
						Next
					End If
				Else
					If Notes(i).Hidden Or Notes(i).Landmine Then
						Notes(i).HasError = True
					Else
						'ShortNote: If overlapping a note, then error.
						For j = i - 1 To 1 Step -1
							If Notes(j).VPosition < Notes(i).VPosition Then Exit For
							If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
							Notes(i).HasError = True
							Exit For
						Next
					End If
				End If
			Next


		End If

		Dim currentMS = 0.0#
		Dim currentBPM = Notes(0).Value / 10000
		Dim currentBPMVPosition = 0.0#
		For i = 1 To UBound(Notes)
			If Notes(i).ColumnIndex = niBPM Then
				currentMS += (Notes(i).VPosition - currentBPMVPosition) / currentBPM * 1250
				currentBPM = Notes(i).Value / 10000
				currentBPMVPosition = Notes(i).VPosition
			End If
			'K(i).TimeOffset = currentMS + (K(i).VPosition - currentBPMVPosition) / currentBPM * 1250
		Next
	End Sub



	Public Sub ExceptionSave(Path As String)
		SaveiBMSC(Path)
	End Sub

	''' <summary>
	''' True if pressed cancel. False elsewise.
	''' </summary>
	''' <returns>True if pressed cancel. False elsewise.</returns>

	Private Function ClosingPopSave() As Boolean
		If Not IsSaved Then
			Dim xResult As MsgBoxResult = MsgBox(Strings.Messages.SaveOnExit, MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question, Text)

			If xResult = MsgBoxResult.Yes Then
				If ExcludeFileName(FileName) = String.Empty Then
					Dim xDSave As New SaveFileDialog With {
					.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
								Strings.FileType.BMS & "|*.bms|" &
								Strings.FileType.BME & "|*.bme|" &
								Strings.FileType.BML & "|*.bml|" &
								Strings.FileType.PMS & "|*.pms|" &
								Strings.FileType.TXT & "|*.txt|" &
								Strings.FileType._all & "|*.*",
					.DefaultExt = "bms",
					.InitialDirectory = InitPath
				}

					If xDSave.ShowDialog = Forms.DialogResult.Cancel Then Return True
					SetFileName(xDSave.FileName)
				End If
				Dim xStrAll As String = SaveBMS()
				My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
				NewRecent(FileName)
				If BeepWhileSaved Then Beep()
			End If

			If xResult = MsgBoxResult.Cancel Then Return True
		End If
		Return False
	End Function

	Private Sub TBNew_Click(sender As Object, e As EventArgs) Handles TBNew.Click, mnNew.Click

		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1
		If ClosingPopSave() Then Exit Sub

		ClearUndo()
		InitializeNewBMS()

		ReDim Notes(0)
		ReDim mColumn(999)
		ReDim hWAV(1295)
		ReDim hBMP(1295)
		ReDim hBPM(1295)    'x10000
		ReDim hSTOP(1295)
		ReDim hBMSCROLL(1295)
		THGenre.Text = String.Empty
		THTitle.Text = String.Empty
		THArtist.Text = String.Empty
		THPlayLevel.Text = String.Empty

		With Notes(0)
			.ColumnIndex = niBPM
			.VPosition = -1
			'.LongNote = False
			'.Selected = False
			.Value = 1200000
		End With
		THBPM.Value = 120

		LWAV.Items.Clear()
		LBMP.Items.Clear()
		Dim xI1 As Integer
		For xI1 = 1 To 1295
			Dim unused1 = LWAV.Items.Add(C10to36(xI1) & ": " & hWAV(xI1))
			Dim unused = LBMP.Items.Add(C10to36(xI1) & ": " & hBMP(xI1))
		Next
		LWAV.SelectedIndex = 0
		LBMP.SelectedIndex = 0

		SetFileName("Untitled.bms")
		SetIsSaved(True)
		'pIsSaved.Visible = Not IsSaved

		CalculateTotalPlayableNotes()
		CalculateGreatestVPosition()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub TBNewC_Click(sender As Object, e As EventArgs) 'Handles TBNewC.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1
		If ClosingPopSave() Then Exit Sub

		ClearUndo()

		ReDim Notes(0)
		ReDim mColumn(999)
		ReDim hWAV(1295)
		ReDim hBMP(1295)
		ReDim hBPM(1295)    'x10000
		ReDim hSTOP(1295)
		ReDim hBMSCROLL(1295)
		THGenre.Text = String.Empty
		THTitle.Text = String.Empty
		THArtist.Text = String.Empty
		THPlayLevel.Text = String.Empty

		With Notes(0)
			.ColumnIndex = niBPM
			.VPosition = -1
			'.LongNote = False
			'.Selected = False
			.Value = 1200000
		End With
		THBPM.Value = 120

		SetFileName("Untitled.bms")
		SetIsSaved(True)
		'pIsSaved.Visible = Not IsSaved

		If MsgBox("Please copy your code to clipboard and click OK.", MsgBoxStyle.OkCancel, "Create from code") = MsgBoxResult.Cancel Then Exit Sub
		OpenBMS(Clipboard.GetText)
	End Sub

	Private Sub TBOpen_ButtonClick(sender As Object, e As EventArgs) Handles TBOpen.ButtonClick, mnOpen.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1
		If ClosingPopSave() Then Exit Sub

		Dim xDOpen As New OpenFileDialog With {
		 .Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt",
		 .DefaultExt = "bms",
		 .InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
		 }

		If xDOpen.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		InitPath = ExcludeFileName(xDOpen.FileName)
		OpenBMS(File.ReadAllText(xDOpen.FileName, TextEncoding))
		ClearUndo()
		SetFileName(xDOpen.FileName)
		NewRecent(FileName)
		SetIsSaved(True)
		'pIsSaved.Visible = Not IsSaved
	End Sub

	Private Sub TBImportIBMSC_Click(sender As Object, e As EventArgs) Handles TBImportIBMSC.Click, mnImportIBMSC.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1
		If ClosingPopSave() Then Return

		Dim xDOpen As New OpenFileDialog With {
		.Filter = Strings.FileType.IBMSC & "|*.ibmsc",
		.DefaultExt = "ibmsc",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}

		If xDOpen.ShowDialog = Forms.DialogResult.Cancel Then Return
		InitPath = ExcludeFileName(xDOpen.FileName)
		SetFileName("Imported_" & GetFileName(xDOpen.FileName))
		OpeniBMSC(xDOpen.FileName)
		NewRecent(xDOpen.FileName)
		SetIsSaved(False)
		'pIsSaved.Visible = Not IsSaved
	End Sub

	Private Sub TBImportSM_Click(sender As Object, e As EventArgs) Handles TBImportSM.Click, mnImportSM.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1
		If ClosingPopSave() Then Exit Sub

		Dim xDOpen As New OpenFileDialog With {
		.Filter = Strings.FileType.SM & "|*.sm",
		.DefaultExt = "sm",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}

		If xDOpen.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		If OpenSM(File.ReadAllText(xDOpen.FileName, TextEncoding)) Then Exit Sub
		InitPath = ExcludeFileName(xDOpen.FileName)
		SetFileName("Untitled.bms")
		ClearUndo()
		SetIsSaved(False)
		'pIsSaved.Visible = Not IsSaved
	End Sub

	Private Sub TBSave_ButtonClick(sender As Object, e As EventArgs) Handles TBSave.ButtonClick, mnSave.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1

		If ExcludeFileName(FileName) = String.Empty Then
			Dim xDSave As New SaveFileDialog With {
			.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
						Strings.FileType.BMS & "|*.bms|" &
						Strings.FileType.BME & "|*.bme|" &
						Strings.FileType.BML & "|*.bml|" &
						Strings.FileType.PMS & "|*.pms|" &
						Strings.FileType.TXT & "|*.txt|" &
						Strings.FileType._all & "|*.*",
			.DefaultExt = "bms",
			.InitialDirectory = InitPath
		}

			If xDSave.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
			InitPath = ExcludeFileName(xDSave.FileName)
			SetFileName(xDSave.FileName)
		End If
		Dim xStrAll = SaveBMS()
		My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
		NewRecent(FileName)
		SetFileName(FileName)
		SetIsSaved(True)
		'pIsSaved.Visible = Not IsSaved
		If BeepWhileSaved Then Beep()
	End Sub

	Private Sub TBSaveAs_Click(sender As Object, e As EventArgs) Handles TBSaveAs.Click, mnSaveAs.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1

		Dim xDSave As New SaveFileDialog With {
		.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
					Strings.FileType.BMS & "|*.bms|" &
					Strings.FileType.BME & "|*.bme|" &
					Strings.FileType.BML & "|*.bml|" &
					Strings.FileType.PMS & "|*.pms|" &
					Strings.FileType.TXT & "|*.txt|" &
					Strings.FileType._all & "|*.*",
		.DefaultExt = "bms",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}

		If xDSave.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		InitPath = ExcludeFileName(xDSave.FileName)
		SetFileName(xDSave.FileName)
		Dim xStrAll As String = SaveBMS()
		My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
		NewRecent(FileName)
		SetFileName(FileName)
		SetIsSaved(True)
		'pIsSaved.Visible = Not IsSaved
		If BeepWhileSaved Then Beep()
	End Sub

	Private Sub TBExportIBMSC_Click(sender As Object, e As EventArgs) Handles TBExportIBMSC.Click, mnExportIBMSC.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1

		Dim xDSave As New SaveFileDialog With {
		.Filter = Strings.FileType.IBMSC & "|*.ibmsc",
		.DefaultExt = "ibmsc",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}
		If xDSave.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		SaveiBMSC(xDSave.FileName)
		'My.Computer.FileSystem.WriteAllText(xDSave.FileName, xStrAll, False, TextEncoding)
		NewRecent(FileName)
		If BeepWhileSaved Then Beep()
	End Sub

	Private Sub TBExportBMSON_Click(sender As Object, e As EventArgs) Handles TBExportBMSON.Click, mnExportBMSON.Click
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1

		Dim xDSave As New SaveFileDialog With {
		.Filter = Strings.FileType.BMSON & "|*.bmson",
		.DefaultExt = "bmson",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}
		If xDSave.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		SaveBMSON(xDSave.FileName)
		'My.Computer.FileSystem.WriteAllText(xDSave.FileName, xStrAll, False, TextEncoding)
		NewRecent(FileName)
		If BeepWhileSaved Then Beep()
	End Sub

	Private Sub VSGotFocus(sender As Object, e As EventArgs) Handles MainPanelScroll.GotFocus, LeftPanelScroll.GotFocus, RightPanelScroll.GotFocus
		PanelFocus = sender.Tag
		Dim unused = spMain(PanelFocus).Focus()
	End Sub

	Private Sub VSValueChanged(sender As Object, e As EventArgs) Handles MainPanelScroll.ValueChanged, LeftPanelScroll.ValueChanged, RightPanelScroll.ValueChanged
		Dim iI As Integer = sender.Tag

		' az: We got a wheel event when we're zooming in/out
		If My.Computer.Keyboard.CtrlKeyDown Then
			sender.Value = VSValue ' Undo the scroll
			Exit Sub
		End If

		If iI = PanelFocus And Not LastMouseDownLocation = New Point(-1, -1) And Not VSValue = -1 Then LastMouseDownLocation.Y += (VSValue - sender.Value) * gxHeight
		PanelVScroll(iI) = sender.Value

		If spLock((iI + 1) Mod 3) Then
			Dim xVS As Integer = PanelVScroll(iI) + spDiff(iI)
			If xVS > 0 Then xVS = 0
			If xVS < MainPanelScroll.Minimum Then xVS = MainPanelScroll.Minimum
			Select Case iI
				Case 0 : MainPanelScroll.Value = xVS
				Case 1 : RightPanelScroll.Value = xVS
				Case 2 : LeftPanelScroll.Value = xVS
			End Select
		End If

		If spLock((iI + 2) Mod 3) Then
			Dim xVS As Integer = PanelVScroll(iI) - spDiff((iI + 2) Mod 3)
			If xVS > 0 Then xVS = 0
			If xVS < MainPanelScroll.Minimum Then xVS = MainPanelScroll.Minimum
			Select Case iI
				Case 0 : RightPanelScroll.Value = xVS
				Case 1 : LeftPanelScroll.Value = xVS
				Case 2 : MainPanelScroll.Value = xVS
			End Select
		End If

		spDiff(iI) = PanelVScroll((iI + 1) Mod 3) - PanelVScroll(iI)
		spDiff((iI + 2) Mod 3) = PanelVScroll(iI) - PanelVScroll((iI + 2) Mod 3)

		VSValue = sender.Value
		RefreshPanel(iI, spMain(iI).DisplayRectangle)
	End Sub

	Private Sub cVSLock_CheckedChanged(sender As Object, e As EventArgs) Handles cVSLockL.CheckedChanged, cVSLock.CheckedChanged, cVSLockR.CheckedChanged
		Dim iI As Integer = sender.Tag
		spLock(iI) = sender.Checked
		If Not spLock(iI) Then Return

		spDiff(iI) = PanelVScroll((iI + 1) Mod 3) - PanelVScroll(iI)
		spDiff((iI + 2) Mod 3) = PanelVScroll(iI) - PanelVScroll((iI + 2) Mod 3)

		'POHeaderB.Text = spDiff(0) & "_" & spDiff(1) & "_" & spDiff(2)
	End Sub

	Private Sub HSGotFocus(sender As Object, e As EventArgs) Handles HS.GotFocus, HSL.GotFocus, HSR.GotFocus
		PanelFocus = sender.Tag
		Dim unused = spMain(PanelFocus).Focus()
	End Sub

	Private Sub HSValueChanged(sender As Object, e As EventArgs) Handles HS.ValueChanged, HSL.ValueChanged, HSR.ValueChanged
		Dim iI As Integer = sender.Tag
		If Not LastMouseDownLocation = New Point(-1, -1) And Not HSValue = -1 Then LastMouseDownLocation.X += (HSValue - sender.Value) * gxWidth
		PanelhBMSCROLL(iI) = sender.Value
		HSValue = sender.Value
		RefreshPanel(iI, spMain(iI).DisplayRectangle)
	End Sub

	Private Sub TBSelect_Click(sender As Object, e As EventArgs) Handles TBSelect.Click, mnSelect.Click
		TBSelect.Checked = True
		TBWrite.Checked = False
		TBTimeSelect.Checked = False
		mnSelect.Checked = True
		mnWrite.Checked = False
		mnTimeSelect.Checked = False

		FStatus2.Visible = False
		FStatus.Visible = True

		ShouldDrawTempNote = False
		SelectedColumn = -1
		TempVPosition = -1
		TempLength = 0

		vSelStart = MeasureBottom(MeasureAtDisplacement(-PanelVScroll(PanelFocus)) + 1)
		vSelLength = 0

		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub TBWrite_Click(sender As Object, e As EventArgs) Handles TBWrite.Click, mnWrite.Click
		TBSelect.Checked = False
		TBWrite.Checked = True
		TBTimeSelect.Checked = False
		mnSelect.Checked = False
		mnWrite.Checked = True
		mnTimeSelect.Checked = False

		FStatus2.Visible = False
		FStatus.Visible = True

		ShouldDrawTempNote = True
		SelectedColumn = -1
		TempVPosition = -1
		TempLength = 0

		vSelStart = MeasureBottom(MeasureAtDisplacement(-PanelVScroll(PanelFocus)) + 1)
		vSelLength = 0

		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub TBPostEffects_Click(sender As Object, e As EventArgs) Handles TBTimeSelect.Click, mnTimeSelect.Click
		TBSelect.Checked = False
		TBWrite.Checked = False
		TBTimeSelect.Checked = True
		mnSelect.Checked = False
		mnWrite.Checked = False
		mnTimeSelect.Checked = True

		FStatus.Visible = False
		FStatus2.Visible = True

		vSelMouseOverLine = 0
		ShouldDrawTempNote = False
		SelectedColumn = -1
		TempVPosition = -1
		TempLength = 0
		ValidateSelection()

		Dim xI1 As Integer
		For xI1 = 0 To UBound(Notes)
			Notes(xI1).Selected = False
		Next
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub CGHeight_ValueChanged(sender As Object, e As EventArgs) Handles CGHeight.ValueChanged
		gxHeight = CGHeight.Value
		CGHeight2.Value = IIf(CGHeight.Value * 4 < CGHeight2.Maximum, CGHeight.Value * 4, CGHeight2.Maximum)
		RefreshPanelAll()
	End Sub

	Private Sub CGHeight2_Scroll(sender As Object, e As EventArgs) Handles CGHeight2.Scroll
		CGHeight.Value = CGHeight2.Value / 4
	End Sub

	Private Sub CGWidth_ValueChanged(sender As Object, e As EventArgs) Handles CGWidth.ValueChanged
		gxWidth = CGWidth.Value
		CGWidth2.Value = IIf(CGWidth.Value * 4 < CGWidth2.Maximum, CGWidth.Value * 4, CGWidth2.Maximum)

		HS.LargeChange = PMainIn.Width / gxWidth
		If HS.Value > HS.Maximum - HS.LargeChange + 1 Then HS.Value = HS.Maximum - HS.LargeChange + 1
		HSL.LargeChange = PMainInL.Width / gxWidth
		If HSL.Value > HSL.Maximum - HSL.LargeChange + 1 Then HSL.Value = HSL.Maximum - HSL.LargeChange + 1
		HSR.LargeChange = PMainInR.Width / gxWidth
		If HSR.Value > HSR.Maximum - HSR.LargeChange + 1 Then HSR.Value = HSR.Maximum - HSR.LargeChange + 1

		RefreshPanelAll()
	End Sub

	Private Sub CGWidth2_Scroll(sender As Object, e As EventArgs) Handles CGWidth2.Scroll
		CGWidth.Value = CGWidth2.Value / 4
	End Sub

	Private Sub CGDivide_ValueChanged(sender As Object, e As EventArgs) Handles CGDivide.ValueChanged
		gDivide = CGDivide.Value
		RefreshPanelAll()
	End Sub
	Private Sub CGSub_ValueChanged(sender As Object, e As EventArgs) Handles CGSub.ValueChanged
		gSub = CGSub.Value
		RefreshPanelAll()
	End Sub
	Private Sub BGSlash_Click(sender As Object, e As EventArgs) Handles BGSlash.Click
		Dim xd As Integer = Val(InputBox(Strings.Messages.PromptSlashValue, , gSlash))
		If xd = 0 Then Exit Sub
		If xd > CGDivide.Maximum Then xd = CGDivide.Maximum
		If xd < CGDivide.Minimum Then xd = CGDivide.Minimum
		gSlash = xd
	End Sub


	Private Sub CGSnap_CheckedChanged(sender As Object, e As EventArgs) Handles CGSnap.CheckedChanged
		gSnap = CGSnap.Checked
		RefreshPanelAll()
	End Sub

	Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
		Dim xI1 As Integer

		Select Case PanelFocus
			Case 0
				With LeftPanelScroll
					xI1 = .Value + (tempY / 5 / gxHeight)
					If xI1 > 0 Then xI1 = 0
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
				With HSL
					xI1 = .Value + (tempX / 10 / gxWidth)
					If xI1 > .Maximum - .LargeChange + 1 Then xI1 = .Maximum - .LargeChange + 1
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With

			Case 1
				With MainPanelScroll
					xI1 = .Value + (tempY / 5 / gxHeight)
					If xI1 > 0 Then xI1 = 0
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
				With HS
					xI1 = .Value + (tempX / 10 / gxWidth)
					If xI1 > .Maximum - .LargeChange + 1 Then xI1 = .Maximum - .LargeChange + 1
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With

			Case 2
				With RightPanelScroll
					xI1 = .Value + (tempY / 5 / gxHeight)
					If xI1 > 0 Then xI1 = 0
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
				With HSR
					xI1 = .Value + (tempX / 10 / gxWidth)
					If xI1 > .Maximum - .LargeChange + 1 Then xI1 = .Maximum - .LargeChange + 1
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
		End Select

		Dim xMEArgs As New MouseEventArgs(MouseButtons.Left, 0, MouseMoveStatus.X, MouseMoveStatus.Y, 0)
		PMainInMouseMove(spMain(PanelFocus), xMEArgs)

	End Sub

	Private Sub TimerMiddle_Tick(sender As Object, e As EventArgs) Handles TimerMiddle.Tick
		If Not MiddleButtonClicked Then TimerMiddle.Enabled = False : Return

		Dim xI1 As Integer

		Select Case PanelFocus
			Case 0
				With LeftPanelScroll
					xI1 = .Value + ((Cursor.Position.Y - MiddleButtonLocation.Y) / 5 / gxHeight)
					If xI1 > 0 Then xI1 = 0
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
				With HSL
					xI1 = .Value + ((Cursor.Position.X - MiddleButtonLocation.X) / 5 / gxWidth)
					If xI1 > .Maximum - .LargeChange + 1 Then xI1 = .Maximum - .LargeChange + 1
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With

			Case 1
				With MainPanelScroll
					xI1 = .Value + ((Cursor.Position.Y - MiddleButtonLocation.Y) / 5 / gxHeight)
					If xI1 > 0 Then xI1 = 0
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
				With HS
					xI1 = .Value + ((Cursor.Position.X - MiddleButtonLocation.X) / 5 / gxWidth)
					If xI1 > .Maximum - .LargeChange + 1 Then xI1 = .Maximum - .LargeChange + 1
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With

			Case 2
				With RightPanelScroll
					xI1 = .Value + ((Cursor.Position.Y - MiddleButtonLocation.Y) / 5 / gxHeight)
					If xI1 > 0 Then xI1 = 0
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
				With HSR
					xI1 = .Value + ((Cursor.Position.X - MiddleButtonLocation.X) / 5 / gxWidth)
					If xI1 > .Maximum - .LargeChange + 1 Then xI1 = .Maximum - .LargeChange + 1
					If xI1 < .Minimum Then xI1 = .Minimum
					.Value = xI1
				End With
		End Select

		Dim xMEArgs As New MouseEventArgs(MouseButtons.Left, 0, MouseMoveStatus.X, MouseMoveStatus.Y, 0)
		PMainInMouseMove(spMain(PanelFocus), xMEArgs)
	End Sub

	Private Sub ValidateWavListView()
		Try
			Dim xRect As Rectangle = LWAV.GetItemRectangle(LWAV.SelectedIndex)
			If xRect.Top + xRect.Height > LWAV.DisplayRectangle.Height Then Dim unused = SendMessage(LWAV.Handle, &H115, 1, 0)
		Catch ex As Exception
		End Try
	End Sub

	Private Sub ValidateBmpListView()
		Try
			Dim xRect As Rectangle = LBMP.GetItemRectangle(LBMP.SelectedIndex)
			If xRect.Top + xRect.Height > LBMP.DisplayRectangle.Height Then Dim unused = SendMessage(LBMP.Handle, &H115, 1, 0)
		Catch ex As Exception
		End Try
	End Sub

	Private Sub LWAV_Click(sender As Object, e As EventArgs) Handles LWAV.Click
		If TBWrite.Checked Then FSW.Text = C10to36(LWAV.SelectedIndex + 1)

		PreviewNote(String.Empty, True)
		If Not PreviewOnClick Then Exit Sub
		If hWAV(LWAV.SelectedIndex + 1) = String.Empty Then Exit Sub

		Dim xFileLocation As String = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName)) & "\" & hWAV(LWAV.SelectedIndex + 1)
		PreviewNote(xFileLocation, False)
	End Sub

	Private Sub LWAV_DoubleClick(sender As Object, e As EventArgs) Handles LWAV.DoubleClick
		Dim xDWAV As New OpenFileDialog With {
		.DefaultExt = "wav",
		.Filter = Strings.FileType._wave & "|*.wav;*.ogg;*.mp3;*.flac|" &
				   Strings.FileType.WAV & "|*.wav|" &
				   Strings.FileType.OGG & "|*.ogg|" &
				   Strings.FileType.MP3 & "|*.mp3|" &
				   Strings.FileType.FLAC & "|*.flac|" &
				   Strings.FileType._all & "|*.*",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}

		If xDWAV.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		InitPath = ExcludeFileName(xDWAV.FileName)
		hWAV(LWAV.SelectedIndex + 1) = GetFileName(xDWAV.FileName)
		LWAV.Items.Item(LWAV.SelectedIndex) = C10to36(LWAV.SelectedIndex + 1) & ": " & GetFileName(xDWAV.FileName)
		If IsSaved Then SetIsSaved(False)
	End Sub

	Private Sub LWAV_KeyDown(sender As Object, e As KeyEventArgs) Handles LWAV.KeyDown
		Select Case e.KeyCode
			Case Keys.Delete
				hWAV(LWAV.SelectedIndex + 1) = String.Empty
				LWAV.Items.Item(LWAV.SelectedIndex) = C10to36(LWAV.SelectedIndex + 1) & ": "
				If IsSaved Then SetIsSaved(False)
		End Select
	End Sub

	Private Sub LBMP_DoubleClick(sender As Object, e As EventArgs) Handles LBMP.DoubleClick
		Dim xDBMP As New OpenFileDialog With {
		.DefaultExt = "bmp",
		.Filter = Strings.FileType._image & "|*.bmp;*.png;*.jpg;*.jpeg;.gif|" &
				   Strings.FileType._movie & "|*.mpg;*.m1v;*.m2v;*.avi;*.mp4;*.m4v;*.wmv;*.webm|" &
				   Strings.FileType.BMP & "|*.bmp|" &
				   Strings.FileType.PNG & "|*.png|" &
				   Strings.FileType.JPG & "|*.jpg;*.jpeg|" &
				   Strings.FileType.GIF & "|*.gif|" &
				   Strings.FileType.MP4 & "|*.mp4;*.m4v|" &
				   Strings.FileType.AVI & "|*.avi|" &
				   Strings.FileType.MPG & "|*.mpg;*.m1v;*.m2v|" &
				   Strings.FileType.WMV & "|*.wmv|" &
				   Strings.FileType.WEBM & "|*.webm|" &
				   Strings.FileType._all & "|*.*",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName))
	}

		If xDBMP.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		InitPath = ExcludeFileName(xDBMP.FileName)
		hBMP(LBMP.SelectedIndex + 1) = GetFileName(xDBMP.FileName)
		LBMP.Items.Item(LBMP.SelectedIndex) = C10to36(LBMP.SelectedIndex + 1) & ": " & GetFileName(xDBMP.FileName)
		If IsSaved Then SetIsSaved(False)
	End Sub

	Private Sub LBMP_KeyDown(sender As Object, e As KeyEventArgs) Handles LBMP.KeyDown
		Select Case e.KeyCode
			Case Keys.Delete
				hBMP(LBMP.SelectedIndex + 1) = String.Empty
				LBMP.Items.Item(LBMP.SelectedIndex) = C10to36(LBMP.SelectedIndex + 1) & ": "
				If IsSaved Then SetIsSaved(False)
		End Select
	End Sub

	Private Sub TBErrorCheck_Click(sender As Object, e As EventArgs) Handles TBErrorCheck.Click, mnErrorCheck.Click
		ErrorCheck = sender.Checked
		TBErrorCheck.Checked = ErrorCheck
		mnErrorCheck.Checked = ErrorCheck
		TBErrorCheck.Image = IIf(TBErrorCheck.Checked, My.Resources.x16CheckError, My.Resources.x16CheckErrorN)
		mnErrorCheck.Image = IIf(TBErrorCheck.Checked, My.Resources.x16CheckError, My.Resources.x16CheckErrorN)
		RefreshPanelAll()
	End Sub

	Private Sub TBPreviewOnClick_Click(sender As Object, e As EventArgs) Handles TBPreviewOnClick.Click, mnPreviewOnClick.Click
		PreviewNote(String.Empty, True)
		PreviewOnClick = sender.Checked
		TBPreviewOnClick.Checked = PreviewOnClick
		mnPreviewOnClick.Checked = PreviewOnClick
		TBPreviewOnClick.Image = IIf(PreviewOnClick, My.Resources.x16PreviewOnClick, My.Resources.x16PreviewOnClickN)
		mnPreviewOnClick.Image = IIf(PreviewOnClick, My.Resources.x16PreviewOnClick, My.Resources.x16PreviewOnClickN)
	End Sub

	Private Sub TBChangePlaySide_Click(sender As Object, e As EventArgs) Handles TBChangePlaySide.Click, mnChangePlaySide.Click
		Rscratch = sender.Checked
		ChangePlaySide(True)
		TBChangePlaySide.Checked = Rscratch
		mnChangePlaySide.Checked = Rscratch
		TBChangePlaySide.Image = My.Resources.x16ChangePlaySide
		mnChangePlaySide.Image = My.Resources.x16ChangePlaySide
		RefreshPanelAll()
	End Sub

	'Private Sub TBPreviewErrorCheck_Click( sender As System.Object,  e As EventArgs)
	'    PreviewErrorCheck = TBPreviewErrorCheck.Checked
	'    TBPreviewErrorCheck.Image = IIf(PreviewErrorCheck, My.Resources.x16PreviewCheck, My.Resources.x16PreviewCheckN)
	'End Sub

	Private Sub TBShowFileName_Click(sender As Object, e As EventArgs) Handles TBShowFileName.Click, mnShowFileName.Click
		ShowFileName = sender.Checked
		TBShowFileName.Checked = ShowFileName
		mnShowFileName.Checked = ShowFileName
		TBShowFileName.Image = IIf(ShowFileName, My.Resources.x16ShowFileName, My.Resources.x16ShowFileNameN)
		mnShowFileName.Image = IIf(ShowFileName, My.Resources.x16ShowFileName, My.Resources.x16ShowFileNameN)
		RefreshPanelAll()
	End Sub

	Private Sub TBCut_Click(sender As Object, e As EventArgs) Handles TBCut.Click, mnCut.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
		RedoRemoveNoteSelected(True, xUndo, xRedo)
		'Dim xRedo As String = sCmdKDs()
		'Dim xUndo As String = sCmdKs(True)

		Try
			CopyNotes(False)
			RemoveNotes(False)
			AddUndo(xUndo, xBaseRedo.Next)
		Catch ex As Exception
			Dim unused = MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
		End Try
		SortByVPositionInsertion()
		UpdatePairing()
		CalculateTotalPlayableNotes()
		RefreshPanelAll()
		POStatusRefresh()
		CalculateGreatestVPosition()
	End Sub

	Private Sub TBCopy_Click(sender As Object, e As EventArgs) Handles TBCopy.Click, mnCopy.Click
		Try
			CopyNotes()
		Catch ex As Exception
			Dim unused = MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
		End Try
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub TBPaste_Click(sender As Object, e As EventArgs) Handles TBPaste.Click, mnPaste.Click
		AddNotesFromClipboard()

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
		RedoAddNoteSelected(True, xUndo, xRedo)
		AddUndo(xUndo, xBaseRedo.Next)

		'AddUndo(sCmdKDs(), sCmdKs(True))

		SortByVPositionInsertion()
		UpdatePairing()
		CalculateTotalPlayableNotes()
		RefreshPanelAll()
		POStatusRefresh()
		CalculateGreatestVPosition()
	End Sub

	'Private Function pArgPath( I As Integer)
	'    Return Mid(pArgs(I), 1, InStr(pArgs(I), vbCrLf) - 1)
	'End Function

	Private Function GetFileName(s As String) As String
		Dim fslash As Integer = InStrRev(s, "/")
		Dim bslash As Integer = InStrRev(s, "\")

		Return Mid(s, IIf(fslash > bslash, fslash, bslash) + 1)

	End Function

	Private Function ExcludeFileName(s As String) As String
		Dim fslash As Integer = InStrRev(s, "/")
		Dim bslash As Integer = InStrRev(s, "\")
		If (bslash Or fslash) = 0 Then Return String.Empty
		Return Mid(s, 1, IIf(fslash > bslash, fslash, bslash) - 1)
	End Function

	Private Sub PlayerMissingPrompt()
		Dim xArg As PlayerArguments = pArgs(CurrentPlayer)
		Dim unused = MsgBox(Strings.Messages.CannotFind.Replace("{}", PrevCodeToReal(xArg.Path)) & vbCrLf &
		   Strings.Messages.PleaseRespecifyPath, MsgBoxStyle.Critical, Strings.Messages.PlayerNotFound)

		Dim xDOpen As New OpenFileDialog With {
		.InitialDirectory = IIf(ExcludeFileName(PrevCodeToReal(xArg.Path)) = String.Empty,
								  My.Application.Info.DirectoryPath,
								  ExcludeFileName(PrevCodeToReal(xArg.Path))),
		.FileName = PrevCodeToReal(xArg.Path),
		.Filter = Strings.FileType.EXE & "|*.exe",
		.DefaultExt = "exe"
	}
		If xDOpen.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		'pArgs(CurrentPlayer) = Replace(xDOpen.FileName, My.Application.Info.DirectoryPath, "<apppath>") & _
		'                                           Mid(pArgs(CurrentPlayer), InStr(pArgs(CurrentPlayer), vbCrLf))
		'xStr = Split(pArgs(CurrentPlayer), vbCrLf)
		pArgs(CurrentPlayer).Path = Replace(xDOpen.FileName, My.Application.Info.DirectoryPath, "<apppath>")
		xArg = pArgs(CurrentPlayer)
	End Sub

	Private Sub TBPlay_Click(sender As Object, e As EventArgs) Handles TBPlay.Click, mnPlay.Click
		'Dim xStr() As String = Split(pArgs(CurrentPlayer), vbCrLf)
		Dim xArg As PlayerArguments = pArgs(CurrentPlayer)

		If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
			PlayerMissingPrompt()
			xArg = pArgs(CurrentPlayer)
		End If

		' az: Treat it like we cancelled the operation
		If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
			Exit Sub
		End If

		Dim xStrAll As String = SaveBMS()
		Dim xFileName As String = IIf(Not PathIsValid(FileName),
								  IIf(InitPath = String.Empty, My.Application.Info.DirectoryPath, InitPath),
								  ExcludeFileName(FileName)) & "\___TempBMS.bms"
		My.Computer.FileSystem.WriteAllText(xFileName, xStrAll, False, TextEncoding)

		AddTempFileList(xFileName)
		Dim unused = Process.Start(PrevCodeToReal(xArg.Path), PrevCodeToReal(xArg.aHere))
	End Sub

	Private Sub TBPlayB_Click(sender As Object, e As EventArgs) Handles TBPlayB.Click, mnPlayB.Click
		'Dim xStr() As String = Split(pArgs(CurrentPlayer), vbCrLf)
		Dim xArg As PlayerArguments = pArgs(CurrentPlayer)

		If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
			PlayerMissingPrompt()
			xArg = pArgs(CurrentPlayer)
		End If

		If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
			Exit Sub
		End If

		Dim xStrAll As String = SaveBMS()
		Dim xFileName As String = IIf(Not PathIsValid(FileName),
								  IIf(InitPath = String.Empty, My.Application.Info.DirectoryPath, InitPath),
								  ExcludeFileName(FileName)) & "\___TempBMS.bms"
		My.Computer.FileSystem.WriteAllText(xFileName, xStrAll, False, TextEncoding)

		AddTempFileList(xFileName)

		Dim unused = Process.Start(PrevCodeToReal(xArg.Path), PrevCodeToReal(xArg.aBegin))
	End Sub

	Private Sub TBStop_Click(sender As Object, e As EventArgs) Handles TBStop.Click, mnStop.Click
		'Dim xStr() As String = Split(pArgs(CurrentPlayer), vbCrLf)
		Dim xArg As PlayerArguments = pArgs(CurrentPlayer)

		If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
			PlayerMissingPrompt()
			xArg = pArgs(CurrentPlayer)
		End If

		If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
			Exit Sub
		End If

		Dim unused = Process.Start(PrevCodeToReal(xArg.Path), PrevCodeToReal(xArg.aStop))
	End Sub

	Private Sub AddTempFileList(s As String)
		Dim xAdd As Boolean = True
		If pTempFileNames IsNot Nothing Then
			For Each xStr1 As String In pTempFileNames
				If xStr1 = s Then xAdd = False : Exit For
			Next
		End If

		If xAdd Then
			ReDim Preserve pTempFileNames(UBound(pTempFileNames) + 1)
			pTempFileNames(UBound(pTempFileNames)) = s
		End If
	End Sub
	Private Sub TBinfo_Click(sender As Object, e As EventArgs) Handles mnStatistics.Click, TBStatistics.Click
		My.Forms.Chartinfo.Show()
	End Sub
	Public Sub TBStatistics_Click(sender As Object, e As EventArgs)
		SortByVPositionInsertion()
		UpdatePairing()

		Dim data(6, 5) As Integer
		For i = 1 To UBound(Notes)
			With Notes(i)
				Dim row = -1
				If .ColumnIndex = niBPM Then
					row = 0
				ElseIf .ColumnIndex = niSTOP Then
					row = 1
				ElseIf .ColumnIndex = niSCROLL Then
					row = 2
				ElseIf .ColumnIndex >= niA1 AndAlso .ColumnIndex <= niAQ Then
					row = 3
				ElseIf .ColumnIndex >= niD1 AndAlso .ColumnIndex <= niDQ Then
					row = 4
				ElseIf .ColumnIndex >= niB Then
					row = 5
				Else
					row = 6
				End If


StartCount:     If Not NTInput Then
					If Not .LongNote Then data(row, 0) += 1
					If .LongNote Then data(row, 1) += 1
					If .Value \ 10000 = LnObj Then data(row, 1) += 1
					If .Hidden Then data(row, 2) += 1
					If .Landmine Then data(row, 3) += 1
					If .HasError Then data(row, 4) += 1
					data(row, 5) += 1

				Else
					Dim noteUnit = 1
					If .Length = 0 Then data(row, 0) += 1
					If .Length <> 0 Then data(row, 1) += 2 : noteUnit = 2

					If .Value \ 10000 = LnObj Then data(row, 1) += noteUnit
					If .Hidden Then data(row, 2) += noteUnit
					If .Landmine Then data(row, 3) += noteUnit
					If .HasError Then data(row, 4) += noteUnit
					data(row, 5) += noteUnit

				End If

				If row <> 6 Then row = 6 : GoTo StartCount
			End With
		Next

		Dim dStat As New dgStatistics(data)
		Dim unused = dStat.ShowDialog
	End Sub

	''' <summary>
	''' Remark: Pls sort and updatepairing before this process.
	''' </summary>

	Private Sub CalculateTotalPlayableNotes()
		Dim xI1 As Integer
		Dim xIAll As Integer = 0

		If Not NTInput Then
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).ColumnIndex >= niA1 AndAlso Notes(xI1).ColumnIndex <= niAQ Then xIAll += 1
				If Notes(xI1).ColumnIndex >= niD1 AndAlso Notes(xI1).ColumnIndex <= niDQ AndAlso column(Notes(xI1).ColumnIndex).isVisible Then xIAll += 1
			Next

		Else
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).ColumnIndex >= niA1 And Notes(xI1).ColumnIndex <= niAQ Then
					xIAll += 1
					If Notes(xI1).Length <> 0 Then xIAll += 1
				End If
				If Notes(xI1).ColumnIndex >= niD1 And Notes(xI1).ColumnIndex <= niDQ AndAlso column(Notes(xI1).ColumnIndex).isVisible Then
					xIAll += 1
					If Notes(xI1).Length <> 0 Then xIAll += 1
				End If
			Next

		End If

		TBStatistics.Text = xIAll
	End Sub

	Private Function CalculateTotalNotes() As Integer
		Dim xI1 As Integer
		Dim xIAll As Integer = 0

		For xI1 = 1 To UBound(Notes)
			If Notes(xI1).ColumnIndex >= niA1 AndAlso Notes(xI1).ColumnIndex <= niAQ AndAlso
			   Not Notes(xI1).Hidden AndAlso Not Notes(xI1).Landmine AndAlso column(Notes(xI1).ColumnIndex).isVisible Then
				xIAll += 1
			End If
			If Notes(xI1).ColumnIndex >= niD1 AndAlso Notes(xI1).ColumnIndex <= niDQ AndAlso
			   Not Notes(xI1).Hidden AndAlso Not Notes(xI1).Landmine AndAlso column(Notes(xI1).ColumnIndex).isVisible Then
				xIAll += 1
			End If
		Next

		Return xIAll
	End Function

	Public Function GetMouseVPosition(Optional snap As Boolean = True)
		Dim panHeight = spMain(PanelFocus).Height
		Dim panDisplacement = PanelVScroll(PanelFocus)
		Dim vpos = (panHeight - (panDisplacement * gxHeight) - MouseMoveStatus.Y - 1) / gxHeight
		Return If(snap, SnapToGrid(vpos), DirectCast(vpos, Object))
	End Function

	Private Sub POStatusRefresh()

		If TBSelect.Checked Then
			Dim xI1 As Integer = KMouseOver
			If xI1 < 0 Then

				TempVPosition = GetMouseVPosition(gSnap)

				SelectedColumn = GetColumnAtX(MouseMoveStatus.X, PanelhBMSCROLL(PanelFocus))

				Dim xMeasure As Integer = MeasureAtDisplacement(TempVPosition)
				Dim xMLength As Double = MeasureLength(xMeasure)
				Dim xVposMod As Double = TempVPosition - MeasureBottom(xMeasure)
				Dim xGCD As Double = GCD(IIf(xVposMod = 0, xMLength, xVposMod), xMLength)

				FSP1.Text = (xVposMod * gDivide / 192).ToString & " / " & (xMLength * gDivide / 192).ToString & "  "
				FSP2.Text = xVposMod.ToString & " / " & xMLength & "  "
				FSP3.Text = CInt(xVposMod / xGCD).ToString & " / " & CInt(xMLength / xGCD).ToString & "  "
				FSP4.Text = TempVPosition.ToString() & "  "
				TimeStatusLabel.Text = GetTimeFromVPosition(TempVPosition).ToString("F4")
				FSC.Text = nTitle(SelectedColumn)
				FSW.Text = String.Empty
				FSM.Text = Add3Zeros(xMeasure)
				FST.Text = String.Empty
				FSH.Text = String.Empty
				FSL.Text = String.Empty
				FSE.Text = String.Empty

			Else
				Dim xMeasure As Integer = MeasureAtDisplacement(Notes(xI1).VPosition)
				Dim xMLength As Double = MeasureLength(xMeasure)
				Dim xVposMod As Double = Notes(xI1).VPosition - MeasureBottom(xMeasure)
				Dim xGCD As Double = GCD(IIf(xVposMod = 0, xMLength, xVposMod), xMLength)

				FSP1.Text = (xVposMod * gDivide / 192).ToString & " / " & (xMLength * gDivide / 192).ToString & "  "
				FSP2.Text = xVposMod.ToString & " / " & xMLength & "  "
				FSP3.Text = CInt(xVposMod / xGCD).ToString & " / " & CInt(xMLength / xGCD).ToString & "  "
				FSP4.Text = Notes(xI1).VPosition.ToString() & "  "
				TimeStatusLabel.Text = GetTimeFromVPosition(TempVPosition).ToString("F4")
				FSC.Text = nTitle(Notes(xI1).ColumnIndex)
				FSW.Text = IIf(IsColumnNumeric(Notes(xI1).ColumnIndex),
						   Notes(xI1).Value / 10000,
						   C10to36(Notes(xI1).Value \ 10000))
				FSM.Text = Add3Zeros(xMeasure)
				FST.Text = IIf(NTInput, Strings.StatusBar.Length & " = " & Notes(xI1).Length, IIf(Notes(xI1).LongNote, Strings.StatusBar.LongNote, String.Empty))
				FSH.Text = IIf(Notes(xI1).Hidden, Strings.StatusBar.Hidden, String.Empty)
				FSL.Text = IIf(Notes(xI1).Landmine, Strings.StatusBar.LandMine, String.Empty)
				FSE.Text = IIf(Notes(xI1).HasError, Strings.StatusBar.Err, String.Empty)

			End If

		ElseIf TBWrite.Checked Then
			If SelectedColumn < 0 Then Exit Sub

			Dim xMeasure As Integer = MeasureAtDisplacement(TempVPosition)
			Dim xMLength As Double = MeasureLength(xMeasure)
			Dim xVposMod As Double = TempVPosition - MeasureBottom(xMeasure)
			Dim xGCD As Double = GCD(IIf(xVposMod = 0, xMLength, xVposMod), xMLength)

			FSP1.Text = (xVposMod * gDivide / 192).ToString & " / " & (xMLength * gDivide / 192).ToString & "  "
			FSP2.Text = xVposMod.ToString & " / " & xMLength & "  "
			FSP3.Text = CInt(xVposMod / xGCD).ToString & " / " & CInt(xMLength / xGCD).ToString & "  "
			FSP4.Text = TempVPosition.ToString() & "  "
			TimeStatusLabel.Text = GetTimeFromVPosition(TempVPosition).ToString("F4")
			FSC.Text = nTitle(SelectedColumn)
			If IsColumnSound(SelectedColumn) Then
				FSW.Text = C10to36(LWAV.SelectedIndex + 1)
			ElseIf IsColumnImage(SelectedColumn) Then
				FSW.Text = C10to36(LBMP.SelectedIndex + 1)
			Else
				FSW.Text = String.Empty
			End If
			FSM.Text = Add3Zeros(xMeasure)
			FST.Text = IIf(NTInput, TempLength, IIf(My.Computer.Keyboard.ShiftKeyDown And Not My.Computer.Keyboard.CtrlKeyDown, Strings.StatusBar.LongNote, String.Empty))
			FSH.Text = IIf(My.Computer.Keyboard.CtrlKeyDown And Not My.Computer.Keyboard.ShiftKeyDown, Strings.StatusBar.Hidden, String.Empty)
			FSL.Text = IIf(My.Computer.Keyboard.ShiftKeyDown And My.Computer.Keyboard.CtrlKeyDown, Strings.StatusBar.LandMine, String.Empty)

		ElseIf TBTimeSelect.Checked Then
			FSSS.Text = vSelStart
			FSSL.Text = vSelLength
			FSSH.Text = vSelHalf

		End If
		FStatus.Invalidate()
	End Sub

	Private Function GetTimeFromVPosition(vpos As Double) As Double
		Dim timing_notes = (From note In Notes
							Where note.ColumnIndex = niBPM Or note.ColumnIndex = niSTOP
							Group By Column = note.ColumnIndex
						   Into NoteGroups = Group).ToDictionary(Function(x) x.Column, Function(x) x.NoteGroups)

		Dim bpm_notes = timing_notes.Item(niBPM)

		Dim stop_notes As IEnumerable(Of Note) = Nothing

		Dim value As IEnumerable(Of Note) = Nothing
		If timing_notes.TryGetValue(niSTOP, value) Then
			stop_notes = value
		End If


		Dim stop_contrib As Double
		Dim bpm_contrib As Double

		For i = 0 To bpm_notes.Count() - 1
			' az: sum bpm contribution first
			Dim duration = 0.0
			Dim current_note = bpm_notes.ElementAt(i)
			Dim notevpos = Math.Max(0, current_note.VPosition)

			If i + 1 <> bpm_notes.Count() Then
				Dim next_note = bpm_notes.ElementAt(i + 1)
				duration = next_note.VPosition - notevpos
			Else
				duration = vpos - notevpos
			End If

			Dim current_bps = 60 / (current_note.Value / 10000)
			bpm_contrib += current_bps * duration / 48

			If stop_notes Is Nothing Then Continue For

			Dim stops = From stp In stop_notes
						Where stp.VPosition >= notevpos And
						stp.VPosition < notevpos + duration

			Dim stop_beats = stops.Sum(Function(x) x.Value / 10000.0) / 48
			stop_contrib += current_bps * stop_beats

		Next

		Return stop_contrib + bpm_contrib
	End Function

	Private Sub POBStorm_Click(sender As Object, e As EventArgs)

	End Sub

	Private Sub POBMirror_Click(sender As Object, e As EventArgs) Handles POBMirror.Click
		Dim xI1 As Integer
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		For xI1 = 1 To UBound(Notes)
			If Not Notes(xI1).Selected Then Continue For
			'xRedo &= sCmdKM(niA1, .VPosition, .Value, IIf(NTInput, .Length, .LongNote), .Hidden, RealColumnToEnabled(niA7) - RealColumnToEnabled(niA1), 0, True) & vbCrLf
			'xUndo &= sCmdKM(niA7, .VPosition, .Value, IIf(NTInput, .Length, .LongNote), .Hidden, RealColumnToEnabled(niA1) - RealColumnToEnabled(niA7), 0, True) & vbCrLf

			Dim xCol As Integer
			If Rscratch Then
				Select Case Notes(xI1).ColumnIndex
					Case niA1 : xCol = niA7
					Case niA2 : xCol = niA6
					Case niA3 : xCol = niA5
					Case niA4 : xCol = niA4
					Case niA5 : xCol = niA3
					Case niA6 : xCol = niA2
					Case niA7 : xCol = niA1
					Case niD1 : xCol = niD7
					Case niD2 : xCol = niD6
					Case niD3 : xCol = niD5
					Case niD4 : xCol = niD4
					Case niD5 : xCol = niD3
					Case niD6 : xCol = niD2
					Case niD7 : xCol = niD1
					Case Else : Continue For
				End Select
			Else
				Select Case Notes(xI1).ColumnIndex
					Case niA3 : xCol = niA9
					Case niA4 : xCol = niA8
					Case niA5 : xCol = niA7
					Case niA6 : xCol = niA6
					Case niA7 : xCol = niA5
					Case niA8 : xCol = niA4
					Case niA9 : xCol = niA3
					Case niD1 : xCol = niD7
					Case niD2 : xCol = niD6
					Case niD3 : xCol = niD5
					Case niD4 : xCol = niD4
					Case niD5 : xCol = niD3
					Case niD6 : xCol = niD2
					Case niD7 : xCol = niD1
					Case Else : Continue For
				End Select
			End If

			RedoMoveNote(Notes(xI1), xCol, Notes(xI1).VPosition, xUndo, xRedo)
			Notes(xI1).ColumnIndex = xCol
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		UpdatePairing()
		RefreshPanelAll()
	End Sub







	Private Sub ValidateSelection()
		If vSelStart < 0 Then vSelLength += vSelStart : vSelHalf += vSelStart : vSelStart = 0
		If vSelStart > GetMaxVPosition() - 1 Then vSelLength += vSelStart - GetMaxVPosition() + 1 : vSelHalf += vSelStart - GetMaxVPosition() + 1 : vSelStart = GetMaxVPosition() - 1
		If vSelStart + vSelLength < 0 Then vSelLength = -vSelStart
		If vSelStart + vSelLength > GetMaxVPosition() - 1 Then vSelLength = GetMaxVPosition() - 1 - vSelStart

		If Math.Sign(vSelHalf) <> Math.Sign(vSelLength) Then vSelHalf = 0
		If Math.Abs(vSelHalf) > Math.Abs(vSelLength) Then vSelHalf = vSelLength
	End Sub



	Private Sub TVCM_KeyDown(sender As Object, e As KeyEventArgs) Handles TVCM.KeyDown
		If e.KeyCode = Keys.Enter Then
			TVCM.Text = Val(TVCM.Text)
			If Val(TVCM.Text) <= 0 Then
				Dim unused = MsgBox(Strings.Messages.NegativeFactorError, MsgBoxStyle.Critical, Strings.Messages.Err)
				TVCM.Text = 1
				TVCM.Focus()
				TVCM.SelectAll()
			Else
				BVCApply_Click(BVCApply, New EventArgs)
			End If
		End If
	End Sub

	Private Sub TVCM_LostFocus(sender As Object, e As EventArgs) Handles TVCM.LostFocus
		TVCM.Text = Val(TVCM.Text)
		If Val(TVCM.Text) <= 0 Then
			Dim unused = MsgBox(Strings.Messages.NegativeFactorError, MsgBoxStyle.Critical, Strings.Messages.Err)
			TVCM.Text = 1
			TVCM.Focus()
			TVCM.SelectAll()
		End If
	End Sub

	Private Sub TVCD_KeyDown(sender As Object, e As KeyEventArgs) Handles TVCD.KeyDown
		If e.KeyCode = Keys.Enter Then
			TVCD.Text = Val(TVCD.Text)
			If Val(TVCD.Text) <= 0 Then
				Dim unused = MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
				TVCD.Text = 1
				TVCD.Focus()
				TVCD.SelectAll()
			Else
				BVCApply_Click(BVCApply, New EventArgs)
			End If
		End If
	End Sub

	Private Sub TVCD_LostFocus(sender As Object, e As EventArgs) Handles TVCD.LostFocus
		TVCD.Text = Val(TVCD.Text)
		If Val(TVCD.Text) <= 0 Then
			Dim unused = MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
			TVCD.Text = 1
			TVCD.Focus()
			TVCD.SelectAll()
		End If
	End Sub

	Private Sub TVCBPM_KeyDown(sender As Object, e As KeyEventArgs) Handles TVCBPM.KeyDown
		If e.KeyCode = Keys.Enter Then
			TVCBPM.Text = Val(TVCBPM.Text)
			If Val(TVCBPM.Text) <= 0 Then
				Dim unused = MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
				TVCBPM.Text = Notes(0).Value / 10000
				TVCBPM.Focus()
				TVCBPM.SelectAll()
			Else
				BVCCalculate_Click(BVCCalculate, New EventArgs)
			End If
		End If
	End Sub

	Private Sub TVCBPM_LostFocus(sender As Object, e As EventArgs) Handles TVCBPM.LostFocus
		TVCBPM.Text = Val(TVCBPM.Text)
		If Val(TVCBPM.Text) <= 0 Then
			Dim unused = MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
			TVCBPM.Text = Notes(0).Value / 10000
			TVCBPM.Focus()
			TVCBPM.SelectAll()
		End If
	End Sub

	Private Function FindNoteIndex(note As Note) As Integer
		Dim xI1 As Integer
		If NTInput Then
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).equalsNT(note) Then Return xI1
			Next
		Else
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).equalsBMSE(note) Then Return xI1
			Next
		End If
		Return xI1
	End Function




	Private Function sIA() As Integer
		Return IIf(sI > 98, 0, sI + 1)
	End Function

	Private Function sIM() As Integer
		Return IIf(sI < 1, 99, sI - 1)
	End Function



	Private Sub TBUndo_Click(sender As Object, e As EventArgs) Handles TBUndo.Click, mnUndo.Click
		KMouseOver = -1
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		If sUndo(sI).ofType = UndoRedo.opNoOperation Then Exit Sub
		PerformCommand(sUndo(sI))
		sI = sIM()

		TBUndo.Enabled = sUndo(sI).ofType <> UndoRedo.opNoOperation
		TBRedo.Enabled = sRedo(sIA).ofType <> UndoRedo.opNoOperation
		mnUndo.Enabled = sUndo(sI).ofType <> UndoRedo.opNoOperation
		mnRedo.Enabled = sRedo(sIA).ofType <> UndoRedo.opNoOperation
	End Sub

	Private Sub TBRedo_Click(sender As Object, e As EventArgs) Handles TBRedo.Click, mnRedo.Click
		KMouseOver = -1
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		If sRedo(sIA).ofType = UndoRedo.opNoOperation Then Exit Sub
		PerformCommand(sRedo(sIA))
		sI = sIA()

		TBUndo.Enabled = sUndo(sI).ofType <> UndoRedo.opNoOperation
		TBRedo.Enabled = sRedo(sIA).ofType <> UndoRedo.opNoOperation
		mnUndo.Enabled = sUndo(sI).ofType <> UndoRedo.opNoOperation
		mnRedo.Enabled = sRedo(sIA).ofType <> UndoRedo.opNoOperation
	End Sub

	'Undo appends before, Redo appends after.
	'After a sequence of Commands, 
	'   Undo will be the first one to execute, 
	'   Redo will be the last one to execute.
	'Remember to save the first Redo.

	'In case where undo is Nothing: Dont worry.
	'In case where redo is Nothing: 
	'   If only one redo is in a sequence, put Nothing.
	'   If several redo are in a sequence, 
	'       Create Void first. 
	'       Record its reference into a seperate copy. (xBaseRedo = xRedo)
	'       Use this xRedo as the BaseRedo.
	'       When calling AddUndo subroutine, use xBaseRedo.Next as cRedo.

	'Dim xUndo As UndoRedo.LinkedURCmd = Nothing
	'Dim xRedo As UndoRedo.LinkedURCmd = Nothing
	'... 'Me.RedoRemoveNote(K(xI1), True, xUndo, xRedo)
	'AddUndo(xUndo, xRedo)

	'Dim xUndo As UndoRedo.LinkedURCmd = Nothing
	'Dim xRedo As New UndoRedo.Void
	'Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
	'... 'Me.RedoRemoveNote(K(xI1), True, xUndo, xRedo)
	'AddUndo(xUndo, xBaseRedo.Next)



	Private Sub TBAbout_Click(sender As Object, e As EventArgs)
		'If My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\About.png") Then
		'Aboutboxx1.SelectBitmap()
		Dim Aboutboxx1 As New AboutBox1 With {
		.bBitmap = My.Resources.About0,
		.ClientSize = New Size(1000, 500)
	}
		Aboutboxx1.ClickToCopy.Visible = True
		Dim unused = Aboutboxx1.ShowDialog(Me)
		'Else
		'    MsgBox(locale.Messages.cannotfind & " ""About.png""", MsgBoxStyle.Critical, locale.Messages.err)
		'End If
	End Sub

	Private Sub TBOptions_Click(sender As Object, e As EventArgs) Handles TBVOptions.Click, mnVOptions.Click

		Dim xDiag As New OpVisual(vo, column, LWAV.Font)
		Dim unused = xDiag.ShowDialog(Me)
		UpdateColumnsX()
		RefreshPanelAll()
	End Sub

	Private Sub AddToPOWAV(xPath() As String)
		Dim xIndices(LWAV.SelectedIndices.Count - 1) As Integer
		LWAV.SelectedIndices.CopyTo(xIndices, 0)
		If xIndices.Length = 0 Then Exit Sub

		If WAVEmptyfill Then
			Dim i As Integer = 0
			Dim currWavIndex As Integer = xIndices(0)
			ReDim Preserve xIndices(UBound(xPath))

			Do While i < xIndices.Length And currWavIndex <= 1294
				Do While currWavIndex <= 1294 AndAlso hWAV(currWavIndex + 1) <> String.Empty
					currWavIndex += 1
				Loop
				If currWavIndex > 1294 Then Exit Do

				xIndices(i) = currWavIndex
				currWavIndex += 1
				i += 1
			Loop

			If currWavIndex > 1294 Then
				ReDim Preserve xPath(i - 1)
				ReDim Preserve xIndices(i - 1)
			End If
		Else
			If xIndices.Length < xPath.Length Then
				Dim i As Integer = xIndices.Length
				Dim currWavIndex As Integer = xIndices(UBound(xIndices)) + 1
				ReDim Preserve xIndices(UBound(xPath))

				Do While i < xIndices.Length And currWavIndex <= 1294
					Do While currWavIndex <= 1294 AndAlso hWAV(currWavIndex + 1) <> String.Empty
						currWavIndex += 1
					Loop
					If currWavIndex > 1294 Then Exit Do

					xIndices(i) = currWavIndex
					currWavIndex += 1
					i += 1
				Loop

				If currWavIndex > 1294 Then
					ReDim Preserve xPath(i - 1)
					ReDim Preserve xIndices(i - 1)
				End If
			End If
		End If

		'Dim xI2 As Integer = 0
		For xI1 As Integer = 0 To UBound(xPath)
			'If xI2 > UBound(xIndices) Then Exit For
			'hWAV(xIndices(xI2) + 1) = GetFileName(xPath(xI1))
			'LWAV.Items.Item(xIndices(xI2)) = C10to36(xIndices(xI2) + 1) & ": " & GetFileName(xPath(xI1))
			hWAV(xIndices(xI1) + 1) = GetFileName(xPath(xI1))
			LWAV.Items.Item(xIndices(xI1)) = C10to36(xIndices(xI1) + 1) & ": " & GetFileName(xPath(xI1))
			'xI2 += 1
		Next

		LWAV.SelectedIndices.Clear()
		For xI1 As Integer = 0 To IIf(UBound(xIndices) < UBound(xPath), UBound(xIndices), UBound(xPath))
			LWAV.SelectedIndices.Add(xIndices(xI1))
		Next

		If IsSaved Then SetIsSaved(False)
		RefreshPanelAll()
	End Sub

	Private Sub POWAV_DragDrop(sender As Object, e As DragEventArgs) Handles POWAV.DragDrop
		ReDim DDFileName(-1)
		If Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Return

		Dim xOrigPath() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
		Dim xPath() As String = FilterFileBySupported(xOrigPath, SupportedAudioExtension)
		Array.Sort(xPath)
		If xPath.Length = 0 Then
			RefreshPanelAll()
			Exit Sub
		End If

		AddToPOWAV(xPath)
	End Sub

	Private Sub POWAV_DragEnter(sender As Object, e As DragEventArgs) Handles POWAV.DragEnter
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
			DDFileName = FilterFileBySupported(CType(e.Data.GetData(DataFormats.FileDrop), String()), SupportedAudioExtension)
		Else
			e.Effect = DragDropEffects.None
		End If
		RefreshPanelAll()
	End Sub

	Private Sub POWAV_DragLeave(sender As Object, e As EventArgs) Handles POWAV.DragLeave
		ReDim DDFileName(-1)
		RefreshPanelAll()
	End Sub

	Private Sub POWAV_Resize(sender As Object, e As EventArgs) Handles POWAV.Resize
		LWAV.Height = sender.Height - 25
	End Sub

	Private Sub AddToPOBMP(xPath() As String)
		Dim xIndices(LBMP.SelectedIndices.Count - 1) As Integer
		LBMP.SelectedIndices.CopyTo(xIndices, 0)
		If xIndices.Length = 0 Then Exit Sub

		If WAVEmptyfill Then
			Dim i As Integer = 0
			Dim currBmpIndex As Integer = xIndices(0)
			ReDim Preserve xIndices(UBound(xPath))

			Do While i < xIndices.Length And currBmpIndex <= 1294
				Do While currBmpIndex <= 1294 AndAlso hBMP(currBmpIndex + 1) <> String.Empty
					currBmpIndex += 1
				Loop
				If currBmpIndex > 1294 Then Exit Do

				xIndices(i) = currBmpIndex
				currBmpIndex += 1
				i += 1
			Loop

			If currBmpIndex > 1294 Then
				ReDim Preserve xPath(i - 1)
				ReDim Preserve xIndices(i - 1)
			End If
		Else
			If xIndices.Length < xPath.Length Then
				Dim i As Integer = xIndices.Length
				Dim currBmpIndex As Integer = xIndices(UBound(xIndices)) + 1
				ReDim Preserve xIndices(UBound(xPath))

				Do While i < xIndices.Length And currBmpIndex <= 1294
					Do While currBmpIndex <= 1294 AndAlso hBMP(currBmpIndex + 1) <> String.Empty
						currBmpIndex += 1
					Loop
					If currBmpIndex > 1294 Then Exit Do

					xIndices(i) = currBmpIndex
					currBmpIndex += 1
					i += 1
				Loop

				If currBmpIndex > 1294 Then
					ReDim Preserve xPath(i - 1)
					ReDim Preserve xIndices(i - 1)
				End If
			End If
		End If

		'Dim xI2 As Integer = 0
		For xI1 As Integer = 0 To UBound(xPath)
			'If xI2 > UBound(xIndices) Then Exit For
			'hBMP(xIndices(xI2) + 1) = GetFileName(xPath(xI1))
			'LBMP.Items.Item(xIndices(xI2)) = C10to36(xIndices(xI2) + 1) & ": " & GetFileName(xPath(xI1))
			hBMP(xIndices(xI1) + 1) = GetFileName(xPath(xI1))
			LBMP.Items.Item(xIndices(xI1)) = C10to36(xIndices(xI1) + 1) & ": " & GetFileName(xPath(xI1))
			'xI2 += 1
		Next

		LBMP.SelectedIndices.Clear()
		For xI1 As Integer = 0 To IIf(UBound(xIndices) < UBound(xPath), UBound(xIndices), UBound(xPath))
			LBMP.SelectedIndices.Add(xIndices(xI1))
		Next

		If IsSaved Then SetIsSaved(False)
		RefreshPanelAll()
	End Sub

	Private Sub POBMP_DragDrop(sender As Object, e As DragEventArgs) Handles POBMP.DragDrop
		ReDim DDFileName(-1)
		If Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Return

		Dim xOrigPath() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
		Dim xPath() As String = FilterFileBySupported(xOrigPath, SupportedImageExtension)
		Array.Sort(xPath)
		If xPath.Length = 0 Then
			RefreshPanelAll()
			Exit Sub
		End If

		AddToPOBMP(xPath)
	End Sub

	Private Sub POBMP_DragEnter(sender As Object, e As DragEventArgs) Handles POBMP.DragEnter
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
			DDFileName = FilterFileBySupported(CType(e.Data.GetData(DataFormats.FileDrop), String()), SupportedImageExtension)
		Else
			e.Effect = DragDropEffects.None
		End If
		RefreshPanelAll()
	End Sub

	Private Sub POBMP_DragLeave(sender As Object, e As EventArgs) Handles POBMP.DragLeave
		ReDim DDFileName(-1)
		RefreshPanelAll()
	End Sub

	Private Sub POBMP_Resize(sender As Object, e As EventArgs) Handles POBMP.Resize
		LBMP.Height = sender.Height - 25
	End Sub
	Private Sub POBeat_Resize(sender As Object, e As EventArgs) Handles POBeat.Resize
		LBeat.Height = POBeat.Height - 25
	End Sub
	Private Sub POExpansion_Resize(sender As Object, e As EventArgs) Handles POExpansion.Resize
		TExpansion.Height = POExpansion.Height - 2
	End Sub

	Private Sub mn_DropDownClosed(sender As Object, e As EventArgs)
		sender.ForeColor = Color.White
	End Sub
	Private Sub mn_DropDownOpened(sender As Object, e As EventArgs)
		sender.ForeColor = Color.Black
	End Sub
	Private Sub mn_MouseEnter(sender As Object, e As EventArgs)
		If sender.Pressed Then Return
		sender.ForeColor = Color.Black
	End Sub
	Private Sub mn_MouseLeave(sender As Object, e As EventArgs)
		If sender.Pressed Then Return
		sender.ForeColor = Color.White
	End Sub

	Private Sub TBPOptions_Click(sender As Object, e As EventArgs) Handles TBPOptions.Click, mnPOptions.Click
		Dim xDOp As New OpPlayer(CurrentPlayer)
		Dim unused = xDOp.ShowDialog(Me)
	End Sub

	Private Sub THGenre_TextChanged(sender As Object, e As EventArgs) Handles _
THGenre.TextChanged, THTitle.TextChanged, THArtist.TextChanged, THPlayLevel.TextChanged, CHRank.SelectedIndexChanged, TExpansion.TextChanged,
THSubTitle.TextChanged, THSubArtist.TextChanged, THStageFile.TextChanged, THBanner.TextChanged, THBackBMP.TextChanged,
CHDifficulty.SelectedIndexChanged, THExRank.TextChanged, THTotal.TextChanged, THComment.TextChanged, THPreview.TextChanged, CHLnmode.SelectedIndexChanged
		If IsSaved Then SetIsSaved(False)

		If ReferenceEquals(sender, THLandMine) Then
			hWAV(0) = THLandMine.Text
		ElseIf ReferenceEquals(sender, THMissBMP) Then
			hBMP(0) = THMissBMP.Text
		End If
	End Sub

	Private Sub CHLnObj_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CHLnObj.SelectedIndexChanged
		If IsSaved Then SetIsSaved(False)
		LnObj = CHLnObj.SelectedIndex
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub ConvertBMSE2NT()
		ReDim SelectedNotes(-1)
		SortByVPositionInsertion()

		For i2 As Integer = 0 To UBound(Notes)
			Notes(i2).Length = 0.0#
		Next

		Dim i As Integer = 1
		Dim j As Integer = 0
		Dim xUbound As Integer = UBound(Notes)

		Do While i <= xUbound
			If Not Notes(i).LongNote Then i += 1 : Continue Do

			For j = i + 1 To xUbound
				If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For

				If Notes(j).LongNote Then
					Notes(i).Length = Notes(j).VPosition - Notes(i).VPosition
					For j2 As Integer = j To xUbound - 1
						Notes(j2) = Notes(j2 + 1)
					Next
					xUbound -= 1
					Exit For

				ElseIf Notes(j).Value \ 10000 = LnObj Then
					Exit For

				End If
			Next

			i += 1
		Loop

		ReDim Preserve Notes(xUbound)

		For i = 0 To xUbound
			Notes(i).LongNote = False
		Next

		SortByVPositionInsertion()
		UpdatePairing()
		CalculateTotalPlayableNotes()
	End Sub

	Private Sub ConvertNT2BMSE()
		ReDim SelectedNotes(-1)
		Dim xK(0) As Note
		xK(0) = Notes(0)

		For xI1 As Integer = 1 To UBound(Notes)
			ReDim Preserve xK(UBound(xK) + 1)
			With xK(UBound(xK))
				.ColumnIndex = Notes(xI1).ColumnIndex
				.LongNote = Notes(xI1).Length > 0
				.Landmine = Notes(xI1).Landmine
				.Value = Notes(xI1).Value
				.VPosition = Notes(xI1).VPosition
				.Selected = Notes(xI1).Selected
				.Hidden = Notes(xI1).Hidden
			End With

			If Notes(xI1).Length > 0 Then
				ReDim Preserve xK(UBound(xK) + 1)
				With xK(UBound(xK))
					.ColumnIndex = Notes(xI1).ColumnIndex
					.LongNote = True
					.Landmine = False
					.Value = Notes(xI1).Value
					.VPosition = Notes(xI1).VPosition + Notes(xI1).Length
					.Selected = Notes(xI1).Selected
					.Hidden = Notes(xI1).Hidden
				End With
			End If
		Next

		Notes = xK

		SortByVPositionInsertion()
		UpdatePairing()
		CalculateTotalPlayableNotes()
	End Sub

	Private Sub TBWavIncrease_Click(sender As Object, e As EventArgs) Handles TBWavIncrease.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		TBWavIncrease.Checked = Not sender.Checked
		RedoWavIncrease(TBWavIncrease.Checked, xUndo, xRedo)
		AddUndo(xUndo, xBaseRedo.Next)
	End Sub

	Private Sub TBNTInput_Click(sender As Object, e As EventArgs) Handles TBNTInput.Click, mnNTInput.Click
		'Dim xUndo As String = "NT_" & CInt(NTInput) & "_0" & vbCrLf & "KZ" & vbCrLf & sCmdKsAll(False)
		'Dim xRedo As String = "NT_" & CInt(Not NTInput) & "_1"
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		RedoRemoveNoteAll(False, xUndo, xRedo)

		NTInput = sender.Checked

		TBNTInput.Checked = NTInput
		mnNTInput.Checked = NTInput
		POBLong.Enabled = Not NTInput
		POBLongShort.Enabled = Not NTInput

		bAdjustLength = False
		bAdjustUpper = False

		RedoNT(NTInput, False, xUndo, xRedo)
		If NTInput Then
			ConvertBMSE2NT()
		Else
			ConvertNT2BMSE()
		End If
		RedoAddNoteAll(False, xUndo, xRedo)

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
	End Sub

	Private Sub THBPM_ValueChanged(sender As Object, e As EventArgs) Handles THBPM.ValueChanged
		If Notes IsNot Nothing Then Notes(0).Value = THBPM.Value * 10000 : RefreshPanelAll()
		If IsSaved Then SetIsSaved(False)
	End Sub

	Private Sub TWPosition_ValueChanged(sender As Object, e As EventArgs) Handles TWPosition.ValueChanged
		wPosition = TWPosition.Value
		TWPosition2.Value = IIf(wPosition > TWPosition2.Maximum, TWPosition2.Maximum, wPosition)
		RefreshPanelAll()
	End Sub

	Private Sub TWLeft_ValueChanged(sender As Object, e As EventArgs) Handles TWLeft.ValueChanged
		wLeft = TWLeft.Value
		TWLeft2.Value = IIf(wLeft > TWLeft2.Maximum, TWLeft2.Maximum, wLeft)
		RefreshPanelAll()
	End Sub

	Private Sub TWWidth_ValueChanged(sender As Object, e As EventArgs) Handles TWWidth.ValueChanged
		wWidth = TWWidth.Value
		TWWidth2.Value = IIf(wWidth > TWWidth2.Maximum, TWWidth2.Maximum, wWidth)
		RefreshPanelAll()
	End Sub

	Private Sub TWPrecision_ValueChanged(sender As Object, e As EventArgs) Handles TWPrecision.ValueChanged
		wPrecision = TWPrecision.Value
		TWPrecision2.Value = IIf(wPrecision > TWPrecision2.Maximum, TWPrecision2.Maximum, wPrecision)
		RefreshPanelAll()
	End Sub

	Private Sub TWTransparency_ValueChanged(sender As Object, e As EventArgs) Handles TWTransparency.ValueChanged
		TWTransparency2.Value = TWTransparency.Value
		vo.pBGMWav.Color = Color.FromArgb(TWTransparency.Value, vo.pBGMWav.Color)
		RefreshPanelAll()
	End Sub

	Private Sub TWSaturation_ValueChanged(sender As Object, e As EventArgs) Handles TWSaturation.ValueChanged
		Dim xColor As Color = vo.pBGMWav.Color
		TWSaturation2.Value = TWSaturation.Value
		vo.pBGMWav.Color = HSL2RGB(xColor.GetHue, TWSaturation.Value, xColor.GetBrightness * 1000, xColor.A)
		RefreshPanelAll()
	End Sub

	Private Sub TWPosition2_Scroll(sender As Object, e As EventArgs) Handles TWPosition2.Scroll
		TWPosition.Value = TWPosition2.Value
	End Sub

	Private Sub TWLeft2_Scroll(sender As Object, e As EventArgs) Handles TWLeft2.Scroll
		TWLeft.Value = TWLeft2.Value
	End Sub

	Private Sub TWWidth2_Scroll(sender As Object, e As EventArgs) Handles TWWidth2.Scroll
		TWWidth.Value = TWWidth2.Value
	End Sub

	Private Sub TWPrecision2_Scroll(sender As Object, e As EventArgs) Handles TWPrecision2.Scroll
		TWPrecision.Value = TWPrecision2.Value
	End Sub

	Private Sub TWTransparency2_Scroll(sender As Object, e As EventArgs) Handles TWTransparency2.Scroll
		TWTransparency.Value = TWTransparency2.Value
	End Sub

	Private Sub TWSaturation2_Scroll(sender As Object, e As EventArgs) Handles TWSaturation2.Scroll
		TWSaturation.Value = TWSaturation2.Value
	End Sub

	Private Sub TBLangDef_Click(sender As Object, e As EventArgs) Handles TBLangDef.Click
		DispLang = String.Empty
		Dim unused = MsgBox(Strings.Messages.PreferencePostpone, MsgBoxStyle.Information)
	End Sub

	Private Sub TBLangRefresh_Click(sender As Object, e As EventArgs) Handles TBLangRefresh.Click
		For xI1 As Integer = cmnLanguage.Items.Count - 1 To 3 Step -1
			Try
				cmnLanguage.Items.RemoveAt(xI1)
			Catch ex As Exception
			End Try
		Next

		If Not Directory.Exists(My.Application.Info.DirectoryPath & "\Data") Then My.Computer.FileSystem.CreateDirectory(My.Application.Info.DirectoryPath & "\Data")
		Dim xFileNames() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(My.Application.Info.DirectoryPath & "\Data").GetFiles("*.Lang.xml")

		For Each xStr As FileInfo In xFileNames
			LoadLocaleXML(xStr)
		Next
	End Sub


	Private Sub UpdateColumnsX()
		column(0).Left = 0
		'If col(0).Width = 0 Then col(0).Visible = False

		For xI1 As Integer = 1 To UBound(column)
			column(xI1).Left = column(xI1 - 1).Left + IIf(column(xI1 - 1).isVisible, column(xI1 - 1).Width, 0)
			'If col(xI1).Width = 0 Then col(xI1).Visible = False
		Next
		HSL.Maximum = nLeft(gColumns) + column(niB).Width
		HS.Maximum = nLeft(gColumns) + column(niB).Width
		HSR.Maximum = nLeft(gColumns) + column(niB).Width
	End Sub

	Private Sub CHPlayer_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CHPlayer.SelectedIndexChanged
		If CHPlayer.SelectedIndex = -1 Then CHPlayer.SelectedIndex = 0

		iPlayer = CHPlayer.SelectedIndex
		Dim xGP2 As Boolean = iPlayer <> 0
		column(niD8).isVisible = xGP2 And column(niAA).isVisible
		column(niD9).isVisible = xGP2 And column(niAB).isVisible
		column(niDA).isVisible = xGP2 And column(niAC).isVisible
		column(niDB).isVisible = xGP2 And column(niAD).isVisible
		column(niDC).isVisible = xGP2 And column(niAE).isVisible
		column(niDD).isVisible = xGP2 And column(niAF).isVisible
		column(niDE).isVisible = xGP2 And column(niAG).isVisible
		column(niDF).isVisible = xGP2 And column(niAH).isVisible
		column(niDG).isVisible = xGP2 And column(niAI).isVisible
		column(niDH).isVisible = xGP2 And column(niAJ).isVisible
		column(niDI).isVisible = xGP2 And column(niAK).isVisible
		column(niDJ).isVisible = xGP2 And column(niAL).isVisible
		column(niDK).isVisible = xGP2 And column(niAM).isVisible
		column(niDL).isVisible = xGP2 And column(niAN).isVisible
		column(niDM).isVisible = xGP2 And column(niAO).isVisible
		column(niDN).isVisible = xGP2 And column(niAP).isVisible
		column(niDO).isVisible = xGP2 And column(niAQ).isVisible
		If Rscratch Then
			column(niD1).isVisible = xGP2 And column(niA1).isVisible
			column(niD2).isVisible = xGP2 And column(niA2).isVisible
			column(niD3).isVisible = xGP2 And column(niA3).isVisible
			column(niD4).isVisible = xGP2 And column(niA4).isVisible
			column(niD5).isVisible = xGP2 And column(niA5).isVisible
			column(niD6).isVisible = xGP2 And column(niA6).isVisible
			column(niD7).isVisible = xGP2 And column(niA7).isVisible
			column(niDP).isVisible = xGP2 And column(niA8).isVisible
			column(niDQ).isVisible = xGP2 And column(niA9).isVisible
		Else
			column(niD1).isVisible = xGP2 And column(niA3).isVisible
			column(niD2).isVisible = xGP2 And column(niA4).isVisible
			column(niD3).isVisible = xGP2 And column(niA5).isVisible
			column(niD4).isVisible = xGP2 And column(niA6).isVisible
			column(niD5).isVisible = xGP2 And column(niA7).isVisible
			column(niD6).isVisible = xGP2 And column(niA8).isVisible
			column(niD7).isVisible = xGP2 And column(niA9).isVisible
			column(niDP).isVisible = xGP2 And column(niA1).isVisible
			column(niDQ).isVisible = xGP2 And column(niA2).isVisible
		End If
		column(niS3).isVisible = xGP2

		For xI1 As Integer = 1 To UBound(Notes)
			Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
		Next
		'AddUndo(xUndo, xRedo)
		UpdateColumnsX()

		If IsInitializing Then Exit Sub
		RefreshPanelAll()
	End Sub

	Private Sub CGB_ValueChanged(sender As Object, e As EventArgs) Handles CGB.ValueChanged
		gColumns = niB + CGB.Value - 1
		UpdateColumnsX()
		RefreshPanelAll()
	End Sub

	Private Sub TBGOptions_Click(sender As Object, e As EventArgs) Handles TBGOptions.Click, mnGOptions.Click
		Dim xTE As Integer
		Select Case UCase(EncodingToString(TextEncoding)) ' az: wow seriously? is there really no better way? 
			Case "SYSTEM ANSI" : xTE = 0
			Case "LITTLE ENDIAN UTF16" : xTE = 1
			Case "ASCII" : xTE = 2
			Case "BIG ENDIAN UTF16" : xTE = 3
			Case "LITTLE ENDIAN UTF32" : xTE = 4
			Case "UTF7" : xTE = 5
			Case "UTF8" : xTE = 6
			Case "SJIS" : xTE = 7
			Case "EUC-KR" : xTE = 8
			Case Else : xTE = 0
		End Select

		Dim xDiag As New OpGeneral(gWheel, gPgUpDn, MiddleButtonMoveMethod, xTE, 192.0R / BMSGridLimit,
		AutoSaveInterval, BeepWhileSaved, BPMx1296, STOPx1296,
		AutoFocusMouseEnter, FirstClickDisabled, ClickStopPreview)

		If xDiag.ShowDialog() = Forms.DialogResult.OK Then
			With xDiag
				gWheel = .zWheel
				gPgUpDn = .zPgUpDn
				TextEncoding = .zEncoding
				'SortingMethod = .zSort
				MiddleButtonMoveMethod = .zMiddle
				AutoSaveInterval = .zAutoSave
				BMSGridLimit = 192.0R / .zGridPartition
				BeepWhileSaved = .cBeep.Checked
				BPMx1296 = .cBpm1296.Checked
				STOPx1296 = .cStop1296.Checked
				AutoFocusMouseEnter = .cMEnterFocus.Checked
				FirstClickDisabled = .cMClickFocus.Checked
				ClickStopPreview = .cMStopPreview.Checked
			End With
			If AutoSaveInterval Then AutoSaveTimer.Interval = AutoSaveInterval
			AutoSaveTimer.Enabled = AutoSaveInterval
		End If
	End Sub

	Private Sub POBLong_Click(sender As Object, e As EventArgs) Handles POBLong.Click
		If NTInput Then Exit Sub

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		For xI1 As Integer = 1 To UBound(Notes)
			If Not Notes(xI1).Selected Then Continue For

			RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, True, xUndo, xRedo)
			Notes(xI1).LongNote = True
		Next
		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub POBNormal_Click(sender As Object, e As EventArgs) Handles POBShort.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		If Not NTInput Then
			For xI1 As Integer = 1 To UBound(Notes)
				If Not Notes(xI1).Selected Then Continue For

				RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, 0, xUndo, xRedo)
				Notes(xI1).LongNote = False
			Next

		Else
			For xI1 As Integer = 1 To UBound(Notes)
				If Not Notes(xI1).Selected Then Continue For

				RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, 0, xUndo, xRedo)
				Notes(xI1).Length = 0
			Next
		End If

		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub POBNormalLong_Click(sender As Object, e As EventArgs) Handles POBLongShort.Click
		If NTInput Then Exit Sub

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		For xI1 As Integer = 1 To UBound(Notes)
			If Not Notes(xI1).Selected Then Continue For

			RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, Not Notes(xI1).LongNote, xUndo, xRedo)
			Notes(xI1).LongNote = Not Notes(xI1).LongNote
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub POBHidden_Click(sender As Object, e As EventArgs) Handles POBHidden.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		For xI1 As Integer = 1 To UBound(Notes)
			If Not Notes(xI1).Selected Then Continue For

			RedoHiddenNoteModify(Notes(xI1), True, True, xUndo, xRedo)
			Notes(xI1).Hidden = True
		Next
		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub POBVisible_Click(sender As Object, e As EventArgs) Handles POBVisible.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		For xI1 As Integer = 1 To UBound(Notes)
			If Not Notes(xI1).Selected Then Continue For

			RedoHiddenNoteModify(Notes(xI1), False, True, xUndo, xRedo)
			Notes(xI1).Hidden = False
		Next
		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub POBHiddenVisible_Click(sender As Object, e As EventArgs) Handles POBHiddenVisible.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		For xI1 As Integer = 1 To UBound(Notes)
			If Not Notes(xI1).Selected Then Continue For

			RedoHiddenNoteModify(Notes(xI1), Not Notes(xI1).Hidden, True, xUndo, xRedo)
			Notes(xI1).Hidden = Not Notes(xI1).Hidden
		Next
		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
	End Sub

	Private Sub POBModify_Click(sender As Object, e As EventArgs) Handles POBModify.Click
		Dim xNum As Boolean = False
		Dim xLbl As Boolean = False
		Dim xI1 As Integer

		For xI1 = 1 To UBound(Notes)
			If Notes(xI1).Selected AndAlso IsColumnNumeric(Notes(xI1).ColumnIndex) Then xNum = True : Exit For
		Next
		For xI1 = 1 To UBound(Notes)
			If Notes(xI1).Selected AndAlso Not IsColumnNumeric(Notes(xI1).ColumnIndex) Then xLbl = True : Exit For
		Next
		If Not (xNum Or xLbl) Then Exit Sub

		If xNum Then
			Dim xD1 As Double = Val(InputBox(Strings.Messages.PromptEnterNumeric, Text)) * 10000
			If Not xD1 = 0 Then
				If xD1 <= 0 Then xD1 = 1

				Dim xUndo As UndoRedo.LinkedURCmd = Nothing
				Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
				Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

				For xI1 = 1 To UBound(Notes)
					If Not IsColumnNumeric(Notes(xI1).ColumnIndex) Then Continue For
					If Not Notes(xI1).Selected Then Continue For

					RedoRelabelNote(Notes(xI1), xD1, xUndo, xRedo)
					Notes(xI1).Value = xD1
				Next
				AddUndo(xUndo, xBaseRedo.Next)
			End If
		End If

		If xLbl Then
			Dim xStr As String = UCase(Trim(InputBox(Strings.Messages.PromptEnter, Text)))

			If Len(xStr) = 0 Then GoTo Jump2
			If xStr = "00" Or xStr = "0" Then GoTo Jump1
			If Not Len(xStr) = 1 And Not Len(xStr) = 2 Then GoTo Jump1

			Dim xI3 As Integer = Asc(Mid(xStr, 1, 1))
			If Not ((xI3 >= 48 And xI3 <= 57) Or (xI3 >= 65 And xI3 <= 90)) Then GoTo Jump1
			If Len(xStr) = 2 Then
				Dim xI4 As Integer = Asc(Mid(xStr, 2, 1))
				If Not ((xI4 >= 48 And xI4 <= 57) Or (xI4 >= 65 And xI4 <= 90)) Then GoTo Jump1
			End If
			Dim xVal As Integer = C36to10(xStr) * 10000

			Dim xUndo As UndoRedo.LinkedURCmd = Nothing
			Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
			Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

			For xI1 = 1 To UBound(Notes)
				If IsColumnNumeric(Notes(xI1).ColumnIndex) Then Continue For
				If Not Notes(xI1).Selected Then Continue For

				RedoRelabelNote(Notes(xI1), xVal, xUndo, xRedo)
				Notes(xI1).Value = xVal
			Next
			AddUndo(xUndo, xBaseRedo.Next)
			GoTo Jump2
Jump1:
			Dim unused = MsgBox(Strings.Messages.InvalidLabel, MsgBoxStyle.Critical, Strings.Messages.Err)
Jump2:
		End If

		RefreshPanelAll()
	End Sub

	Private Sub TBMyO2_Click(sender As Object, e As EventArgs) Handles TBMyO2.Click, mnMyO2.Click
		Dim xDiag As New dgMyO2
		xDiag.Show()
	End Sub


	Private Sub TBFind_Click(sender As Object, e As EventArgs) Handles TBFind.Click, mnFind.Click
		Dim xDiag As New diagFind(gColumns, Strings.Messages.Err, Strings.Messages.InvalidLabel)
		xDiag.Show()
	End Sub

	Private Function fdrCheck(xNote As Note) As Boolean
		Return xNote.VPosition >= MeasureBottom(fdriMesL) And xNote.VPosition < MeasureBottom(fdriMesU) + MeasureLength(fdriMesU) AndAlso
		   IIf(IsColumnNumeric(xNote.ColumnIndex),
			   xNote.Value >= fdriValL And xNote.Value <= fdriValU,
			   xNote.Value >= fdriLblL And xNote.Value <= fdriLblU) AndAlso
		   Array.IndexOf(fdriCol, xNote.ColumnIndex) <> -1
	End Function

	Private Function fdrRangeS(xbLim1 As Boolean, xbLim2 As Boolean, xVal As Boolean) As Boolean
		Return (Not xbLim1 And xbLim2 And xVal) Or (xbLim1 And Not xbLim2 And Not xVal) Or (xbLim1 And xbLim2)
	End Function

	Public Sub fdrSelect(iRange As Integer,
					  xMesL As Integer, xMesU As Integer,
					  xLblL As String, xLblU As String,
					  xValL As Integer, xValU As Integer,
					  iCol() As Integer)

		fdriMesL = xMesL
		fdriMesU = xMesU
		fdriLblL = C36to10(xLblL) * 10000
		fdriLblU = C36to10(xLblU) * 10000
		fdriValL = xValL
		fdriValU = xValU
		fdriCol = iCol

		Dim xbSel As Boolean = iRange Mod 2 = 0
		Dim xbUnsel As Boolean = iRange Mod 3 = 0
		Dim xbShort As Boolean = iRange Mod 5 = 0
		Dim xbLong As Boolean = iRange Mod 7 = 0
		Dim xbHidden As Boolean = iRange Mod 11 = 0
		Dim xbVisible As Boolean = iRange Mod 13 = 0

		Dim xSel(UBound(Notes)) As Boolean
		For xI1 As Integer = 1 To UBound(Notes)
			xSel(xI1) = Notes(xI1).Selected
		Next

		'Main process
		For xI1 As Integer = 1 To UBound(Notes)
			Dim bbba As Boolean = xbSel And xSel(xI1)
			Dim bbbb As Boolean = xbUnsel And Not xSel(xI1)
			Dim bbbc As Boolean = nEnabled(Notes(xI1).ColumnIndex)
			Dim bbbd As Boolean = fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote))
			Dim bbbe As Boolean = fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden)
			Dim bbbf As Boolean = fdrCheck(Notes(xI1))

			If ((xbSel And xSel(xI1)) Or (xbUnsel And Not xSel(xI1))) AndAlso
				nEnabled(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
				Notes(xI1).Selected = fdrCheck(Notes(xI1))
			End If
		Next

		RefreshPanelAll()
		Beep()
	End Sub

	Public Sub fdrUnselect(iRange As Integer,
						xMesL As Integer, xMesU As Integer,
						xLblL As String, xLblU As String,
						xValL As Integer, xValU As Integer,
						iCol() As Integer)

		fdriMesL = xMesL
		fdriMesU = xMesU
		fdriLblL = C36to10(xLblL) * 10000
		fdriLblU = C36to10(xLblU) * 10000
		fdriValL = xValL
		fdriValU = xValU
		fdriCol = iCol

		Dim xbSel As Boolean = iRange Mod 2 = 0
		Dim xbUnsel As Boolean = iRange Mod 3 = 0
		Dim xbShort As Boolean = iRange Mod 5 = 0
		Dim xbLong As Boolean = iRange Mod 7 = 0
		Dim xbHidden As Boolean = iRange Mod 11 = 0
		Dim xbVisible As Boolean = iRange Mod 13 = 0

		Dim xSel(UBound(Notes)) As Boolean
		For xI1 As Integer = 1 To UBound(Notes)
			xSel(xI1) = Notes(xI1).Selected
		Next

		'Main process
		For xI1 As Integer = 1 To UBound(Notes)
			If ((xbSel And xSel(xI1)) Or (xbUnsel And Not xSel(xI1))) AndAlso
					nEnabled(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
				Notes(xI1).Selected = Not fdrCheck(Notes(xI1))
			End If
		Next

		RefreshPanelAll()
		Beep()
	End Sub

	Public Sub fdrDelete(iRange As Integer,
					  xMesL As Integer, xMesU As Integer,
					  xLblL As String, xLblU As String,
					  xValL As Integer, xValU As Integer,
					  iCol() As Integer)

		fdriMesL = xMesL
		fdriMesU = xMesU
		fdriLblL = C36to10(xLblL) * 10000
		fdriLblU = C36to10(xLblU) * 10000
		fdriValL = xValL
		fdriValU = xValU
		fdriCol = iCol

		Dim xbSel As Boolean = iRange Mod 2 = 0
		Dim xbUnsel As Boolean = iRange Mod 3 = 0
		Dim xbShort As Boolean = iRange Mod 5 = 0
		Dim xbLong As Boolean = iRange Mod 7 = 0
		Dim xbHidden As Boolean = iRange Mod 11 = 0
		Dim xbVisible As Boolean = iRange Mod 13 = 0

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		'Main process
		Dim xI1 As Integer = 1
		Do While xI1 <= UBound(Notes)
			If ((xbSel And Notes(xI1).Selected) Or (xbUnsel And Not Notes(xI1).Selected)) AndAlso
					fdrCheck(Notes(xI1)) AndAlso nEnabled(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
				RedoRemoveNote(Notes(xI1), xUndo, xRedo)
				RemoveNote(xI1, False)
			Else
				xI1 += 1
			End If
		Loop

		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		RefreshPanelAll()
		CalculateTotalPlayableNotes()
		Beep()
	End Sub

	Public Sub fdrReplaceL(iRange As Integer,
						xMesL As Integer, xMesU As Integer,
						xLblL As String, xLblU As String,
						xValL As Integer, xValU As Integer,
						iCol() As Integer, xReplaceLbl As String)

		fdriMesL = xMesL
		fdriMesU = xMesU
		fdriLblL = C36to10(xLblL) * 10000
		fdriLblU = C36to10(xLblU) * 10000
		fdriValL = xValL
		fdriValU = xValU
		fdriCol = iCol

		Dim xbSel As Boolean = iRange Mod 2 = 0
		Dim xbUnsel As Boolean = iRange Mod 3 = 0
		Dim xbShort As Boolean = iRange Mod 5 = 0
		Dim xbLong As Boolean = iRange Mod 7 = 0
		Dim xbHidden As Boolean = iRange Mod 11 = 0
		Dim xbVisible As Boolean = iRange Mod 13 = 0

		Dim xxLbl As Integer = C36to10(xReplaceLbl) * 10000

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		'Main process
		For xI1 As Integer = 1 To UBound(Notes)
			If ((xbSel And Notes(xI1).Selected) Or (xbUnsel And Not Notes(xI1).Selected)) AndAlso
				fdrCheck(Notes(xI1)) AndAlso nEnabled(Notes(xI1).ColumnIndex) And Not IsColumnNumeric(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
				'xUndo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, xxLbl, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
				'xRedo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, xxLbl, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
				RedoRelabelNote(Notes(xI1), xxLbl, xUndo, xRedo)
				Notes(xI1).Value = xxLbl
			End If
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
		Beep()
	End Sub

	Public Sub fdrReplaceV(iRange As Integer,
						xMesL As Integer, xMesU As Integer,
						xLblL As String, xLblU As String,
						xValL As Integer, xValU As Integer,
						iCol() As Integer, xReplaceVal As Integer)

		fdriMesL = xMesL
		fdriMesU = xMesU
		fdriLblL = C36to10(xLblL) * 10000
		fdriLblU = C36to10(xLblU) * 10000
		fdriValL = xValL
		fdriValU = xValU
		fdriCol = iCol

		Dim xbSel As Boolean = iRange Mod 2 = 0
		Dim xbUnsel As Boolean = iRange Mod 3 = 0
		Dim xbShort As Boolean = iRange Mod 5 = 0
		Dim xbLong As Boolean = iRange Mod 7 = 0
		Dim xbHidden As Boolean = iRange Mod 11 = 0
		Dim xbVisible As Boolean = iRange Mod 13 = 0

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		'Main process
		For xI1 As Integer = 1 To UBound(Notes)
			If ((xbSel And Notes(xI1).Selected) Or (xbUnsel And Not Notes(xI1).Selected)) AndAlso
				fdrCheck(Notes(xI1)) AndAlso nEnabled(Notes(xI1).ColumnIndex) And IsColumnNumeric(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
				'xUndo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, xReplaceVal, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
				'xRedo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, xReplaceVal, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
				RedoRelabelNote(Notes(xI1), xReplaceVal, xUndo, xRedo)
				Notes(xI1).Value = xReplaceVal
			End If
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
		Beep()
	End Sub

	Private Sub MInsert_Click(sender As Object, e As EventArgs) Handles MInsert.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		Dim xMeasure As Integer = MeasureAtDisplacement(menuVPosition)
		Dim xMLength As Double = MeasureLength(xMeasure)
		Dim xVP As Double = MeasureBottom(xMeasure)

		If NTInput Then
			Dim xI1 As Integer = 1
			Do While xI1 <= UBound(Notes)
				If MeasureAtDisplacement(Notes(xI1).VPosition) >= 999 Then
					RedoRemoveNote(Notes(xI1), xUndo, xRedo)
					RemoveNote(xI1, False)
				Else
					xI1 += 1
				End If
			Loop

			Dim xdVP As Double
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).VPosition >= xVP And Notes(xI1).VPosition + Notes(xI1).Length <= MeasureBottom(999) Then
					RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition + xMLength, xUndo, xRedo)
					Notes(xI1).VPosition += xMLength

				ElseIf Notes(xI1).VPosition >= xVP Then
					xdVP = MeasureBottom(999) - 1 - Notes(xI1).VPosition - Notes(xI1).Length
					RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition + xMLength, Notes(xI1).Length + xdVP, xUndo, xRedo)
					Notes(xI1).VPosition += xMLength
					Notes(xI1).Length += xdVP

				ElseIf Notes(xI1).VPosition + Notes(xI1).Length >= xVP Then
					xdVP = IIf(Notes(xI1).VPosition + Notes(xI1).Length > MeasureBottom(999) - 1, GetMaxVPosition() - 1 - Notes(xI1).VPosition - Notes(xI1).Length, xMLength)
					RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, Notes(xI1).Length + xdVP, xUndo, xRedo)
					Notes(xI1).Length += xdVP
				End If
			Next

		Else
			Dim xI1 As Integer = 1
			Do While xI1 <= UBound(Notes)
				If MeasureAtDisplacement(Notes(xI1).VPosition) >= 999 Then
					RedoRemoveNote(Notes(xI1), xUndo, xRedo)
					RemoveNote(xI1, False)
				Else
					xI1 += 1
				End If
			Loop

			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).VPosition >= xVP Then
					RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition + xMLength, xUndo, xRedo)
					Notes(xI1).VPosition += xMLength
				End If
			Next
		End If

		For xI1 As Integer = 999 To xMeasure + 1 Step -1
			MeasureLength(xI1) = MeasureLength(xI1 - 1)
		Next
		UpdateMeasureBottom()

		AddUndo(xUndo, xBaseRedo.Next)
		UpdatePairing()
		CalculateGreatestVPosition()
		CalculateTotalPlayableNotes()
		RefreshPanelAll()
	End Sub

	Private Sub MRemove_Click(sender As Object, e As EventArgs) Handles MRemove.Click
		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		Dim xMeasure As Integer = MeasureAtDisplacement(menuVPosition)
		Dim xMLength As Double = MeasureLength(xMeasure)

		Dim unused As Double = MeasureBottom(xMeasure)


		Dim xVP As Double
		If NTInput Then
			Dim xI1 As Integer = 1
			Do While xI1 <= UBound(Notes)
				If MeasureAtDisplacement(Notes(xI1).VPosition) = xMeasure And MeasureAtDisplacement(Notes(xI1).VPosition + Notes(xI1).Length) = xMeasure Then
					RedoRemoveNote(Notes(xI1), xUndo, xRedo)
					RemoveNote(xI1, False)
				Else
					xI1 += 1
				End If
			Loop

			Dim xdVP As Double
			xVP = MeasureBottom(xMeasure)
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).VPosition >= xVP + xMLength Then
					RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition - xMLength, xUndo, xRedo)
					Notes(xI1).VPosition -= xMLength

				ElseIf Notes(xI1).VPosition >= xVP Then
					xdVP = xMLength + xVP - Notes(xI1).VPosition
					RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition + xdVP - xMLength, Notes(xI1).Length - xdVP, xUndo, xRedo)
					Notes(xI1).VPosition += xdVP - xMLength
					Notes(xI1).Length -= xdVP

				ElseIf Notes(xI1).VPosition + Notes(xI1).Length >= xVP Then
					xdVP = IIf(Notes(xI1).VPosition + Notes(xI1).Length >= xVP + xMLength, xMLength, Notes(xI1).VPosition + Notes(xI1).Length - xVP + 1)
					RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, Notes(xI1).Length - xdVP, xUndo, xRedo)
					Notes(xI1).Length -= xdVP
				End If
			Next

		Else
			Dim xI1 As Integer = 1
			Do While xI1 <= UBound(Notes)
				If MeasureAtDisplacement(Notes(xI1).VPosition) = xMeasure Then
					RedoRemoveNote(Notes(xI1), xUndo, xRedo)
					RemoveNote(xI1, False)
				Else
					xI1 += 1
				End If
			Loop

			xVP = MeasureBottom(xMeasure)
			For xI1 = 1 To UBound(Notes)
				If Notes(xI1).VPosition >= xVP Then
					RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition - xMLength, xUndo, xRedo)
					Notes(xI1).VPosition -= xMLength
				End If
			Next
		End If

		For xI1 As Integer = 999 To xMeasure + 1 Step -1
			MeasureLength(xI1 - 1) = MeasureLength(xI1)
		Next
		UpdateMeasureBottom()

		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		CalculateGreatestVPosition()
		CalculateTotalPlayableNotes()
		RefreshPanelAll()
	End Sub

	Private Sub TBThemeDef_Click(sender As Object, e As EventArgs) Handles TBThemeDef.Click
		Dim xTempFileName As String = My.Application.Info.DirectoryPath & "\____TempFile.Theme.xml"
		My.Computer.FileSystem.WriteAllText(xTempFileName, My.Resources.O2Mania_Theme, False, System.Text.Encoding.Unicode)
		LoadSettings(xTempFileName)
		ChangePlaySideSkin(False)
		File.Delete(xTempFileName)

		RefreshPanelAll()
	End Sub

	Private Sub TBThemeSave_Click(sender As Object, e As EventArgs) Handles TBThemeSave.Click
		Dim xDiag As New SaveFileDialog With {
		.Filter = Strings.FileType.THEME_XML & "|*.Theme.xml",
		.DefaultExt = "Theme.xml",
		.InitialDirectory = My.Application.Info.DirectoryPath & "\Data"
	}
		If xDiag.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		SaveSettings(xDiag.FileName, True)
		If BeepWhileSaved Then Beep()
		TBThemeRefresh_Click(TBThemeRefresh, New EventArgs)
	End Sub

	Private Sub TBThemeRefresh_Click(sender As Object, e As EventArgs) Handles TBThemeRefresh.Click
		For xI1 As Integer = cmnTheme.Items.Count - 1 To 5 Step -1
			Try
				cmnTheme.Items.RemoveAt(xI1)
			Catch ex As Exception
			End Try
		Next

		If Not Directory.Exists(My.Application.Info.DirectoryPath & "\Data") Then My.Computer.FileSystem.CreateDirectory(My.Application.Info.DirectoryPath & "\Data")
		Dim xFileNames() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(My.Application.Info.DirectoryPath & "\Data").GetFiles("*.Theme.xml")
		For Each xStr As FileInfo In xFileNames
			Dim unused = cmnTheme.Items.Add(xStr.Name, Nothing, AddressOf LoadTheme)
		Next
	End Sub

	Private Sub TBThemeLoadComptability_Click(sender As Object, e As EventArgs) Handles TBThemeLoadComptability.Click
		Dim xDiag As New OpenFileDialog With {
		.Filter = Strings.FileType.TH & "|*.th",
		.DefaultExt = "th",
		.InitialDirectory = My.Application.Info.DirectoryPath
	}
		If My.Computer.FileSystem.DirectoryExists(My.Application.Info.DirectoryPath & "\Theme") Then xDiag.InitialDirectory = My.Application.Info.DirectoryPath & "\Theme"
		If xDiag.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		LoadThemeComptability(xDiag.FileName)
		RefreshPanelAll()
	End Sub

	''' <summary>
	''' Will return Double.PositiveInfinity if canceled.
	''' </summary>
	Private Function InputBoxDouble(Prompt As String, LBound As Double, UBound As Double, Optional Title As String = "", Optional DefaultResponse As String = "") As Double
		Dim xStr As String = InputBox(Prompt, Title, DefaultResponse)
		If xStr = String.Empty Then Return Double.PositiveInfinity

		InputBoxDouble = Val(xStr)
		If InputBoxDouble > UBound Then InputBoxDouble = UBound
		If InputBoxDouble < LBound Then InputBoxDouble = LBound
	End Function

	Private Sub FSSS_Click(sender As Object, e As EventArgs) Handles FSSS.Click
		Dim xMax As Double = IIf(vSelLength > 0, GetMaxVPosition() - vSelLength, GetMaxVPosition)
		Dim xMin As Double = IIf(vSelLength < 0, -vSelLength, 0)
		Dim xDouble As Double = InputBoxDouble("Please enter a number between " & xMin & " and " & xMax & ".", xMin, xMax, , vSelStart)
		If xDouble = Double.PositiveInfinity Then Return

		vSelStart = xDouble
		ValidateSelection()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub FSSL_Click(sender As Object, e As EventArgs) Handles FSSL.Click
		Dim xMax As Double = GetMaxVPosition() - vSelStart
		Dim xMin As Double = -vSelStart
		Dim xDouble As Double = InputBoxDouble("Please enter a number between " & xMin & " and " & xMax & ".", xMin, xMax, , vSelLength)
		If xDouble = Double.PositiveInfinity Then Return

		vSelLength = xDouble
		ValidateSelection()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub FSSH_Click(sender As Object, e As EventArgs) Handles FSSH.Click
		Dim xMax As Double = IIf(vSelLength > 0, vSelLength, 0)
		Dim xMin As Double = IIf(vSelLength > 0, 0, -vSelLength)
		Dim xDouble As Double = InputBoxDouble("Please enter a number between " & xMin & " and " & xMax & ".", xMin, xMax, , vSelHalf)
		If xDouble = Double.PositiveInfinity Then Return

		vSelHalf = xDouble
		ValidateSelection()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BVCReverse_Click(sender As Object, e As EventArgs) Handles BVCReverse.Click
		vSelStart += vSelLength
		vSelHalf -= vSelLength
		vSelLength *= -1
		ValidateSelection()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub AutoSaveTimer_Tick(sender As Object, e As EventArgs) Handles AutoSaveTimer.Tick
		Dim xTime As Date = Now
		Dim xFileName As String
		With xTime
			xFileName = My.Application.Info.DirectoryPath & "\AutoSave_" &
						  .Year & "_" & .Month & "_" & .Day & "_" & .Hour & "_" & .Minute & "_" & .Second & "_" & .Millisecond & ".IBMSC"
		End With
		'My.Computer.FileSystem.WriteAllText(xFileName, SaveiBMSC, False, System.Text.Encoding.Unicode)
		SaveiBMSC(xFileName)

		On Error Resume Next
		If PreviousAutoSavedFileName <> String.Empty Then File.Delete(PreviousAutoSavedFileName)
		On Error GoTo 0

		PreviousAutoSavedFileName = xFileName
	End Sub

	Private Sub CWAVMultiSelect_CheckedChanged(sender As Object, e As EventArgs) Handles CWAVMultiSelect.CheckedChanged
		WAVMultiSelect = CWAVMultiSelect.Checked
		LWAV.SelectionMode = IIf(WAVMultiSelect, SelectionMode.MultiExtended, SelectionMode.One)
		LBMP.SelectionMode = IIf(WAVMultiSelect, SelectionMode.MultiExtended, SelectionMode.One)
	End Sub

	Private Sub CWAVChangeLabel_CheckedChanged(sender As Object, e As EventArgs) Handles CWAVChangeLabel.CheckedChanged
		WAVChangeLabel = CWAVChangeLabel.Checked
	End Sub
	Private Sub CWAVEmptyfill_CheckedChanged(sender As Object, e As EventArgs) Handles CWAVEmptyfill.CheckedChanged
		WAVEmptyfill = CWAVEmptyfill.Checked
	End Sub

	Private Sub BWAVUp_Click(sender As Object, e As EventArgs) Handles BWAVUp.Click
		If LWAV.SelectedIndex = -1 Then Return

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		Dim xIndices(LWAV.SelectedIndices.Count - 1) As Integer
		LWAV.SelectedIndices.CopyTo(xIndices, 0)

		Dim xS As Integer
		For xS = 0 To 1294
			If Array.IndexOf(xIndices, xS) = -1 Then Exit For
		Next

		For xI1 As Integer = xS To 1294
			Dim xIndex As Integer = Array.IndexOf(xIndices, xI1)
			If xIndex <> -1 Then
				Dim xStr As String = hWAV(xI1 + 1)
				hWAV(xI1 + 1) = hWAV(xI1)
				hWAV(xI1) = xStr

				LWAV.Items.Item(xI1) = C10to36(xI1 + 1) & ": " & hWAV(xI1 + 1)
				LWAV.Items.Item(xI1 - 1) = C10to36(xI1) & ": " & hWAV(xI1)

				If Not WAVChangeLabel Then GoTo 1100

				Dim xL1 As String = C10to36(xI1)
				Dim xL2 As String = C10to36(xI1 + 1)
				For xI2 As Integer = 1 To UBound(Notes)
					If Not IsColumnSound(Notes(xI2).ColumnIndex) Then Continue For

					If C10to36(Notes(xI2).Value \ 10000) = xL1 Then
						RedoRelabelNote(Notes(xI2), (xI1 * 10000) + 10000, xUndo, xRedo)
						Notes(xI2).Value = (xI1 * 10000) + 10000

					ElseIf C10to36(Notes(xI2).Value \ 10000) = xL2 Then
						RedoRelabelNote(Notes(xI2), xI1 * 10000, xUndo, xRedo)
						Notes(xI2).Value = xI1 * 10000

					End If
				Next

1100:           xIndices(xIndex) += -1
			End If
		Next

		LWAV.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LWAV.SelectedIndices.Add(xIndices(xI1))
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BWAVDown_Click(sender As Object, e As EventArgs) Handles BWAVDown.Click
		If LWAV.SelectedIndex = -1 Then Return

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		Dim xIndices(LWAV.SelectedIndices.Count - 1) As Integer
		LWAV.SelectedIndices.CopyTo(xIndices, 0)

		Dim xS As Integer
		For xS = 1294 To 0 Step -1
			If Array.IndexOf(xIndices, xS) = -1 Then Exit For
		Next

		For xI1 As Integer = xS To 0 Step -1
			Dim xIndex As Integer = Array.IndexOf(xIndices, xI1)
			If xIndex <> -1 Then
				Dim xStr As String = hWAV(xI1 + 1)
				hWAV(xI1 + 1) = hWAV(xI1 + 2)
				hWAV(xI1 + 2) = xStr

				LWAV.Items.Item(xI1) = C10to36(xI1 + 1) & ": " & hWAV(xI1 + 1)
				LWAV.Items.Item(xI1 + 1) = C10to36(xI1 + 2) & ": " & hWAV(xI1 + 2)

				If Not WAVChangeLabel Then GoTo 1100

				Dim xL1 As String = C10to36(xI1 + 2)
				Dim xL2 As String = C10to36(xI1 + 1)
				For xI2 As Integer = 1 To UBound(Notes)
					If Not IsColumnSound(Notes(xI2).ColumnIndex) Then Continue For

					If C10to36(Notes(xI2).Value \ 10000) = xL1 Then
						RedoRelabelNote(Notes(xI2), (xI1 * 10000) + 10000, xUndo, xRedo)
						Notes(xI2).Value = (xI1 * 10000) + 10000

					ElseIf C10to36(Notes(xI2).Value \ 10000) = xL2 Then
						RedoRelabelNote(Notes(xI2), (xI1 * 10000) + 20000, xUndo, xRedo)
						Notes(xI2).Value = (xI1 * 10000) + 20000

					End If
				Next

1100:           xIndices(xIndex) += 1
			End If
		Next

		LWAV.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LWAV.SelectedIndices.Add(xIndices(xI1))
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BWAVBrowse_Click(sender As Object, e As EventArgs) Handles BWAVBrowse.Click
		Dim xDWAV As New OpenFileDialog With {
		.DefaultExt = "wav",
		.Filter = Strings.FileType._wave & "|*.wav;*.ogg;*.mp3;*.flac|" &
				   Strings.FileType.WAV & "|*.wav|" &
				   Strings.FileType.OGG & "|*.ogg|" &
				   Strings.FileType.MP3 & "|*.mp3|" &
				   Strings.FileType.FLAC & "|*.flac|" &
				   Strings.FileType._all & "|*.*",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName)),
		.Multiselect = WAVMultiSelect
	}

		If xDWAV.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		InitPath = ExcludeFileName(xDWAV.FileName)

		AddToPOWAV(xDWAV.FileNames)
	End Sub

	Private Sub BWAVRemove_Click(sender As Object, e As EventArgs) Handles BWAVRemove.Click
		Dim xIndices(LWAV.SelectedIndices.Count - 1) As Integer
		LWAV.SelectedIndices.CopyTo(xIndices, 0)
		For xI1 As Integer = 0 To UBound(xIndices)
			hWAV(xIndices(xI1) + 1) = String.Empty
			LWAV.Items.Item(xIndices(xI1)) = C10to36(xIndices(xI1) + 1) & ": "
		Next

		LWAV.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LWAV.SelectedIndices.Add(xIndices(xI1))
		Next

		If IsSaved Then SetIsSaved(False)
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BBMPUp_Click(sender As Object, e As EventArgs) Handles BBMPUp.Click
		If LBMP.SelectedIndex = -1 Then Return

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		Dim xIndices(LBMP.SelectedIndices.Count - 1) As Integer
		LBMP.SelectedIndices.CopyTo(xIndices, 0)

		Dim xS As Integer
		For xS = 0 To 1294
			If Array.IndexOf(xIndices, xS) = -1 Then Exit For
		Next

		For xI1 As Integer = xS To 1294
			Dim xIndex As Integer = Array.IndexOf(xIndices, xI1)
			If xIndex <> -1 Then
				Dim xStr As String = hBMP(xI1 + 1)
				hBMP(xI1 + 1) = hBMP(xI1)
				hBMP(xI1) = xStr

				LBMP.Items.Item(xI1) = C10to36(xI1 + 1) & ": " & hBMP(xI1 + 1)
				LBMP.Items.Item(xI1 - 1) = C10to36(xI1) & ": " & hBMP(xI1)

				If Not WAVChangeLabel Then GoTo 1101

				Dim xL1 As String = C10to36(xI1)
				Dim xL2 As String = C10to36(xI1 + 1)
				For xI2 As Integer = 1 To UBound(Notes)
					If Not IsColumnImage(Notes(xI2).ColumnIndex) Then Continue For

					If C10to36(Notes(xI2).Value \ 10000) = xL1 Then
						RedoRelabelNote(Notes(xI2), (xI1 * 10000) + 10000, xUndo, xRedo)
						Notes(xI2).Value = (xI1 * 10000) + 10000

					ElseIf C10to36(Notes(xI2).Value \ 10000) = xL2 Then
						RedoRelabelNote(Notes(xI2), xI1 * 10000, xUndo, xRedo)
						Notes(xI2).Value = xI1 * 10000

					End If
				Next

1101:           xIndices(xIndex) += -1
			End If
		Next

		LBMP.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LBMP.SelectedIndices.Add(xIndices(xI1))
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BBMPDown_Click(sender As Object, e As EventArgs) Handles BBMPDown.Click
		If LBMP.SelectedIndex = -1 Then Return

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		Dim xIndices(LBMP.SelectedIndices.Count - 1) As Integer
		LBMP.SelectedIndices.CopyTo(xIndices, 0)

		Dim xS As Integer
		For xS = 1294 To 0 Step -1
			If Array.IndexOf(xIndices, xS) = -1 Then Exit For
		Next

		For xI1 As Integer = xS To 0 Step -1
			Dim xIndex As Integer = Array.IndexOf(xIndices, xI1)
			If xIndex <> -1 Then
				Dim xStr As String = hBMP(xI1 + 1)
				hBMP(xI1 + 1) = hBMP(xI1 + 2)
				hBMP(xI1 + 2) = xStr

				LBMP.Items.Item(xI1) = C10to36(xI1 + 1) & ": " & hBMP(xI1 + 1)
				LBMP.Items.Item(xI1 + 1) = C10to36(xI1 + 2) & ": " & hBMP(xI1 + 2)

				If Not WAVChangeLabel Then GoTo 1100

				Dim xL1 As String = C10to36(xI1 + 2)
				Dim xL2 As String = C10to36(xI1 + 1)
				For xI2 As Integer = 1 To UBound(Notes)
					If Not IsColumnImage(Notes(xI2).ColumnIndex) Then Continue For

					If C10to36(Notes(xI2).Value \ 10000) = xL1 Then
						RedoRelabelNote(Notes(xI2), (xI1 * 10000) + 10000, xUndo, xRedo)
						Notes(xI2).Value = (xI1 * 10000) + 10000

					ElseIf C10to36(Notes(xI2).Value \ 10000) = xL2 Then
						RedoRelabelNote(Notes(xI2), (xI1 * 10000) + 20000, xUndo, xRedo)
						Notes(xI2).Value = (xI1 * 10000) + 20000

					End If
				Next

1100:           xIndices(xIndex) += 1
			End If
		Next

		LBMP.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LBMP.SelectedIndices.Add(xIndices(xI1))
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BBMPBrowse_Click(sender As Object, e As EventArgs) Handles BBMPBrowse.Click
		Dim xDBMP As New OpenFileDialog With {
		.DefaultExt = "bmp",
		.Filter = Strings.FileType._image & "|*.bmp;*.png;*.jpg;*.jpeg;.gif|" &
				   Strings.FileType._movie & "|*.mpg;*.m1v;*.m2v;*.avi;*.mp4;*.m4v;*.wmv;*.webm|" &
				   Strings.FileType.BMP & "|*.bmp|" &
				   Strings.FileType.PNG & "|*.png|" &
				   Strings.FileType.JPG & "|*.jpg;*.jpeg|" &
				   Strings.FileType.GIF & "|*.gif|" &
				   Strings.FileType.MP4 & "|*.mp4;*.m4v|" &
				   Strings.FileType.AVI & "|*.avi|" &
				   Strings.FileType.MPG & "|*.mpg;*.m1v;*.m2v|" &
				   Strings.FileType.WMV & "|*.wmv|" &
				   Strings.FileType.WEBM & "|*.webm|" &
				   Strings.FileType._all & "|*.*",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName)),
		.Multiselect = WAVMultiSelect
	}

		If xDBMP.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		InitPath = ExcludeFileName(xDBMP.FileName)

		AddToPOBMP(xDBMP.FileNames)
	End Sub

	Private Sub BBMPRemove_Click(sender As Object, e As EventArgs) Handles BBMPRemove.Click
		Dim xIndices(LBMP.SelectedIndices.Count - 1) As Integer
		LBMP.SelectedIndices.CopyTo(xIndices, 0)
		For xI1 As Integer = 0 To UBound(xIndices)
			hBMP(xIndices(xI1) + 1) = String.Empty
			LBMP.Items.Item(xIndices(xI1)) = C10to36(xIndices(xI1) + 1) & ": "
		Next

		LBMP.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LBMP.SelectedIndices.Add(xIndices(xI1))
		Next

		If IsSaved Then SetIsSaved(False)
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub mnMain_MouseDown(sender As Object, e As MouseEventArgs) Handles mnMain.MouseDown ', TBMain.MouseDown  ', pttl.MouseDown, pIsSaved.MouseDown
		If e.Button = MouseButtons.Left Then
			Dim unused1 = ReleaseCapture()
			Dim unused = SendMessage(Handle, &H112, &HF012, 0)
			If e.Clicks = 2 Then
				If WindowState = FormWindowState.Maximized Then WindowState = FormWindowState.Normal Else WindowState = FormWindowState.Maximized
			End If
		ElseIf e.Button = MouseButtons.Right Then
			'mnSys.Show(sender, e.Location)
		End If
	End Sub

	Private Sub mnSelectAll_Click(sender As Object, e As EventArgs) Handles mnSelectAll.Click
		If Not (PMainIn.Focused OrElse PMainInL.Focused Or PMainInR.Focused) Then Exit Sub
		For xI1 As Integer = 1 To UBound(Notes)
			Notes(xI1).Selected = nEnabled(Notes(xI1).ColumnIndex)
		Next
		If TBTimeSelect.Checked Then
			CalculateGreatestVPosition()
			vSelStart = 0
			vSelLength = MeasureBottom(MeasureAtDisplacement(GreatestVPosition)) + MeasureLength(MeasureAtDisplacement(GreatestVPosition))
		End If
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub mnDelete_Click(sender As Object, e As EventArgs) Handles mnDelete.Click
		If Not (PMainIn.Focused OrElse PMainInL.Focused Or PMainInR.Focused) Then Exit Sub

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		RedoRemoveNoteSelected(True, xUndo, xRedo)
		RemoveNotes(True)

		AddUndo(xUndo, xBaseRedo.Next)
		CalculateGreatestVPosition()
		CalculateTotalPlayableNotes()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub mnUpdate_Click(sender As Object, e As EventArgs)
		Dim unused = Process.Start("http://www.cs.mcgill.ca/~ryang6/iBMSC/")
	End Sub

	Private Sub mnUpdateC_Click(sender As Object, e As EventArgs)
		Dim unused = Process.Start("http://bbs.rohome.net/thread-1074065-1-1.html")
	End Sub

	Private Sub mnQuit_Click(sender As Object, e As EventArgs) Handles mnQuit.Click
		Close()
	End Sub


	Private Sub EnableDWM()
		mnMain.BackColor = Color.Black
		'TBMain.BackColor = Color.FromArgb(64, 64, 64)

		For Each xmn As ToolStripMenuItem In mnMain.Items
			xmn.ForeColor = Color.White
			AddHandler xmn.DropDownClosed, AddressOf mn_DropDownClosed
			AddHandler xmn.DropDownOpened, AddressOf mn_DropDownOpened
			AddHandler xmn.MouseEnter, AddressOf mn_MouseEnter
			AddHandler xmn.MouseLeave, AddressOf mn_MouseLeave
		Next
	End Sub

	Private Sub DisableDWM()
		mnMain.BackColor = SystemColors.Control
		'TBMain.BackColor = SystemColors.Control

		For Each xmn As ToolStripMenuItem In mnMain.Items
			xmn.ForeColor = SystemColors.ControlText
			RemoveHandler xmn.DropDownClosed, AddressOf mn_DropDownClosed
			RemoveHandler xmn.DropDownOpened, AddressOf mn_DropDownOpened
			RemoveHandler xmn.MouseEnter, AddressOf mn_MouseEnter
			RemoveHandler xmn.MouseLeave, AddressOf mn_MouseLeave
		Next
	End Sub

	Private Sub ttlIcon_MouseDown(sender As Object, e As MouseEventArgs)
		'ttlIcon.Image = My.Resources.icon2_16
		'mnSys.Show(ttlIcon, 0, ttlIcon.Height)
	End Sub
	Private Sub ttlIcon_MouseEnter(sender As Object, e As EventArgs)
		'ttlIcon.Image = My.Resources.icon2_16_highlight
	End Sub
	Private Sub ttlIcon_MouseLeave(sender As Object, e As EventArgs)
		'ttlIcon.Image = My.Resources.icon2_16
	End Sub

	Private Sub mnSMenu_Click(sender As Object, e As EventArgs) Handles mnSMenu.CheckedChanged
		mnMain.Visible = mnSMenu.Checked
	End Sub
	Private Sub mnSTB_Click(sender As Object, e As EventArgs) Handles mnSTB.CheckedChanged
		TBMain.Visible = mnSTB.Checked
	End Sub
	Private Sub mnSOP_Click(sender As Object, e As EventArgs) Handles mnSOP.CheckedChanged
		POptions.Visible = mnSOP.Checked
	End Sub
	Private Sub mnSStatus_Click(sender As Object, e As EventArgs) Handles mnSStatus.CheckedChanged
		pStatus.Visible = mnSStatus.Checked
	End Sub
	Private Sub mnSLSplitter_Click(sender As Object, e As EventArgs) Handles mnSLSplitter.CheckedChanged
		SpL.Visible = mnSLSplitter.Checked
	End Sub
	Private Sub mnSRSplitter_Click(sender As Object, e As EventArgs) Handles mnSRSplitter.CheckedChanged
		SpR.Visible = mnSRSplitter.Checked
	End Sub
	Private Sub CGShow_CheckedChanged(sender As Object, e As EventArgs) Handles CGShow.CheckedChanged
		gShowGrid = CGShow.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGShowS_CheckedChanged(sender As Object, e As EventArgs) Handles CGShowS.CheckedChanged
		gShowSubGrid = CGShowS.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGShowBG_CheckedChanged(sender As Object, e As EventArgs) Handles CGShowBG.CheckedChanged
		gShowBG = CGShowBG.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGShowM_CheckedChanged(sender As Object, e As EventArgs) Handles CGShowM.CheckedChanged
		gShowMeasureNumber = CGShowM.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGShowV_CheckedChanged(sender As Object, e As EventArgs) Handles CGShowV.CheckedChanged
		gShowVerticalLine = CGShowV.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGShowMB_CheckedChanged(sender As Object, e As EventArgs) Handles CGShowMB.CheckedChanged
		gShowMeasureBar = CGShowMB.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGShowC_CheckedChanged(sender As Object, e As EventArgs) Handles CGShowC.CheckedChanged
		gShowC = CGShowC.Checked
		RefreshPanelAll()
	End Sub
	Private Sub CGBLP_CheckedChanged(sender As Object, e As EventArgs) Handles CGBLP.CheckedChanged
		gDisplayBGAColumn = CGBLP.Checked

		column(niBGA).isVisible = gDisplayBGAColumn
		column(niLAYER).isVisible = gDisplayBGAColumn
		column(niPOOR).isVisible = gDisplayBGAColumn
		column(niS4).isVisible = gDisplayBGAColumn

		If IsInitializing Then Exit Sub
		For xI1 As Integer = 1 To UBound(Notes)
			Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
		Next
		'AddUndo(xUndo, xRedo)
		UpdateColumnsX()
		RefreshPanelAll()
	End Sub
	Private Sub CGSCROLL_CheckedChanged(sender As Object, e As EventArgs) Handles CGSCROLL.CheckedChanged
		gSCROLL = CGSCROLL.Checked

		column(niSCROLL).isVisible = gSCROLL

		If IsInitializing Then Exit Sub
		For xI1 As Integer = 1 To UBound(Notes)
			Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
		Next
		'AddUndo(xUndo, xRedo)
		UpdateColumnsX()
		RefreshPanelAll()
	End Sub
	Private Sub CGSTOP_CheckedChanged(sender As Object, e As EventArgs) Handles CGSTOP.CheckedChanged
		gSTOP = CGSTOP.Checked

		column(niSTOP).isVisible = gSTOP

		If IsInitializing Then Exit Sub
		For xI1 As Integer = 1 To UBound(Notes)
			Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
		Next
		'AddUndo(xUndo, xRedo)
		UpdateColumnsX()
		RefreshPanelAll()
	End Sub
	Private Sub CGBPM_CheckedChanged(sender As Object, e As EventArgs) Handles CGBPM.CheckedChanged
		'Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		'Dim xRedo As UndoRedo.LinkedURCmd = Nothing
		'Me.RedoChangeVisibleColumns(gBLP, gSTOP, iPlayer, gBLP, CGSTOP.Checked, iPlayer, xUndo, xRedo)
		gBPM = CGBPM.Checked

		column(niBPM).isVisible = gBPM

		If IsInitializing Then Exit Sub
		For xI1 As Integer = 1 To UBound(Notes)
			Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
		Next
		'AddUndo(xUndo, xRedo)
		UpdateColumnsX()
		RefreshPanelAll()
	End Sub

	Private Sub CGDisableVertical_CheckedChanged(sender As Object, e As EventArgs) Handles CGDisableVertical.CheckedChanged
		DisableVerticalMove = CGDisableVertical.Checked
	End Sub

	Private Sub CBeatPreserve_Click(sender As Object, e As EventArgs) Handles CBeatPreserve.Click, CBeatMeasure.Click, CBeatCut.Click, CBeatScale.Click
		'If Not sender.Checked Then Exit Sub
		Dim xBeatList() As RadioButton = {CBeatPreserve, CBeatMeasure, CBeatCut, CBeatScale}
		BeatChangeMode = Array.IndexOf(Of RadioButton)(xBeatList, sender)
		'For xI1 As Integer = 0 To mnBeat.Items.Count - 1
		'If xI1 <> BeatChangeMode Then CType(mnBeat.Items(xI1), ToolStripMenuItem).Checked = False
		'Next
		'sender.Checked = True
	End Sub


	Private Sub tBeatValue_LostFocus(sender As Object, e As EventArgs) Handles tBeatValue.LostFocus
		Dim a As Double
		If Double.TryParse(tBeatValue.Text, a) Then
			If a <= 0.0# Or a >= 1000.0# Then tBeatValue.BackColor = Color.FromArgb(&HFFFFC0C0) Else tBeatValue.BackColor = Nothing

			tBeatValue.Text = a
		End If
	End Sub



	Private Sub ApplyBeat(xRatio As Double, xDisplay As String)
		SortByVPositionInsertion()

		Dim xUndo As UndoRedo.LinkedURCmd = Nothing
		Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
		Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

		RedoChangeMeasureLengthSelected(192 * xRatio, xUndo, xRedo)

		Dim xIndices(LBeat.SelectedIndices.Count - 1) As Integer
		LBeat.SelectedIndices.CopyTo(xIndices, 0)


		For Each xI1 As Integer In xIndices
			Dim dLength As Double = (xRatio * 192.0R) - MeasureLength(xI1)
			Dim dRatio As Double = xRatio * 192.0R / MeasureLength(xI1)

			Dim xBottom As Double = 0
			For xI2 As Integer = 0 To xI1 - 1
				xBottom += MeasureLength(xI2)
			Next
			Dim xUpBefore As Double = xBottom + MeasureLength(xI1)
			Dim xUpAfter As Double = xUpBefore + dLength

			Select Case BeatChangeMode
				Case 1
case2:              Dim xI0 As Integer

					If NTInput Then
						For xI0 = 1 To UBound(Notes)
							If Notes(xI0).VPosition >= xUpBefore Then Exit For
							If Notes(xI0).VPosition + Notes(xI0).Length >= xUpBefore Then
								RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, Notes(xI0).Length + dLength, xUndo, xRedo)
								Notes(xI0).Length += dLength
							End If
						Next
					Else
						For xI0 = 1 To UBound(Notes)
							If Notes(xI0).VPosition >= xUpBefore Then Exit For
						Next
					End If

					For xI9 As Integer = xI0 To UBound(Notes)
						RedoLongNoteModify(Notes(xI9), Notes(xI9).VPosition + dLength, Notes(xI9).Length, xUndo, xRedo)
						Notes(xI9).VPosition += dLength
					Next

				Case 2
					If dLength < 0 Then
						If NTInput Then
							Dim xI0 As Integer = 1
							Dim xU As Integer = UBound(Notes)
							Do While xI0 <= xU
								If Notes(xI0).VPosition < xUpAfter Then
									If Notes(xI0).VPosition + Notes(xI0).Length >= xUpAfter And Notes(xI0).VPosition + Notes(xI0).Length < xUpBefore Then
										Dim nLen As Double = xUpAfter - Notes(xI0).VPosition - 1.0R
										RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, nLen, xUndo, xRedo)
										Notes(xI0).Length = nLen
									End If
								ElseIf Notes(xI0).VPosition < xUpBefore Then
									If Notes(xI0).VPosition + Notes(xI0).Length < xUpBefore Then
										RedoRemoveNote(Notes(xI0), xUndo, xRedo)
										RemoveNote(xI0)
										xI0 -= 1
										xU -= 1
									Else
										Dim nLen As Double = Notes(xI0).Length - xUpBefore + Notes(xI0).VPosition
										RedoLongNoteModify(Notes(xI0), xUpBefore, nLen, xUndo, xRedo)
										Notes(xI0).Length = nLen
										Notes(xI0).VPosition = xUpBefore
									End If
								End If
								xI0 += 1
							Loop
						Else
							Dim xI0 As Integer
							Dim xI9 As Integer
							For xI0 = 1 To UBound(Notes)
								If Notes(xI0).VPosition >= xUpAfter Then Exit For
							Next
							For xI9 = xI0 To UBound(Notes)
								If Notes(xI9).VPosition >= xUpBefore Then Exit For
							Next

							For xI8 As Integer = xI0 To xI9 - 1
								RedoRemoveNote(Notes(xI8), xUndo, xRedo)
							Next
							For xI8 As Integer = xI9 To UBound(Notes)
								Notes(xI8 - xI9 + xI0) = Notes(xI8)
							Next
							ReDim Preserve Notes(UBound(Notes) - xI9 + xI0)
						End If
					End If

					GoTo case2

				Case 3
					If NTInput Then
						For xI0 As Integer = 1 To UBound(Notes)
							If Notes(xI0).VPosition < xBottom Then
								If Notes(xI0).VPosition + Notes(xI0).Length > xUpBefore Then
									RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, Notes(xI0).Length + dLength, xUndo, xRedo)
									Notes(xI0).Length += dLength
								ElseIf Notes(xI0).VPosition + Notes(xI0).Length > xBottom Then
									Dim nLen As Double = ((Notes(xI0).Length + Notes(xI0).VPosition - xBottom) * dRatio) + xBottom - Notes(xI0).VPosition
									RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, nLen, xUndo, xRedo)
									Notes(xI0).Length = nLen
								End If
							ElseIf Notes(xI0).VPosition < xUpBefore Then
								If Notes(xI0).VPosition + Notes(xI0).Length > xUpBefore Then
									Dim nLen As Double = ((xUpBefore - Notes(xI0).VPosition) * dRatio) + Notes(xI0).VPosition + Notes(xI0).Length - xUpBefore
									Dim nVPos As Double = ((Notes(xI0).VPosition - xBottom) * dRatio) + xBottom
									RedoLongNoteModify(Notes(xI0), nVPos, nLen, xUndo, xRedo)
									Notes(xI0).Length = nLen
									Notes(xI0).VPosition = nVPos
								Else
									Dim nLen As Double = Notes(xI0).Length * dRatio
									Dim nVPos As Double = ((Notes(xI0).VPosition - xBottom) * dRatio) + xBottom
									RedoLongNoteModify(Notes(xI0), nVPos, nLen, xUndo, xRedo)
									Notes(xI0).Length = nLen
									Notes(xI0).VPosition = nVPos
								End If
							Else
								RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition + dLength, Notes(xI0).Length, xUndo, xRedo)
								Notes(xI0).VPosition += dLength
							End If
						Next
					Else
						Dim xI0 As Integer
						Dim xI9 As Integer
						For xI0 = 1 To UBound(Notes)
							If Notes(xI0).VPosition >= xBottom Then Exit For
						Next
						For xI9 = xI0 To UBound(Notes)
							If Notes(xI9).VPosition >= xUpBefore Then Exit For
						Next

						For xI8 As Integer = xI0 To xI9 - 1
							Dim nVP As Double = ((Notes(xI8).VPosition - xBottom) * dRatio) + xBottom
							RedoLongNoteModify(Notes(xI0), nVP, Notes(xI0).Length, xUndo, xRedo)
							Notes(xI8).VPosition = nVP
						Next

						'GoTo case2

						For xI8 As Integer = xI9 To UBound(Notes)
							RedoLongNoteModify(Notes(xI8), Notes(xI8).VPosition + dLength, Notes(xI8).Length, xUndo, xRedo)
							Notes(xI8).VPosition += dLength
						Next
					End If

			End Select

			MeasureLength(xI1) = xRatio * 192.0R
			LBeat.Items(xI1) = Add3Zeros(xI1) & ": " & xDisplay
		Next
		UpdateMeasureBottom()
		'xUndo &= vbCrLf & xUndo2
		'xRedo &= vbCrLf & xRedo2

		LBeat.SelectedIndices.Clear()
		For xI1 As Integer = 0 To UBound(xIndices)
			LBeat.SelectedIndices.Add(xIndices(xI1))
		Next

		AddUndo(xUndo, xBaseRedo.Next)
		SortByVPositionInsertion()
		UpdatePairing()
		CalculateTotalPlayableNotes()
		CalculateGreatestVPosition()
		RefreshPanelAll()
		POStatusRefresh()
	End Sub

	Private Sub BBeatApply_Click(sender As Object, e As EventArgs) Handles BBeatApply.Click
		Dim xxD As Integer = nBeatD.Value
		Dim xxN As Integer = nBeatN.Value
		Dim xxRatio As Double = xxN / xxD

		ApplyBeat(xxRatio, xxRatio & " ( " & xxN & " / " & xxD & " ) ")
	End Sub

	Private Sub BBeatApplyV_Click(sender As Object, e As EventArgs) Handles BBeatApplyV.Click
		Dim a As Double
		If Double.TryParse(tBeatValue.Text, a) Then
			If a <= 0.0# Or a >= 1000.0# Then Media.SystemSounds.Hand.Play() : Exit Sub

			Dim xxD As Long = GetDenominator(a)

			ApplyBeat(a, a & IIf(xxD > 10000, String.Empty, " ( " & (a * xxD) & " / " & xxD & " ) "))
		End If
	End Sub


	Private Sub BHStageFile_Click(sender As Object, e As EventArgs) Handles BHStageFile.Click, BHBanner.Click, BHBackBMP.Click, BHMissBMP.Click
		Dim xDiag As New OpenFileDialog With {
		.Filter = Strings.FileType._image & "|*.bmp;*.png;*.jpeg;*.jpg;*.gif|" &
				   Strings.FileType._all & "|*.*",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName)),
		.DefaultExt = "png"
	}

		If xDiag.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub
		InitPath = ExcludeFileName(xDiag.FileName)

		If ReferenceEquals(sender, BHStageFile) Then
			THStageFile.Text = GetFileName(xDiag.FileName)
		ElseIf ReferenceEquals(sender, BHBanner) Then
			THBanner.Text = GetFileName(xDiag.FileName)
		ElseIf ReferenceEquals(sender, BHBackBMP) Then
			THBackBMP.Text = GetFileName(xDiag.FileName)
		ElseIf ReferenceEquals(sender, BHMissBMP) Then
			THMissBMP.Text = GetFileName(xDiag.FileName)
			hBMP(0) = THMissBMP.Text
		End If
	End Sub

	Private Sub BHWavFile_Click(sender As Object, e As EventArgs) Handles BHLandMine.Click, BHPreview.Click
		Dim xDiag As New OpenFileDialog With {
		.Filter = Strings.FileType._wave & "|*.wav;*.ogg;*.mp3;*.flac|" &
				   Strings.FileType.WAV & "|*.wav|" &
				   Strings.FileType.OGG & "|*.ogg|" &
				   Strings.FileType.MP3 & "|*.mp3|" &
				   Strings.FileType.FLAC & "|*.flac|" &
				   Strings.FileType._all & "|*.*",
		.InitialDirectory = IIf(ExcludeFileName(FileName) = String.Empty, InitPath, ExcludeFileName(FileName)),
		.DefaultExt = "wav"
	}

		If xDiag.ShowDialog = Forms.DialogResult.Cancel Then Exit Sub

		If ReferenceEquals(sender, BHLandMine) Then
			InitPath = ExcludeFileName(xDiag.FileName)
			THLandMine.Text = GetFileName(xDiag.FileName)
			hWAV(0) = THLandMine.Text
		ElseIf ReferenceEquals(sender, BHPreview) Then
			InitPath = ExcludeFileName(xDiag.FileName)
			THPreview.Text = GetFileName(xDiag.FileName)
		End If
	End Sub

	Private Sub Switches_CheckedChanged(sender As Object, e As EventArgs) Handles _
POHeaderSwitch.CheckedChanged,
POGridSwitch.CheckedChanged,
POWaveFormSwitch.CheckedChanged,
POWAVSwitch.CheckedChanged,
POBMPSwitch.CheckedChanged,
POBeatSwitch.CheckedChanged,
POExpansionSwitch.CheckedChanged

		Try
			Dim Source As CheckBox = CType(sender, CheckBox)
			Dim Target As Panel = Nothing

			If sender Is Nothing Then : Exit Sub
			ElseIf sender Is POHeaderSwitch Then : Target = POHeaderInner
			ElseIf sender Is POGridSwitch Then : Target = POGridInner
			ElseIf sender Is POWaveFormSwitch Then : Target = POWaveFormInner
			ElseIf sender Is POWAVSwitch Then : Target = POWAVInner
			ElseIf sender Is POBMPSwitch Then : Target = POBMPInner
			ElseIf sender Is POBeatSwitch Then : Target = POBeatInner
			ElseIf sender Is POExpansionSwitch Then : Target = POExpansionInner
			End If

			Target.Visible = Source.Checked

		Catch ex As Exception

		End Try
	End Sub

	Private Sub Expanders_CheckChanged(sender As Object, e As EventArgs) Handles _
POHeaderExpander.CheckedChanged,
POGridExpander.CheckedChanged,
POWaveFormExpander.CheckedChanged,
POWAVExpander.CheckedChanged,
POBeatExpander.CheckedChanged

		Try
			Dim Source As CheckBox = CType(sender, CheckBox)
			Dim Target As Panel = Nothing
			'Dim TargetParent As Panel = Nothing

			If sender Is Nothing Then : Exit Sub
			ElseIf ReferenceEquals(sender, POHeaderExpander) Then : Target = POHeaderPart2 ' : TargetParent = POHeaderInner
			ElseIf ReferenceEquals(sender, POGridExpander) Then : Target = POGridPart2 ' : TargetParent = POGridInner
			ElseIf ReferenceEquals(sender, POWaveFormExpander) Then : Target = POWaveFormPart2 ' : TargetParent = POWaveFormInner
			ElseIf ReferenceEquals(sender, POWAVExpander) Then : Target = POWAVPart2 ' : TargetParent = POWaveFormInner
			ElseIf ReferenceEquals(sender, POBeatExpander) Then : Target = POBeatPart2 ' : TargetParent = POWaveFormInner
			End If

			Target.Visible = Source.Checked

		Catch ex As Exception

		End Try

	End Sub

	Private Sub VerticalResizer_MouseDown(sender As Object, e As MouseEventArgs) Handles POWAVResizer.MouseDown, POBMPResizer.MouseDown, POBeatResizer.MouseDown, POExpansionResizer.MouseDown
		tempResize = e.Y
	End Sub

	Private Sub HorizontalResizer_MouseDown(sender As Object, e As MouseEventArgs) Handles POptionsResizer.MouseDown, SpL.MouseDown, SpR.MouseDown
		tempResize = e.X
	End Sub

	Private Sub POResizer_MouseMove(sender As Object, e As MouseEventArgs) Handles POWAVResizer.MouseMove, POBMPResizer.MouseMove, POBeatResizer.MouseMove, POExpansionResizer.MouseMove
		If e.Button <> MouseButtons.Left Then Exit Sub
		If e.Y = tempResize Then Exit Sub

		Try
			Dim Source As Button = CType(sender, Button)
			Dim Target As Panel = Source.Parent

			Dim xHeight As Integer = Target.Height + e.Y - tempResize
			If xHeight < 10 Then xHeight = 10
			Target.Height = xHeight

			Target.Refresh()
		Catch ex As Exception

		End Try
	End Sub

	Private Sub POptionsResizer_MouseMove(sender As Object, e As MouseEventArgs) Handles POptionsResizer.MouseMove
		If e.Button <> MouseButtons.Left Then Exit Sub
		If e.X = tempResize Then Exit Sub

		Try
			Dim xWidth As Integer = POptionsScroll.Width - e.X + tempResize
			If xWidth < 25 Then xWidth = 25
			POptionsScroll.Width = xWidth

			Refresh()
			Application.DoEvents()
		Catch ex As Exception

		End Try
	End Sub

	Private Sub SpR_MouseMove(sender As Object, e As MouseEventArgs) Handles SpR.MouseMove
		If e.Button <> MouseButtons.Left Then Exit Sub
		If e.X = tempResize Then Exit Sub

		Try
			Dim xWidth As Integer = PMainR.Width - e.X + tempResize
			If xWidth < 0 Then xWidth = 0
			PMainR.Width = xWidth

			ToolStripContainer1.Refresh()
			Application.DoEvents()
		Catch ex As Exception

		End Try
	End Sub

	Private Sub SpL_MouseMove(sender As Object, e As MouseEventArgs) Handles SpL.MouseMove
		If e.Button <> MouseButtons.Left Then Exit Sub
		If e.X = tempResize Then Exit Sub

		Try
			Dim xWidth As Integer = PMainL.Width + e.X - tempResize
			If xWidth < 0 Then xWidth = 0
			PMainL.Width = xWidth

			ToolStripContainer1.Refresh()
			Application.DoEvents()
		Catch ex As Exception

		End Try
	End Sub

	Private Sub mnGotoMeasure_Click(sender As Object, e As EventArgs) Handles mnGotoMeasure.Click
		Dim s = InputBox(Strings.Messages.PromptEnterMeasure, "Enter Measure")

		Dim i As Integer
		If Integer.TryParse(s, i) Then
			If i < 0 Or i > 999 Then
				Exit Sub
			End If

			PanelVScroll(PanelFocus) = -MeasureBottom(i)
		End If
	End Sub
	Private Sub Reloadbms()
		'KMouseDown = -1
		ReDim SelectedNotes(-1)
		KMouseOver = -1
		If ClosingPopSave() Then Exit Sub
		OpenBMS(File.ReadAllText(FileName, TextEncoding))
		ClearUndo()
		SetFileName(FileName)
		NewRecent(FileName)
		SetIsSaved(True)
		'pIsSaved.Visible = Not IsSaved
	End Sub

End Class