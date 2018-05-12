Module Module1

    Sub Main()
        Dim battlefield As New Battlefield
        battlefield.Add(CombatantPlayer.Construct("Fenris"))
        battlefield.Add(CombatantAI.Construct("Goblin"))
        battlefield.Main()
    End Sub
    Private Function BuildMech() As CombatantPlayer
        Dim mechBaseBPRaw As New Queue(Of String)
        With mechBaseBPRaw
            .Enqueue("Fenris")
            .Enqueue("Name:Fenris")
            .Enqueue("Carry:100")
            .Enqueue("Speed:5")
            .Enqueue("Dodge:-5")
            .Enqueue("ShockCapacity:100")
            .Enqueue("Bodypart:Arm")
        End With

        Dim bodyparts As New List(Of Bodypart)
        Dim armRaw As New Queue(Of String)
        With armRaw
            .Enqueue("Arm")
            .Enqueue("Name:Arm")
            .Enqueue("Armour:5")
            .Enqueue("Agility:5")
            .Enqueue("ShockLoss:10")
            .Enqueue("Attack:Claws|0|0|3|1|90|45|0|15|10")
        End With
        bodyparts.Add(Bodypart.Construct(armRaw))
        Dim mech As CombatantPlayer = CombatantPlayer.Construct("Fenris", Bodypart.Construct(mechBaseBPRaw), bodyparts)
        Return mech
    End Function
End Module
