Public Class BattlefieldReport
    Public TurnNumber As Integer
    Public Value As String
    Public Colour As ConsoleColor

    Public Sub New(ByVal _turnNumber As Integer, ByVal _value As String, ByVal _color As ConsoleColor)
        TurnNumber = _turnNumber
        Value = _value
        Colour = _color
    End Sub
    Public Overrides Function ToString() As String
        Return "[" & TurnNumber.ToString("000") & "] " & Value
    End Function
End Class
