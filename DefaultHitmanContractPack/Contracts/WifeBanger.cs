using System;
using HitmanMod.API;
using GTA;
using GTA.Math;
using GTA.UI;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("The secret lover", 1163, EContractDifficulty.Normal)]
    class WifeBanger : IHitmanContract
    {
        Vector3 spawnPos;
        Ped target;
        Blip marker;
        int step;
        int cashBonus;

        public void OnStart()
        {
            var possiblePedHashes = new PedHash[] { PedHash.Bevhills02AMM, PedHash.Eastsa01AMM, PedHash.MexLabor01AMM, PedHash.Soucent01AMM, PedHash.Genstreet02AMY };
            
            var pos = Game.Player.Character.Position.Around(200f);
            spawnPos = World.GetNextPositionOnSidewalk(World.GetNextPositionOnStreet(pos, true));

            step = -1;
            cashBonus = 0;

            Main.HitmanEngine.StartPhoneInstructions(new string[] {
                "~b~Ricky: ~w~Alright kiddo",
                "~b~Ricky: ~w~So this guy's been banging my wife recently",
                "~b~Ricky: ~w~Take care of him and I will pay you fairly",
                "~b~Ricky: ~w~He's waiting for an Uber",
                "~b~Ricky: ~w~This would be a great opportunity to get rid of him",
                "~b~Ricky: ~w~Now get to work",
            }, () =>
            {
                target = World.CreatePed(new Model(possiblePedHashes[new Random().Next(possiblePedHashes.Length)]), spawnPos);
                target.IsPersistent = true;
                target.Task.UseMobilePhone();

                step = 0;
            });
        }

        public void OnUpdate()
        {
            if (target != null)
                if (target.Exists())
                    if (target.IsDead)
                        Complete();

            if (Game.Player.IsDead)
                Fail();

            if (step == 0)
            {
                Screen.ShowSubtitle("Get inside a vehicle");

                if (Game.Player.Character.IsSittingInVehicle())
                {
                    step = 1;

                    if (marker == null)
                        marker = Main.HitmanEngine.AttachTargetBlipToPed(target);
                }
            } else if (step == 1)
            {
                Screen.ShowSubtitle("Pick up the ~r~target");

                if (!Game.Player.Character.IsSittingInVehicle())
                    step = 0;

                if (World.GetDistance(Game.Player.Character.Position, target.Position) <= 15f)
                {
                    Screen.ShowHelpText("Use your horn to signal the target to get in.");

                    if (Game.IsControlJustPressed(Control.VehicleHorn))
                    {
                        target.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Passenger);
                        step = 2;
                    }
                }
            } else if (step == 2)
            {
                if (Game.Player.Character.IsSittingInVehicle() && target.IsInVehicle(Game.Player.Character.CurrentVehicle))
                {
                    target.Task.ClearAll();
                    target.PlayAmbientSpeech("GENERIC_HI", SpeechModifier.Force);
                    step = 3;

                    Screen.ShowHelpText("Use your Hitman Vision to sense if there are any unwanted eyes watching");
                }
            } else if (step == 3)
            {
                Screen.ShowSubtitle("Eliminate the ~r~target");
            }
        }

        public void OnAborted()
        {
            Cleanup();
        }

        void Cleanup()
        {
            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (marker != null && marker.Exists())
                marker.Delete();
        }

        void Fail()
        {
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract failed", "You useless prick! I told you not to erouse cop attention! We're done.");
            Cleanup();
            Main.HitmanEngine.FailContract();
        }

        void Complete()
        {
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract complete", "Good job. Sending you the money now.");
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);

            if (AreTooManyEyesWatching())
            {
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);
                cashBonus = new Random().Next(500, 701);
                Notification.PostTicker($"~g~Optional task completed: ~w~~n~Eliminate the target in a quiet area ~n~~g~+${cashBonus} cash bonus", true);
            } else
            {
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
            }

            Cleanup();
            Main.HitmanEngine.CompleteContract(cashBonus);
        }

        public void OnHitmanVision()
        {
            if (step == 3)
            {
                if (AreTooManyEyesWatching())
                    Screen.ShowHelpText("You can sense that there are too many unwanted eyes watching");
                else
                    Screen.ShowHelpText("You can sense that it's quiet enough around here");
            } else
                Screen.ShowHelpText("You can't sense anything right now");
        }

        bool AreTooManyEyesWatching() => World.GetNearbyPeds(Game.Player.Character, 40f).Length > 6;
    }
}
