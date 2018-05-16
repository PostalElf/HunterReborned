Public Class Bodypart
#Region "Constructors"
    Public Shared Function Construct(ByVal rawdata As Queue(Of String)) As Bodypart
        Dim bp As New Bodypart
        bp._Name = rawdata.Dequeue()         'remove header
        While rawdata.Count > 0
            Dim ln As String() = rawdata.Dequeue.Split(":")
            Dim header As String = ln(0).Trim
            Dim entry As String = ln(1).Trim
            bp.Construct(header, entry)
        End While
        Return bp
    End Function
    Public Sub Construct(ByVal header As String, ByVal entry As String)
        Select Case header
            Case "Name" : _Name = entry
            Case "IsVital" : _IsVital = CBool(entry)
            Case "Weight" : _BonusWeight = CInt(entry)
            Case "Carry" : _BonusCarry = CInt(entry)
            Case "Speed" : _BonusSpeed = CInt(entry)
            Case "Dodge" : _BonusDodge = CInt(entry)
            Case "ShockCapacity" : _BonusShockCapacity = CInt(entry)

            Case "Agility" : Agility = CInt(entry)
            Case "Armour" : Armour = CInt(entry)
            Case "Health" : Health = CInt(entry)
            Case "ShockAbsorb" : ShockAbsorb = CDbl(entry)
            Case "ShockLoss" : _ShockLoss = CInt(entry)
            Case "Attack" : Attack = Attack.Construct(entry)
        End Select
    End Sub
    Public Function Export() As Queue(Of String)
        Dim total As New Queue(Of String)
        With total
            .Enqueue(Name)
            .Enqueue("IsVital:" & _IsVital.ToString)
            .Enqueue("Weight:" & BonusWeight)
            .Enqueue("Carry:" & BonusCarry)
            .Enqueue("Speed:" & BonusSpeed)
            .Enqueue("Dodge:" & BonusDodge)
            .Enqueue("ShockCapacity:" & BonusShockCapacity)

            .Enqueue("Agility:" & Agility)
            .Enqueue("Armour:" & Armour)
            .Enqueue("Health:" & Health)
            .Enqueue("ShockAbsorb:" & ShockAbsorb)
            .Enqueue("ShockLoss:" & ShockLoss)

            If Attack Is Nothing = False Then .Enqueue("Attack:" & Attack.Export)
        End With
        Return total
    End Function
#End Region

#Region "Personal Identifiers"
    Private _Name As String
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property
    Public Owner As Combatant

    Public Overrides Function ToString() As String
        Return Name
    End Function
#End Region

#Region "Events"
    Public Event IsMissed(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart)
    Public Event IsHit(ByVal attacker As Combatant, ByVal attack As Attack, ByVal target As Combatant, ByVal targetBp As Bodypart, ByVal isFullHit As Boolean)
    Public Event IsDestroyed(ByVal target As Combatant, ByVal targetBp As Bodypart)

    Private Sub HandlerShieldOverloaded(ByVal shield As Shield, ByVal overloadShock As Integer, ByVal overloadDamage As Integer)
        DamageSustained += overloadDamage
    End Sub
#End Region

#Region "Combatant Bonuses"
    Private _BonusWeight As Integer
    Public ReadOnly Property BonusWeight As Integer
        Get
            Return _BonusWeight
        End Get
    End Property
    Private _BonusCarry As Integer
    Public ReadOnly Property BonusCarry As Integer
        Get
            Return _BonusCarry
        End Get
    End Property
    Private _BonusSpeed As Integer
    Public ReadOnly Property BonusSpeed As Integer
        Get
            Return _BonusSpeed
        End Get
    End Property
    Private _BonusDodge As Integer
    Public ReadOnly Property BonusDodge As Integer
        Get
            Return _BonusDodge
        End Get
    End Property
    Private _BonusShockCapacity As Integer
    Public ReadOnly Property BonusShockCapacity As Integer
        Get
            Return _BonusShockCapacity
        End Get
    End Property
#End Region

#Region "BP Specific Properties"
    Private _IsVital As Boolean
    Public ReadOnly Property IsVital As Boolean
        Get
            Return _IsVital
        End Get
    End Property
    Private Agility As Integer
    Private Armour As Integer

    Private _DamageSustained As Integer
    Private Property DamageSustained As Integer
        Get
            Return _DamageSustained
        End Get
        Set(ByVal value As Integer)
            _DamageSustained = value
            If _damageSustained > Health Then RaiseEvent IsDestroyed(Owner, Me)
        End Set
    End Property
    Private Health As Integer
    Private ShockAbsorb As Double
    Private _ShockLoss As Integer
    Public ReadOnly Property ShockLoss As Integer
        Get
            Return _ShockLoss
        End Get
    End Property

    Private _Attack As Attack
    Public Property Attack As Attack
        Get
            Return _Attack
        End Get
        Set(ByVal value As Attack)
            _Attack = value
            _Attack.Bodypart = Me
        End Set
    End Property
    Private AttackCooldown As Integer
    Public ReadOnly Property AttackReady As Boolean
        Get
            If Attack Is Nothing Then Return False
            If AttackCooldown > 0 Then Return False

            Return True
        End Get
    End Property

    Private _Shield As Shield
    Public Property Shield As Shield
        Get
            Return _Shield
        End Get
        Set(ByVal value As Shield)
            If value Is Nothing = False Then
                _Shield = value
                AddHandler _Shield.IsOverloaded, AddressOf HandlerShieldOverloaded
            Else
                RemoveHandler _Shield.IsOverloaded, AddressOf HandlerShieldOverloaded
                _Shield = value
            End If
        End Set
    End Property
    Public ReadOnly Property HasActiveShield As Boolean
        Get
            If Shield Is Nothing Then Return False
            If Shield.IsActive = False Then Return False

            Return True
        End Get
    End Property
#End Region

    Public Sub Tick()
        If AttackCooldown > 0 Then AttackCooldown -= 1
    End Sub
    Public Sub IsAttacked(ByVal attacker As Combatant, ByVal attack As Attack)
        Dim roll As Integer = Rng.Next(1, 101)
        If roll <= attack.Accuracy - Owner.Dodge - Agility Then
            'attack hits; check for shield
            If Owner.ActiveShield Is Nothing = False Then
                'shielded; add damage to shield instead
                Owner.ActiveShield.AbsorbAttack(attacker, attack)
            Else
                'no shield; roll for penetration
                AttackPenetration(attacker, attack)
            End If
        Else
            'attack misses
            RaiseEvent IsMissed(attacker, attack, Owner, Me)
        End If
    End Sub
    Private Sub AttackPenetration(ByVal attacker As Combatant, ByVal attack As Attack)
        Dim damage As Integer
        Dim isFullHit As Boolean
        Dim formerOwner As Combatant = Owner            'used to hold a reference to owner in the event of bodypart destruction (which causes owner = nothing)

        Dim roll As Integer = Rng.Next(1, 101)
        If roll <= Attack.Penetration - Armour Then
            'full hit
            damage = Attack.DamageFull
            isFullHit = True
        Else
            'glancing hit
            damage = Attack.DamageGlancing
            isFullHit = False
        End If

        'apply damage
        RaiseEvent IsHit(attacker, attack, formerOwner, Me, isFullHit)
        DamageSustained += damage

        'apply shock
        Dim shock As Integer = Convert.ToInt32(damage * (1 - ShockAbsorb - Attack.ShockModifier))
        If shock <= 0 Then shock = 1
        formerOwner.Shock += shock
    End Sub
End Class
