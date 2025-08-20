using GTA;
using GTA.UI;
using GTA.Math;
using HitmanMod.API;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("The unknown target", 1509, EContractDifficulty.Normal)]
    class UnknownTarget : IHitmanContract
    {
        Ped target;
        Ped client;
        Blip clientBlip;
        Blip targetBlip;
        int step;

        public void OnStart()
        {
            step = -1;

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Okay so this one's a bit strange...",
                "~b~Ricky: ~w~I don't know who your target is",
                "~b~Ricky: ~w~BUT, I know he will meet a client",
                "~b~Ricky: ~w~I'll mark you the client on your map",
                "~b~Ricky: ~w~Follow him and take out the target when they meet"
            }, () =>
            {
                step = 0;

                Vector3 spawnPos = World.GetNextPositionOnSidewalk(World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(250f), true));
                target = World.CreateRandomPed(spawnPos);
                target.IsPersistent = true;

                client = World.CreateRandomPed(World.GetNextPositionOnSidewalk(World.GetNextPositionOnStreet(spawnPos.Around(100f))));
                client.IsPersistent = true;

                TaskSequence seq = new TaskSequence();
                seq.AddTask.FollowNavMeshTo(target.Position.Around(3f), PedMoveBlendRatio.Walk);
                seq.AddTask.ChatTo(target);
                client.Task.PerformSequence(seq);
                seq.Close(false);

                clientBlip = client.AddBlip();
                clientBlip.Color = BlipColor.Blue;
                clientBlip.Name = "Client";
                clientBlip.FlashInterval = 500;
                clientBlip.FlashTimeLeft = 4000;
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (target != null && target.Exists() && target.IsDead)
                Complete();

            if (step == 0)
            {
                if (target.Position.DistanceTo2D(client.Position) <= 2f)
                {
                    step = 1;
                    targetBlip = Main.HitmanEngine.AttachTargetBlipToPed(target, true);
                    target.Task.ChatTo(client);

                    if (clientBlip != null && clientBlip.Exists())
                        clientBlip.Delete();
                }
            }
        }

        public void OnAborted()
        {
            Cleanup();
        }

        void Fail()
        {
            Cleanup();
            Main.HitmanEngine.FailContract();
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract failed", "You useless prick! We're done.");
        }

        void Complete() 
        {
            Cleanup();
            Main.HitmanEngine.CompleteContract();
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "Good job, sending money right now!");
        }

        void Cleanup()
        {
            if (client != null && client.Exists())
                client.IsPersistent = false;
            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (clientBlip != null && clientBlip.Exists())
                clientBlip.Delete();
            if (targetBlip != null && targetBlip.Exists())
                targetBlip.Delete();
        }

        public void OnHitmanVision()
        {
            Screen.ShowHelpText("You can't sense anything right now");
        }
    }
}
