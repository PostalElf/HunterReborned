Public Class Battlefield
#Region "Reports"
    Private TurnNumber As Integer = 1
    Private Reports As New Queue(Of BattlefieldReport)
    Private Sub AddReport(ByVal report As String)
        Reports.Enqueue(New BattlefieldReport(TurnNumber, report))
    End Sub
    Public Sub ShowReports()
        While Reports.Count > 0
            Console.WriteLine(Reports.Dequeue.ToString)
        End While
    End Sub
#End Region

#Region "Combatants"
    Private Attackers As New List(Of Combatant)
    Private Defenders As New List(Of Combatant)
    Private ReadOnly Property GetCombatantList(ByVal combatant As Combatant) As List(Of Combatant)
        Get
            If TypeOf combatant Is CombatantAI Then
                Return Defenders
            ElseIf TypeOf combatant Is CombatantPlayer Then
                Return Attackers
            Else
                Throw New Exception("Unknown Combatant type.")
            End If
        End Get
    End Property
    Public Sub Add(ByVal combatant As Combatant)
        combatant.Battlefield = Me
        GetCombatantList(combatant).Add(combatant)
        combatant.BattlefieldSetup()

        AddHandler combatant.IsMoved, AddressOf HandlerMove
        AddHandler combatant.IsShocked, AddressOf HandlerShocked
        AddHandler combatant.IsDestroyed, AddressOf HandlerDestroyed
        AddHandler combatant.IsBodypartMissed, AddressOf HandlerMissed
        AddHandler combatant.IsBodypartHit, AddressOf HandlerHit
        AddHandler combatant.IsBodypartDestroyed, AddressOf HandlerBodypartDestroyed
    End Sub
    Public Function Contains(ByVal combatant As Combatant)
        If Attackers.Contains(combatant) Then Return True
        If Defenders.Contains(combatant) Then Return True
        Return False
    End Function

    Public Function GetHighestSpeed() As Integer
        Dim highestSpeed As Integer = -1
        Dim highestSpeedCombatant As Combatant = Nothing

        For Each c In Attackers
            If c.Speed > highestSpeed Then
                highestSpeedCombatant = c
                highestSpeed = c.Speed
            End If
        Next
        For Each c In Defenders
            If c.Speed > highestSpeed Then
                highestSpeedCombatant = c
                highestSpeed = c.Speed
            End If
        Next
        Return highestSpeed
    End Function

    Private Sub HandlerMove(ByVal combatant As Combatant, ByVal currentPosition As ePosition, ByVal targetPosition As ePosition)
        AddReport(combatant.Name & " moves from " & currentPosition.ToString & " to " & targetPosition.ToString & ".")
    End Sub
    Private Sub HandlerShocked(ByVal combatant As Combatant, ByVal value As Integer)
        AddReport(combatant.Name & " suffers " & value & " shock.")
    End Sub
    Private Sub HandlerDestroyed(ByVal combatant As Combatant)
        Dim targetList = GetCombatantList(combatant)
        targetList.Remove(combatant)

        AddReport(combatant.Name & " has been destroyed!!!")
    End Sub
    Private Sub HandlerMissed(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart)
        AddReport(attacker.Name & " missed " & target.Name & "'s " & targetBp.Name & " with " & attack.Name & ".")
    End Sub
    Private Sub HandlerHit(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart, ByVal isFullHit As Boolean)
        AddReport(attacker.Name & " hit " & target.Name & "'s " & targetBp.Name & " with " & attack.Name & "!")
    End Sub
    Private Sub HandlerBodypartDestroyed(ByVal target As Combatant, ByVal targetBp As Bodypart)
        AddReport(target.Name & "'s " & targetBp.Name & " is destroyed!!!")
    End Sub
#End Region

#Region "Main"
    Public Sub Main()
        While True
            Dim hasActed As Boolean = False
            For n = Attackers.Count - 1 To 0 Step -1
                Dim c As Combatant = Attackers(n)
                If c.Tick() = True Then hasActed = True
            Next
            For n = Defenders.Count - 1 To 0 Step -1
                Dim c As Combatant = Defenders(n)
                If c.Tick() = True Then hasActed = True
            Next

            If hasActed = True Then
                ShowReports()
                TurnNumber += 1

                'check if either side has won
                If Attackers.Count = 0 Then
                    'defenders win
                    AddReport("The defenders have won!")
                    Exit While
                ElseIf Defenders.Count = 0 Then
                    'attackers win
                    AddReport("The attackers have won!")
                    Exit While
                End If
            End If
        End While
    End Sub
    Public Function GetTargets(ByVal attacker As Combatant) As List(Of Combatant)
        If TypeOf attacker Is CombatantAI Then
            Return Attackers
        ElseIf TypeOf attacker Is CombatantPlayer Then
            Return Defenders
        Else
            Throw New Exception("Unrecognised combatant type")
        End If
    End Function
    Public Function GetTargetsWithinRange(ByVal attacker As Combatant, ByVal attack As Attack) As List(Of Combatant)
        Dim total As New List(Of Combatant)
        For Each target In GetTargets(attacker)
            Dim distance As Integer = target.BattlefieldPosition + attacker.BattlefieldPosition
            If distance >= attack.MinRange AndAlso distance <= attack.MaxRange Then total.Add(target)
        Next
        Return total
    End Function
#End Region

End Class
