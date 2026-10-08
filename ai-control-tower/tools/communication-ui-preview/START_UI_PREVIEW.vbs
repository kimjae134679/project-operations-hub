Option Explicit
Dim fs, previewRoot, previewData, shell, processEnvironment
Set fs = CreateObject("Scripting.FileSystemObject")
previewRoot = fs.GetParentFolderName(WScript.ScriptFullName)
previewData = previewRoot & "\preview-data"
If Not fs.FileExists(previewRoot & "\AIControlTower.exe") Or Not fs.FileExists(previewData & "\settings.json") Then
  MsgBox "Read-only preview executable or settings missing", 48, "Communication UI preview"
  WScript.Quit 2
End If
Set shell = CreateObject("WScript.Shell")
Set processEnvironment = shell.Environment("PROCESS")
processEnvironment("AI_CONTROL_TOWER_DATA") = previewData
shell.CurrentDirectory = previewRoot
shell.Run Chr(34) & previewRoot & "\AIControlTower.exe" & Chr(34) & " --local-view", 1, False
