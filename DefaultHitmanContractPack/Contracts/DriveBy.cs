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
            var vehicleHashes = new VehicleHash[] {  VehicleHash.Sanctus, VehicleHash.Adder, VehicleHash.Habanero };

            vehicle = World.CreateVehicle(new Model(vehicleHashes[new Random().Next(vehicleHashes.Length)]), spawnPos);
            vehicle.IsPersistent = true;

            target = World.CreatePed(new Model(pedHashes[new Random().Next(pedHashes.Length)]), spawnPos);
            target.IsPersistent = true;
            target.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            target.Task.CruiseWithVehicle(vehicle, 19f, VehicleDrivingFlags.AdjustCruiseSpeedBasedOnRoadSpeed);

            blip = Main.HitmanEngine.AttachTargetBlipToPed(target);

            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract details", "I've marked you a guy which just crashed the stock market big time. Deal with him! Make sure it looks like a drive-by.");
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
    }
}
