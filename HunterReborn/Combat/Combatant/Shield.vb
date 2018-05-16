Public Class Shield
#Region "Personal Identifiers"
    Private _Name As String
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property
    Public Owner As Bodypart

    Public Overrides Function ToString() As String
        Return Name
    End Function
#End Region

#Region "Events"
    Public Event IsHit(ByVal shield As Shield, ByVal attacker As Combatant, ByVal attack As Attack)
    Public Event IsOverloaded(ByVal shield As Shield, ByVal overloadShock As Integer, ByVal overloadDamage As Integer)
    Public Event IsTurnedOn(ByVal shield As Shield)
    Public Event IsTurnedOff(ByVal shield As Shield)

    Private Sub IsHitHandler(ByVal Shield As Shield, ByVal attacker As Combatant, ByVal attack As Attack) Handles Me.IsHit
        Shield.DamageSustained += attack.DamageFull
        If Shield.DamageSustained > Shield.DamageCapacity Then RaiseEvent IsOverloaded(Shield, OverloadShock, OverloadDamage)
    End Sub
#End Region

    Private _IsActive As Boolean
    Public Property IsActive As Boolean
        Get
            Return _IsActive
        End Get
        Set(ByVal value As Boolean)
            'if shield property doesn't change then no need to raise events
            If value = _IsActive Then Exit Property

            'otherwise, set and raise events
            _IsActive = value
            If value = True Then
                RaiseEvent IsTurnedOn(Me)
            ElseIf value = False Then
                RaiseEvent IsTurnedOff(Me)
            End If
        End Set
    End Property
    Private DamageSustained As Integer
    Private DamageCapacity As Integer
    Public Sub AbsorbAttack(ByVal attacker As Combatant, ByVal attack As Attack)
        RaiseEvent IsHit(Me, attacker, attack)
    End Sub

    Private OverloadShock As Integer
    Private OverloadDamage As Integer
End Class
