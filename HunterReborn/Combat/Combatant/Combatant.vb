Public MustInherit Class Combatant
#Region "Personal Identifiers"
    Protected _Name As String
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return Name & " [" & BattlefieldInitiative & "]"
    End Function
#End Region

#Region "Events"
    Public Event IsBodypartMissed(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart)
    Public Event IsBodypartHit(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart, ByVal isFullHit As Boolean)
    Public Event IsBodypartDestroyed(ByVal target As Combatant, ByVal targetBp As Bodypart)
    Public Event IsMoved(ByVal combatant As Combatant, ByVal currentPosition As ePosition, ByVal targetPosition As ePosition)
    Public Event IsShocked(ByVal target As Combatant, ByVal value As Integer)
    Public Event IsDestroyed(ByVal combatant As Combatant)

    Private Sub HandlerBodypartIsMissed(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart)
        RaiseEvent IsBodypartMissed(attacker, attack, target, targetBp)
    End Sub
    Private Sub HandlerBodypartIsHit(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart, ByVal isFullHit As Boolean)
        RaiseEvent IsBodypartHit(attacker, attack, target, targetBp, True)
    End Sub
    Private Sub HandlerBodypartIsDestroyed(ByVal target As Combatant, ByVal targetBp As Bodypart)
        RaiseEvent IsBodypartDestroyed(target, targetBp)

        target.Remove(targetBp)
        If target.HasVitals = False Then RaiseEvent IsDestroyed(target) : Exit Sub
        target.Shock += targetBp.ShockLoss
    End Sub

    Public Event ShieldIsHit(ByVal shield As Shield, ByVal attacker As Combatant, ByVal attack As Attack)
    Public Event ShieldIsOverloaded(ByVal shield As Shield, ByVal overloadShock As Integer, ByVal overloadDamage As Integer)
    Public Event ShieldIsTurnedOn(ByVal shield As Shield)
    Public Event ShieldIsTurnedOff(ByVal shield As Shield)

    Private Sub HandlerShieldIsHit(ByVal shield As Shield, ByVal attacker As Combatant, ByVal attack As Attack)
        RaiseEvent ShieldIsHit(shield, attacker, attack)
    End Sub
    Private Sub HandlerShieldIsOverloaded(ByVal shield As Shield, ByVal overloadShock As Integer, ByVal overloadDamage As Integer)
        Shock += overloadShock
        RaiseEvent ShieldIsOverloaded(shield, overloadShock, overloadDamage)
    End Sub
    Private Sub HandlerShieldIsTurnedOn(ByVal shield As Shield)
        'hook up events
        AddHandler shield.IsHit, AddressOf HandlerShieldIsHit
        AddHandler shield.IsOverloaded, AddressOf HandlerShieldIsOverloaded
        AddHandler shield.IsTurnedOn, AddressOf HandlerShieldIsTurnedOn
        AddHandler shield.IsTurnedOff, AddressOf HandlerShieldIsTurnedOff
        RaiseEvent ShieldIsTurnedOn(shield)

        'only one shield can be active at any time
        'turn off each shield that isn't the shield that just got turned on
        If BaseBodypart.Shield Is Nothing AndAlso BaseBodypart.Shield.Equals(shield) = False Then BaseBodypart.Shield.IsActive = False
        For Each bp In Bodyparts
            If bp.Shield Is Nothing AndAlso bp.Shield.Equals(shield) = False Then bp.Shield.IsActive = False
        Next
    End Sub
    Private Sub HandlerShieldIsTurnedOff(ByVal shield As Shield)
        RemoveHandler shield.IsHit, AddressOf HandlerShieldIsHit
        RemoveHandler shield.IsOverloaded, AddressOf HandlerShieldIsOverloaded
        RemoveHandler shield.IsTurnedOn, AddressOf HandlerShieldIsTurnedOn
        RemoveHandler shield.IsTurnedOff, AddressOf HandlerShieldIsTurnedOff
        RaiseEvent ShieldIsTurnedOff(shield)
    End Sub
#End Region

#Region "Battlefield"
    Public Battlefield As Battlefield
    Private _BattlefieldPosition As ePosition
    Public ReadOnly Property BattlefieldPosition
        Get
            Return _BattlefieldPosition
        End Get
    End Property
    Public Sub BattlefieldSetup()
        Const MaxPosition As Integer = 4
        _BattlefieldPosition = Rng.Next(0, MaxPosition)

        'randomise initiative
        BattlefieldInitiativeReset()
        BattlefieldInitiative += Rng.Next(1, 11)
    End Sub
    Private BattlefieldInitiative As Integer
    Private Sub BattlefieldInitiativeReset()
        BattlefieldInitiative = Battlefield.GetHighestSpeed - Speed + 10
    End Sub

    Private _Shock As Integer
    Public Property Shock As Integer
        Get
            Return _Shock
        End Get
        Set(ByVal value As Integer)
            _Shock += value
            RaiseEvent IsShocked(Me, value)

            If _Shock >= ShockCapacity Then
                RaiseEvent IsDestroyed(Me)
            End If
        End Set
    End Property
    Public ReadOnly Property ActiveShield As Shield
        Get
            If BaseBodypart.HasActiveShield = True Then Return BaseBodypart.Shield
            For Each bp In Bodyparts
                If bp.hasactiveshield = True Then Return bp.Shield
            Next
            Return Nothing
        End Get
    End Property

    Public MustOverride Function Tick() As Boolean
    Protected Function TickBase() As Boolean
        BattlefieldInitiative -= 1
        If BattlefieldInitiative <= 0 Then
            'reset initiative
            BattlefieldInitiativeReset()

            'tick bodyparts
            For n = Bodyparts.Count - 1 To 0 Step -1
                Dim bp As Bodypart = Bodyparts(n)
                bp.Tick()
            Next

            'return true to indicate that turn can be performed
            Return True
        End If

        Return False
    End Function
#End Region

#Region "Bodyparts"
    Protected BaseBodypart As Bodypart
    Protected Bodyparts As New List(Of Bodypart)
    Public ReadOnly Property HasVitals As Boolean
        Get
            For Each bp In Bodyparts
                If bp.isvital = True Then Return True
            Next
            Return False
        End Get
    End Property
    Protected Sub Add(ByVal bp As Bodypart)
        bp.Owner = Me
        Bodyparts.Add(bp)

        AddHandler bp.IsMissed, AddressOf HandlerBodypartIsMissed
        AddHandler bp.IsHit, AddressOf HandlerBodypartIsHit
        AddHandler bp.IsDestroyed, AddressOf HandlerBodypartIsDestroyed
    End Sub
    Protected Sub Remove(ByVal bp As Bodypart)
        bp.Owner = Nothing
        If Bodyparts.Contains(bp) Then Bodyparts.Remove(bp)
    End Sub
    Public Function GetTargetableBodyparts(ByVal attack As Attack) As List(Of Bodypart)
        Return Bodyparts
    End Function
    Public ReadOnly Property AttacksAll As List(Of Attack)
        Get
            Dim total As New List(Of Attack)
            If BaseBodypart.Attack Is Nothing = False Then total.Add(BaseBodypart.Attack)
            For Each bp In Bodyparts
                If bp.Attack Is Nothing = False Then total.Add(bp.Attack)
            Next
            Return total
        End Get
    End Property
    Public ReadOnly Property AttacksReady As List(Of Attack)
        Get
            Dim total As New List(Of Attack)
            For Each Attack In AttacksAll
                If Attack.Ready = True Then total.Add(Attack)
            Next
            Return total
        End Get
    End Property

    Public ReadOnly Property Weight As Integer
        Get
            Dim total As Integer = BaseBodypart.BonusWeight
            For Each bp In Bodyparts
                total += bp.BonusWeight
            Next
            Return total
        End Get
    End Property
    Public ReadOnly Property Carry As Integer
        Get
            Dim total As Integer = BaseBodypart.BonusCarry
            For Each bp In Bodyparts
                total += bp.BonusCarry
            Next
            Return total
        End Get
    End Property
    Private ReadOnly Property Encumbrance As Double
        Get
            If Weight = 0 Then Return 1
            If Carry = 0 Then Return 0

            Dim absEncumbrance As Integer = Math.Ceiling(Weight / Carry * 100)
            Select Case absEncumbrance
                Case Is < 50 : Return 1
                Case 51 To 70 : Return 0.75
                Case 71 To 85 : Return 0.5
                Case 86 To 100 : Return 0.25
                Case Else : Return 0.1
            End Select
        End Get
    End Property
    Public ReadOnly Property Speed As Integer
        Get
            Dim total As Integer = BaseBodypart.BonusSpeed
            For Each bp In Bodyparts
                total += bp.BonusSpeed
            Next

            'apply encumbrance
            total = Math.Ceiling(total * Encumbrance)
            If total <= 0 Then total = 1

            Return total
        End Get
    End Property
    Public ReadOnly Property Dodge As Integer
        Get
            Dim total As Integer = BaseBodypart.BonusDodge
            For Each bp In Bodyparts
                total += bp.BonusDodge
            Next
            Return total
        End Get
    End Property
    Public ReadOnly Property ShockCapacity
        Get
            Dim total As Integer = BaseBodypart.BonusShockCapacity
            For Each bp In Bodyparts
                total += bp.BonusShockCapacity
            Next
            Return total
        End Get
    End Property
#End Region

    Public Sub PerformsMove(ByVal target As ePosition)
        Dim oldPosition = _BattlefieldPosition
        _BattlefieldPosition = target

        RaiseEvent IsMoved(Me, oldPosition, target)
    End Sub
End Class
