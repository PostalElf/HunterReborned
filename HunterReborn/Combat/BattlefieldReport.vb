Public Class BattlefieldReport
    Public TurnNumber As Integer
    Public Value As String

    Public Sub New(ByVal _turnNumber As Integer, ByVal _value As String)
        TurnNumber = _turnNumber
        Value = _value
    End Sub
    Public Overrides Function ToString() As String
        Return "[" & TurnNumber.ToString("000") & "] " & Value
    End Function
End Class
