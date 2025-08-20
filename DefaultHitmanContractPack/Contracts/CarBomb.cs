using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.UI;
using GTA.Math;
using HitmanMod.API;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("Car Bomb", 4240, EContractDifficulty.Difficult)]
    class CarBomb : IHitmanContract
    {
        Vehicle vehicle;
        Ped ped;
        Blip blip;
        int progress;

        public void OnStart()
        {
            progress = -1;

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Alright here's the deal",
                "~b~Ricky: ~w~I've already planted a bomb in this guy's car",
                "~b~Ricky: ~w~You just need to get close to him before the time runs out",
                "~b~Ricky: ~w~in order to detonate it",
                "~b~Ricky: ~w~I've marked his location on your map, now get to work!"
            }, () =>
            {
                progress = 0;
                Vector3 spawnPos = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(500f));
                VehicleHash[] vehicleHashes = new VehicleHash[] { VehicleHash.Adder, VehicleHash.Habanero, VehicleHash.FQ2, VehicleHash.Asterope };
                vehicle = World.CreateVehicle(new Model(vehicleHashes[new Random().Next(vehicleHashes.Length)]), spawnPos);
                ped = World.CreateRandomPed(spawnPos);
                ped.IsPersistent = true;
                ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                ped.Task.CruiseWithVehicle(vehicle, 30f, VehicleDrivingFlags.AdjustCruiseSpeedBasedOnRoadSpeed | VehicleDrivingFlags.SteerAroundPeds | VehicleDrivingFlags.StopForPeds | VehicleDrivingFlags.StopForVehicles);
                blip = Main.HitmanEngine.AttachTargetBlipToPed(ped, true);
                Main.HitmanEngine.StartTimerEvent(4, 20, () => Fail());
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (ped != null && ped.Exists())
                if (ped.IsDead)
                    Complete();

            if (progress != -1)
            {
                if (progress > 0)
                    progress--;

                if (Game.Player.Character.Position.DistanceTo2D(vehicle.Position) <= 7f)
                {
                    progress += 2;
                    Screen.ShowHelpTextThisFrame($"Detonation progress: {progress}/400", false);

                    if (progress >= 400)
                    {
                        progress = -1;
                        Screen.ShowHelpText("~y~Success! Bomb will detonate shortly!");
                        Main.HitmanEngine.Delay(() =>
                        {
                            vehicle.Explode();
                        }, 0, 5);
                    }
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
        }

        void Complete() 
        {
            int cashBonus = 0;
            if (Game.Player.Character.Position.DistanceTo2D(vehicle.Position) <= 25f)
            {
                cashBonus = new Random().Next(400, 601);
                Notification.PostTicker($"~g~Optional task completed: ~w~~n~Be within 25m radius of the vehicle when the bomb detonates ~n~~g~+${cashBonus} cash bonus", true);
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
            } else
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);

            Cleanup();
            Main.HitmanEngine.CompleteContract(cashBonus);
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "Good job, sending money right now!");
        }

        void Cleanup()
        {
            if (blip != null && blip.Exists())
                blip.Delete();
            if (ped != null && ped.Exists())
                ped.IsPersistent = false;

            Main.HitmanEngine.AbortTimerEvent();
        }

        public void OnHitmanVision()
        {
            if (Game.Player.Character.Position.DistanceTo2D(vehicle.Position) <= 30f)
                Screen.ShowHelpText("You can sense that the target is close by");
            else
                Screen.ShowHelpText("You can't sense anything right now");
        }
    }
}
