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

            step = 0;
            cashBonus = 0;

            target = World.CreatePed(new Model(possiblePedHashes[new Random().Next(possiblePedHashes.Length)]), spawnPos);
            target.IsPersistent = true;
            target.Task.UseMobilePhone();

            marker = Main.HitmanEngine.AttachTargetBlipToPed(target);

            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract details", "Alright, so this guy I just marked on your map's been banging my wife recently. Take care of him and I will pay you fairly. Make sure you DO NOT erouse cop attention!");
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
                if (World.GetDistance(Game.Player.Character.Position, target.Position) <= 15f)
                {
                    Screen.ShowHelpText("Press ~g~E ~w~to listen what he says ont the phone.");

                    if (Game.IsKeyPressed(System.Windows.Forms.Keys.E))
                    {
                        step = 1;
                        Notification.PostTicker("You hear that he is waiting for an Uber. Organize a vehicle to disguise yourself as the Uber driver.", true);
                    }
                }
            } else if (step == 1)
            {
                if (Game.Player.Character.IsSittingInVehicle())
                {
                    if (World.GetDistance(Game.Player.Character.Position, target.Position) <= 40f)
                    {
                        Screen.ShowHelpText("Use your horn to signal the target to get in.");

                        if (Game.IsControlJustPressed(Control.VehicleHorn))
                        {
                            target.Task.EnterVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Passenger);
                            step = 2;
                        }
                    }
                }
            } else if (step == 2)
            {
                if (Game.Player.Character.IsSittingInVehicle() && target.IsInVehicle(Game.Player.Character.CurrentVehicle))
                {
                    target.Task.ClearAll();
                    target.PlayAmbientSpeech("GENERIC_HI", SpeechModifier.Force);
                    cashBonus = new Random().Next(400, 501);
                    Notification.PostTicker($"~g~Optional task completed: ~w~~n~Make the target enter your vehicle. ~n~~g~+${ cashBonus } cash bonus", true);
                    step = 3;
                }
            }

            if (Game.Player.Wanted.WantedLevel > 0)
                Fail();
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

            if (cashBonus > 0)
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
            else
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);

            Cleanup();
            Main.HitmanEngine.CompleteContract(cashBonus);
        }
    }
}
