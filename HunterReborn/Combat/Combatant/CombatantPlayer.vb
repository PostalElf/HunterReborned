Public Class CombatantPlayer
    Inherits Combatant

#Region "Constructors"
    Public Shared Function Construct(ByVal name As String) As CombatantPlayer
        Dim bodyparts As New List(Of Bodypart)
        Dim baseBodypart As New Bodypart

        Dim rawdata As Queue(Of String) = IO.BracketFileget("data/" & name & "-Design.txt", name)
        rawdata.Dequeue()             'remove name header
        While rawdata.Count > 0
            Dim ln As String() = rawdata.Dequeue.Split(":")
            Dim header As String = ln(0).Trim
            Dim entry As String = ln(1).Trim

            Select Case header
                Case "Bodypart"
                    Dim bpRaw As Queue(Of String) = IO.BracketFileget("data/" & name & "-Parts.txt", entry)
                    Dim bp As Bodypart = Bodypart.Construct(bpRaw)
                    Bodyparts.Add(bp)
                Case Else
                    BaseBodypart.Construct(header, entry)
            End Select
        End While

        Return Construct(name, baseBodypart, bodyparts)
    End Function
    Public Shared Function Construct(ByVal name As String, ByVal baseBodypart As Bodypart, ByVal bodyparts As List(Of Bodypart)) As CombatantPlayer
        Dim total As New CombatantPlayer
        With total
            ._Name = name
            .BaseBodypart = baseBodypart
            For Each bp In bodyparts
                .Add(bp)
            Next
        End With
        Return total
    End Function
    Public Sub Save()
        Dim partsPathname As String = "data/" & Name & "-Parts.txt"
        Dim partsRawAll As New List(Of Queue(Of String))

        Dim designPathname As String = "data/" & Name & "-Design.txt"
        Dim designRaw As New Queue(Of String)
        With designRaw
            .Enqueue(Name)

            'write baseBodypart
            Dim baseBodypartRaw As Queue(Of String) = BaseBodypart.Export
            baseBodypartRaw.Dequeue()
            While baseBodypartRaw.Count > 0
                .Enqueue(baseBodypartRaw.Dequeue)
            End While

            'write each bodypart
            For Each bp In Bodyparts
                .Enqueue("Bodypart:" & bp.Name)         'record name of bodypart to designRaw
                partsRawAll.Add(bp.Export)              'write bodypart spec to partsRawAll
            Next
        End With

        IO.BracketFilesave(designPathname, designRaw)
        IO.BracketFileSaveAll(partsPathname, partsRawAll)
    End Sub
#End Region


    Public Overrides Function Tick() As Boolean
        Dim canAct As Boolean = MyBase.TickBase()
        Return canAct
    End Function
End Class
