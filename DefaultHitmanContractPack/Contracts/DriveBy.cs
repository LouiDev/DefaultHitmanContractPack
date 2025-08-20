using System;
using HitmanMod.API;
using GTA;
using GTA.UI;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("Stock market crash", 645, EContractDifficulty.Easy)]
    class DriveBy : IHitmanContract
    {
        Ped target;
        Blip blip;
        Vehicle vehicle;

        public void OnStart()
        {
            var pedHashes = new PedHash[] { PedHash.Bevhills02AMM, PedHash.Eastsa01AMM, PedHash.MexLabor01AMM, PedHash.Soucent01AMM, PedHash.Genstreet02AMY };
            var spawnPos = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(260f), true);
            var vehicleHashes = new VehicleHash[] {  VehicleHash.Adder, VehicleHash.Habanero, VehicleHash.FQ2, VehicleHash.Asterope };

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Hey kiddo, I've got a job for you.",
                "~b~Ricky: ~w~A guy just crashed the stock market big time.",
                "~b~Ricky: ~w~I want you to take him out, but make it look like a drive-by.",
                "~b~Ricky: ~w~I've marked him on your map, now get to work!"
            }, () =>
            {
                vehicle = World.CreateVehicle(new Model(vehicleHashes[new Random().Next(vehicleHashes.Length)]), spawnPos);
                vehicle.IsPersistent = true;

                target = World.CreatePed(new Model(pedHashes[new Random().Next(pedHashes.Length)]), spawnPos);
                target.IsPersistent = true;
                target.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                target.Task.CruiseWithVehicle(vehicle, 19f, VehicleDrivingFlags.AdjustCruiseSpeedBasedOnRoadSpeed | VehicleDrivingFlags.SteerAroundPeds | VehicleDrivingFlags.StopForPeds | VehicleDrivingFlags.StopForVehicles);

                blip = Main.HitmanEngine.AttachTargetBlipToPed(target);
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (target != null)
                if (target.Exists())
                    if (target.IsDead)
                    {
                        var cashBonus = 0;
                        if (target.IsInVehicle())
                        {
                            cashBonus = new Random().Next(200, 400);
                            Notification.PostTicker($"~g~Optional task completed: ~w~~n~Kill the target in a drive-by ~n~~g~+${cashBonus} cash bonus", true);
                            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
                        } else
                            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);
                        Complete(cashBonus);
                    }
        }

        public void OnAborted()
        {
            Cleanup();
        }

        void Complete(int cashBonus = 0)
        {
            Cleanup();

            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "A job well done! Sending you the money rn!");
            Main.HitmanEngine.CompleteContract(cashBonus);
        }

        void Fail()
        {
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract failed", "You useless prick! We're done.");
            Cleanup();
            Main.HitmanEngine.FailContract();
        }

        void Cleanup()
        {
            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (blip != null && blip.Exists())
                blip.Delete();
        }

        public void OnHitmanVision()
        {
            Screen.ShowHelpText("You can't sense anything right now");
        }
    }
}
