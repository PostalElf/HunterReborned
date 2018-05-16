Public Class Battlefield
#Region "Reports"
    Private TurnNumber As Integer = 1
    Private Reports As New Queue(Of BattlefieldReport)
    Private Sub AddReport(ByVal report As String, ByVal color As ConsoleColor)
        Reports.Enqueue(New BattlefieldReport(TurnNumber, report, color))
    End Sub
    Public Sub ShowReports()
        While Reports.Count > 0
            Dim report As BattlefieldReport = Reports.Dequeue
            If Console.ForegroundColor <> report.Colour Then Console.ForegroundColor = report.Colour
            Console.WriteLine(report.ToString)
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

        AddHandler combatant.IsMoved, AddressOf HandlerCombatantMove
        AddHandler combatant.IsShocked, AddressOf HandlerCombatantShocked
        AddHandler combatant.IsDestroyed, AddressOf HandlerCombatantDestroyed

        For Each bp In combatant.bodyparts
            AddHandler bp.IsMissed, AddressOf HandlerBodypartMissed
            AddHandler bp.IsHit, AddressOf HandlerBodypartHit
            AddHandler bp.IsDestroyed, AddressOf HandlerBodypartDestroyed

            If bp.Shield Is Nothing = False Then
                AddHandler bp.Shield.IsTurnedOn, AddressOf HandlerShieldTurnedOn
                AddHandler bp.Shield.IsTurnedOff, AddressOf HandlerShieldTurnedOff
                AddHandler bp.Shield.IsHit, AddressOf HandlerShieldHit
                AddHandler bp.Shield.IsOverloaded, AddressOf HandlerShieldOverloaded
            End If
        Next
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

    Private Sub HandlerCombatantMove(ByVal combatant As Combatant, ByVal currentPosition As ePosition, ByVal targetPosition As ePosition)
        AddReport(combatant.Name & " moves from " & currentPosition.ToString & " to " & targetPosition.ToString & ".", ConsoleColor.DarkGray)
    End Sub
    Private Sub HandlerCombatantShocked(ByVal combatant As Combatant, ByVal value As Integer)
        AddReport(combatant.Name & " suffers " & value & " shock.", ConsoleColor.White)
    End Sub
    Private Sub HandlerCombatantDestroyed(ByVal combatant As Combatant)
        Dim targetList = GetCombatantList(combatant)
        targetList.Remove(combatant)

        AddReport(combatant.Name & " has been destroyed!!!", ConsoleColor.DarkRed)
    End Sub
    Private Sub HandlerBodypartMissed(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart)
        AddReport(attacker.Name & " missed " & target.Name & "'s " & targetBp.Name & " with " & attack.Name & ".", ConsoleColor.DarkGray)
    End Sub
    Private Sub HandlerBodypartHit(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart, ByVal isFullHit As Boolean)
        Dim damage As Integer
        If isFullHit = True Then damage = attack.DamageFull Else damage = attack.DamageGlancing
        AddReport(attacker.Name & " hit " & target.Name & "'s " & targetBp.Name & " with " & attack.Name & " for " & damage & " " & attack.DamageType.ToString & "!", ConsoleColor.Gray)
    End Sub
    Private Sub HandlerBodypartDestroyed(ByVal target As Combatant, ByVal targetBp As Bodypart)
        AddReport(target.Name & "'s " & targetBp.Name & " is destroyed!!!", ConsoleColor.DarkRed)
    End Sub
    Private Sub HandlerShieldTurnedOn(ByVal shield As Shield)
        AddReport(shield.Owner.Name & " turned on its shield.", ConsoleColor.DarkGreen)
    End Sub
    Private Sub HandlerShieldTurnedOff(ByVal shield As Shield)
        AddReport(shield.Owner.Name & " turned off its shield.", ConsoleColor.DarkGreen)
    End Sub
    Private Sub HandlerShieldHit(ByVal shield As Shield, ByVal attacker As Combatant, ByVal attack As Attack)

    End Sub
    Private Sub HandlerShieldOverloaded(ByVal shield As Shield, ByVal overloadShock As Integer, ByVal overloadDamage As Integer)

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
                'check if either side has won
                Dim hasWon As Boolean = False
                If Attackers.Count = 0 Then
                    'defenders win
                    AddReport("The defenders have won!", ConsoleColor.Green)
                    hasWon = True
                ElseIf Defenders.Count = 0 Then
                    'attackers win
                    AddReport("The attackers have won!", ConsoleColor.Green)
                    hasWon = True
                End If

                'show reports
                ShowReports()

                'shortcircuit for victory
                If hasWon = True Then Exit While

                'advance turnnumber
                TurnNumber += 1
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
