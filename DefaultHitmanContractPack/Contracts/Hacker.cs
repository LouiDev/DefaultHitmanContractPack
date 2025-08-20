using System;
using GTA;
using GTA.UI;
using HitmanMod.API;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("Hacking the hacker", 1307, EContractDifficulty.Normal)]
    class Hacker : IHitmanContract
    {
        Ped target;
        Ped p1;
        Blip blip1;
        Blip targetBlip;
        Vehicle vehicle;

        int step;
        int hackProgression;
        int suspicion;
        int cashBonus;

        public void OnStart()
        {
            var spawnPos = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(250f), true);
            var vehicleHashes = new VehicleHash[] { VehicleHash.Sanctus, VehicleHash.Adder, VehicleHash.Habanero };

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Sup",
                "~b~Ricky: ~w~Your target is a well known hacker and an enemy of mine.",
                "~b~Ricky: ~w~So why don't we do some hacking for once?",
                "~b~Ricky: ~w~I've got the location of his friend.",
                "~b~Ricky: ~w~Hack him first to get the target's location."
            }, () =>
            {
                vehicle = World.CreateVehicle(new Model(vehicleHashes[new Random().Next(vehicleHashes.Length)]), spawnPos);
                vehicle.IsPersistent = true;

                p1 = World.CreateRandomPed(spawnPos);
                p1.IsPersistent = true;
                p1.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                p1.Task.CruiseWithVehicle(vehicle, 19f, VehicleDrivingFlags.AdjustCruiseSpeedBasedOnRoadSpeed | VehicleDrivingFlags.SteerAroundPeds | VehicleDrivingFlags.StopForPeds | VehicleDrivingFlags.StopForVehicles);

                blip1 = p1.AddBlip();
                blip1.Color = BlipColor.Blue;
                blip1.Name = "The hacker's son";
                blip1.FlashInterval = 500;
                blip1.FlashTimeLeft = 4000;

                step = 0;
            });

            step = -1;
            hackProgression = 0;
            suspicion = 0;
            cashBonus = 0;
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (step == 0)
            {
                if (World.GetDistance(Game.Player.Character.Position, p1.Position) <= 25f)
                {
                    if (suspicion < 300)
                    {
                        if (World.GetDistance(Game.Player.Character.Position, p1.Position) <= 10f)
                            suspicion++;
                        else if (suspicion > 0)
                            suspicion--;
                    } else
                    {
                        blip1.Color = BlipColor.Red;
                        blip1.FlashInterval = 500;
                        blip1.FlashTimeLeft = 4000;

                        p1.RelationshipGroup = new RelationshipGroup(0x84DCFAAD);

                        Main.HitmanEngine.ShowClientMessage("Ricky", "Contract update", "Great, I told you not to reouse suspicion! Now you have to get rid of him too!");
                        Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);

                        step = 1;
                    }

                    hackProgression++;

                    Screen.ShowHelpTextThisFrame($"Hack progression: ~g~{ (int)hackProgression / 6 }% ~n~~w~Suspicion: ~r~{ (int)suspicion / 3 }%", false);
                }

                if (hackProgression >= 600)
                {
                    SpawnTarget();

                    cashBonus = new Random().Next(500, 701);
                    Notification.PostTicker($"~g~Optional task completed: ~w~~n~Hack the friend without erousing suspicion ~n~~g~+${cashBonus} cash bonus", true);
                    Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);

                    step = 2;
                }
            } else if (step == 1)
            {
                if (p1 != null && p1.Exists())
                {
                    if (p1.IsDead)
                    {
                        SpawnTarget();
                        Main.HitmanEngine.ShowClientMessage("Ricky", "Contract update", "Alright, I just did the hacking myself. Marked the position on your map. And don't mess it up now!");

                        step = 2;
                    }
                }
            } else if (step == 2)
            {
                if (target != null && target.Exists())
                    if (target.IsDead)
                        Complete();
            }
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
            Cleanup();
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "Great job! I knew you could do it. Now, get out of there before the cops arrive!");
            Main.HitmanEngine.CompleteContract(cashBonus);
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
        }

        void Cleanup()
        {
            if (p1 != null && p1.Exists())
                p1.IsPersistent = false;
            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (blip1 != null && blip1.Exists())
                blip1.Delete();
            if (targetBlip != null && targetBlip.Exists())
                targetBlip.Delete();
            if (vehicle != null && vehicle.Exists())
                vehicle.IsPersistent = false;
        }

        void SpawnTarget()
        {
            var pedHashes = new PedHash[] { PedHash.Bevhills02AMM, PedHash.Eastsa01AMM, PedHash.MexLabor01AMM, PedHash.Soucent01AMM, PedHash.Genstreet02AMY };
            var targetPos = World.GetNextPositionOnSidewalk(World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(200f), true));
            target = World.CreatePed(new Model(pedHashes[new Random().Next(pedHashes.Length)]), targetPos);
            target.IsPersistent = true;
            target.Task.Wander();

            targetBlip = Main.HitmanEngine.AttachTargetBlipToPed(target);

            if (blip1 != null && blip1.Exists())
                blip1.Delete();
            if (p1 != null && p1.Exists())
                p1.IsPersistent = false;
            if (vehicle != null && vehicle.Exists())
                vehicle.IsPersistent = false;
        }

        public void OnHitmanVision()
        {
            Screen.ShowHelpText("You can't sense anything right now");
        }
    }
}
