using GTA;
using GTA.UI;
using HitmanMod.API;
using System;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("The daughter's date", 1455, EContractDifficulty.Normal)]
    class TheDate : IHitmanContract
    {
        private Ped target;
        private Blip blip;

        public void OnStart()
        {
            var spawnPos = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(350f), true);

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Listen up!",
                "~b~Ricky: ~w~This guy is taking my DAUGHTER on a DATE!!",
                "~b~Ricky: ~w~This won't be happening!",
                "~b~Ricky: ~w~Run him over, QUICKLY!"
            }, () =>
            {
                target = World.CreatePed(new Model(PedHash.Business02AMY), spawnPos);
                target.IsPersistent = true;
                target.Task.Wander();

                blip = Main.HitmanEngine.AttachTargetBlipToPed(target, true);

                Main.HitmanEngine.StartTimerEvent(2, 0, () =>
                {
                    if (target != null) 
                        if (target.Exists())
                            if (target.IsAlive)
                                Fail();
                });
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (target != null)
                if (target.Exists())
                    if (target.IsDead)
                        Complete();
        }

        public void OnAborted()
        {
            Cleanup();
        }

        void Fail()
        {
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract failed", "You useless prick! We're done.");
            Cleanup();
            Main.HitmanEngine.FailContract();
        }

        void Complete() 
        {
            int cashBonus = 0;
            if (Game.Player.Character.IsInVehicle())
            {
                cashBonus = new Random().Next(200, 401);
                Notification.PostTicker($"~g~Optional task completed: ~w~~n~Run the target over ~n~~g~+${cashBonus} cash bonus", true);
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
            } else
            {
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);
            }

            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "Good job, sending money right now!");
            Cleanup();
            Main.HitmanEngine.CompleteContract(cashBonus);
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
        }

        void Cleanup()
        {
            Main.HitmanEngine.AbortTimerEvent();

            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (blip != null && blip.Exists())
                blip.Delete();
        }

        public void OnHitmanVision()
        {
            Screen.ShowHelpText("You can sense that the target is dressed like a business man");
        }
    }
}
