Public Class Battlefield
    Public Sub New()
        For Each ePosition In [Enum].GetValues(GetType(ePosition))
            Attackers.Add(ePosition, New List(Of Combatant))
            Defenders.Add(ePosition, New List(Of Combatant))
        Next
    End Sub

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
    Private Attackers As New Dictionary(Of ePosition, List(Of Combatant))
    Private Defenders As New Dictionary(Of ePosition, List(Of Combatant))
    Private ReadOnly Property GetCombatantList(ByVal combatant As Combatant, Optional ByVal position As ePosition = -1) As List(Of Combatant)
        Get
            If position = -1 Then position = combatant.BattlefieldPosition

            If TypeOf combatant Is CombatantAI Then
                Return Defenders(position)
            ElseIf TypeOf combatant Is CombatantPlayer Then
                Return Attackers(position)
            Else
                Throw New Exception("Unknown Combatant type.")
            End If
        End Get
    End Property
    Private ReadOnly Property AttackersAll As List(Of Combatant)
        Get
            Dim total As New List(Of Combatant)
            For Each pos In [Enum].GetValues(GetType(ePosition))
                total.AddRange(Attackers(pos))
            Next
            Return total
        End Get
    End Property
    Private ReadOnly Property DefendersAll As List(Of Combatant)
        Get
            Dim total As New List(Of Combatant)
            For Each pos In [Enum].GetValues(GetType(ePosition))
                total.AddRange(Defenders(pos))
            Next
            Return total
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
        If AttackersAll.Contains(combatant) Then Return True
        If DefendersAll.Contains(combatant) Then Return True
        Return False
    End Function

    Public Function GetHighestSpeed() As Integer
        Dim highestSpeed As Integer = -1
        Dim highestSpeedCombatant As Combatant = Nothing

        For Each c In AttackersAll
            If c.Speed > highestSpeed Then
                highestSpeedCombatant = c
                highestSpeed = c.Speed
            End If
        Next
        For Each c In DefendersAll
            If c.Speed > highestSpeed Then
                highestSpeedCombatant = c
                highestSpeed = c.Speed
            End If
        Next
        Return highestSpeed
    End Function

    Private Sub HandlerMove(ByVal combatant As Combatant, ByVal currentPosition As ePosition, ByVal targetPosition As ePosition)
        Dim targetList As List(Of Combatant) = GetCombatantList(combatant, currentPosition)
        Dim newTargetList As List(Of Combatant) = GetCombatantList(combatant, targetPosition)
        targetList.Remove(combatant)
        newTargetList.Add(combatant)

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
            For n = AttackersAll.Count - 1 To 0 Step -1
                Dim c As Combatant = AttackersAll(n)
                If c.Tick() = True Then hasActed = True
            Next
            For n = DefendersAll.Count - 1 To 0 Step -1
                Dim c As Combatant = DefendersAll(n)
                If c.Tick() = True Then hasActed = True
            Next

            If hasActed = True Then
                ShowReports()
                TurnNumber += 1

                'check if either side has won
                If AttackersAll.Count = 0 Then
                    'defenders win
                    AddReport("The defenders have won!")
                    Exit While
                ElseIf DefendersAll.Count = 0 Then
                    'attackers win
                    AddReport("The attackers have won!")
                    Exit While
                End If
            End If
        End While
    End Sub
    Public Function GetTargets(ByVal attacker As Combatant) As List(Of Combatant)
        If TypeOf attacker Is CombatantAI Then
            Return AttackersAll
        ElseIf TypeOf attacker Is CombatantPlayer Then
            Return DefendersAll
        Else
            Throw New Exception("Unrecognised combatant type")
        End If
    End Function
    Public Function GetTargetsWithinRange(ByVal attacker As Combatant, ByVal attack As Attack) As List(Of Combatant)
        Dim minRange As Integer = attacker.BattlefieldPosition + attack.MinRange
        If minRange < 0 Then minRange = 0
        If minRange > ePosition.Back Then minRange = ePosition.Back
        Dim maxRange As Integer = attacker.BattlefieldPosition + attack.MaxRange
        If maxRange < 0 Then maxRange = 0
        If maxRange > ePosition.Back Then maxRange = ePosition.Back

        Dim total As New List(Of Combatant)
        For n = minRange To maxRange
            If TypeOf attacker Is CombatantAI Then
                total.AddRange(Attackers(n))
            ElseIf TypeOf attacker Is CombatantPlayer Then
                total.AddRange(Defenders(n))
            Else
                Throw New Exception("Unrecognised combatant type")
            End If
        Next
        Return total
    End Function
#End Region

End Class
