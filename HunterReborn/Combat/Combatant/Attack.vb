Public Class Attack
#Region "Constructors"
    Public Shared Function Construct(ByVal rawdata As String) As Attack
        Dim attack As New Attack
        With attack
            Dim ln As String() = rawdata.Split("|")
            Dim c As New AutoIncrementer

            .Name = ln(c.N)
            .MinRange = Convert.ToInt32(ln(c.N))
            .MaxRange = Convert.ToInt32(ln(c.N))
            .Multiple = Convert.ToInt32(ln(c.N))
            .Cooldown = Convert.ToInt32(ln(c.N))
            .Accuracy = Convert.ToInt32(ln(c.N))
            .Penetration = Convert.ToInt32(ln(c.N))
            .ShockModifier = Convert.ToDouble(ln(c.N))
            .DamageType = Convert.ToInt32(ln(c.N))
            .DamageFull = Convert.ToInt32(ln(c.N))
            .DamageGlancing = Convert.ToInt32(ln(c.N))
        End With
        Return attack
    End Function
    Public Function Export() As String
        Dim total As String = ""
        total &= Name & "|"
        total &= MinRange & "|"
        total &= MaxRange & "|"
        total &= Multiple & "|"
        total &= Cooldown & "|"
        total &= Accuracy & "|"
        total &= Penetration & "|"
        total &= ShockModifier & "|"
        total &= DamageType & "|"
        total &= DamageFull & "|"
        total &= DamageGlancing
        Return total
    End Function
#End Region

#Region "Properties"
    Public Bodypart As Bodypart
    Public ReadOnly Property Ready As Boolean
        Get
            If Bodypart Is Nothing Then Return False
            Return Bodypart.AttackReady
        End Get
    End Property

    Public Name As String
    Public MinRange As Integer
    Public MaxRange As Integer
    Public Multiple As Integer
    Public Cooldown As Integer
    Public Accuracy As Integer
    Public Penetration As Integer
    Public ShockModifier As Double

    Public DamageType As eDamageType
    Public DamageFull As Integer
    Public DamageGlancing As Integer

    Public Overrides Function ToString() As String
        Dim total As String = Name & ":"
        total &= " [" & Accuracy & "%"
        If Multiple > 1 Then total &= " x" & Multiple
        total &= "] "
        total &= DamageGlancing & " - " & DamageFull & " " & DamageType.ToString
        total &= " (" & Penetration & "%)"
        Return total
    End Function
#End Region
End Class
