using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using HitmanMod.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("Private jet shenanigans", 3421, EContractDifficulty.Normal)]
    class PrivateJet : IHitmanContract
    {
        Vehicle jet;
        Vehicle vehicle;
        Blip blip;
        Blip targetBlip;
        Ped target;
        int step = -1;
        bool keysCollected;

        public void OnStart()
        {
            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Listen up kid",
                "~b~Ricky: ~w~Do you know how to fly a jet?",
                "~b~Ricky: ~w~I don't care actually, you're doing the job anyway",
                "~b~Ricky: ~w~Just make it crash, that's it",
                "~b~Ricky: ~w~Make sure to get the keys first though",
                "~b~Ricky: ~w~Now get to work!"
            }, () =>
            {
                jet = World.CreateVehicle(new Model(-1214293858), new Vector3(-970.1592f, -2980.525f, 14.54827f), 60.81563f);
                jet.IsPersistent = true;
                jet.LockStatus = VehicleLockStatus.PlayerCannotEnter;

                switch (new Random().Next(0, 3))
                {
                    case 0:
                        vehicle = World.CreateVehicle(new Model(2006667053), new Vector3(-902.2531f, -2607.641f, 36.1114f), -29.13637f);
                        break;
                    case 1:
                        vehicle = World.CreateVehicle(new Model(2006667053), new Vector3(-926.1653f, -2675.412f, 35.21127f), -118.8208f);
                        break;
                    default:
                        vehicle = World.CreateVehicle(new Model(2006667053), new GTA.Math.Vector3(-972.6144f, -2671.682f, 36.10991f), 60.50084f);
                        break;
                }
                vehicle.IsPersistent = true;
                vehicle.LockStatus = VehicleLockStatus.PlayerCannotEnter;
                vehicle.Mods.PrimaryColor = VehicleColor.Green;

                blip = World.CreateBlip(new Vector3(-913.4f, -2655.9f, 14.1f));
                blip.Color = BlipColor.Yellow;
                blip.Name = "Destination";
                blip.FlashInterval = 500;
                blip.FlashTimeLeft = 4000;
                blip.ShowRoute = true;

                step = 0;
                keysCollected = false;
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (step > -1)
                if (keysCollected)
                {
                    Function.Call(Hash.SET_MAX_WANTED_LEVEL, 0);
                    Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player, 0, false);
                    Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, Game.Player, false);
                }

            if (target != null && target.Exists() && target.IsDead)
                Complete();

            if (step == 0)
            {
                Screen.ShowSubtitle("Drive to the ~y~destination");

                if (Game.Player.Character.Position.DistanceTo2D(blip.Position) <= 8f)
                {
                    step = 1;
                    if (blip != null && blip.Exists())
                        blip.Delete();
                }
            } else if (step == 1)
            {
                Screen.ShowSubtitle("Search the top deck for a ~g~green vehicle");

                if (Game.Player.Character.Position.DistanceTo(vehicle.Position) <= 8f)
                {
                    step = 2;

                    if (blip != null && blip.Exists())
                        blip.Delete();

                    blip = vehicle.AddBlip();
                    blip.Color = BlipColor.Blue;
                    blip.Name = "Vehicle";
                    blip.FlashInterval = 500;
                    blip.FlashTimeLeft = 4000;
                }
            } else if (step == 2)
            {
                Screen.ShowSubtitle("Get the keys from the ~b~trunk");

                if (IsBehindVehicle(vehicle))
                {
                    Screen.ShowHelpText("Press ~INPUT_CONTEXT~ to get the keys");

                    if (Game.IsControlJustReleased(Control.Context))
                    {
                        step = 3;

                        Game.Player.Character.Task.PlayAnimation("anim@scripted@player@freemode@tun_prep_grab_midd_ig3@male@", "tun_prep_grab_midd_ig3", 8f, -1, AnimationFlags.UpperBodyOnly);
                        Game.Player.Character.Weapons.Give(WeaponHash.Parachute, 1, false, true);

                        if (blip != null && blip.Exists())
                            blip.Delete();

                        blip = jet.AddBlip();
                        blip.Color = BlipColor.Blue;
                        blip.Name = "Private Jet";
                        blip.FlashInterval = 500;
                        blip.FlashTimeLeft = 4000;
                        blip.ShowRoute = true;

                        jet.LockStatus = VehicleLockStatus.Unlocked;
                        keysCollected = true;
                    }
                }
            } else if (step == 3)
            { 
                Screen.ShowSubtitle("Get into the ~b~jet");

                if (Game.Player.Character.IsInVehicle(jet))
                {
                    step = 4;

                    if (target == null)
                    {
                        target = World.CreateRandomPed(new Vector3(-921.7472f, -2946.718f, 13.94507f));
                        target.IsPersistent = true;
                        target.Task.EnterVehicle(jet, VehicleSeat.RightRear);

                        targetBlip = Main.HitmanEngine.AttachTargetBlipToPed(target, true);
                    } else
                    {
                        if (target.IsInVehicle(jet))
                            step = 5;
                    }
                }
            } else if (step == 4)
            {
                Screen.ShowSubtitle("Wait for the ~r~target ~w~to get in");

                if (!Game.Player.Character.IsInVehicle(jet))
                    step = 3;

                if (target.IsInVehicle(jet))
                    step = 5;
            } else if (step == 5)
            {
                Screen.ShowSubtitle("Make the ~b~jet ~w~crash");
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
            if (jet != null && jet.Exists())
                jet.IsPersistent = false;
            if (vehicle != null && vehicle.Exists())
                vehicle.IsPersistent = false;
            if (blip != null && blip.Exists())
                blip.Delete();
            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (targetBlip != null && targetBlip.Exists())
                targetBlip.Delete();

            Function.Call(Hash.SET_MAX_WANTED_LEVEL, 5);
        }

        public void OnHitmanVision()
        {
            if (step == 1)
            {
                if (vehicle != null && vehicle.Exists())
                {
                    if (Game.Player.Character.Position.DistanceTo(vehicle.Position) <= 30f)
                    {
                        Screen.ShowHelpText("You can sense that the vehicle is within a 30m radius");
                    } else
                    {
                        Screen.ShowHelpText("You can't sense anything right now");
                    }
                } else
                {
                    Screen.ShowHelpText("You can't sense anything right now");
                }
            } else if (step == 2)
            {
                Screen.ShowHelpText("You can sense that the keys are in the trunk");
            } else
            {
                Screen.ShowHelpText("You can't sense anything right now");
            }
        }

        bool IsBehindVehicle(Vehicle target, float maxDistance = 4f, float angleThreshold = 30f)
        {
            Vector3 vehiclePos = target.Position;
            Vector3 playerPos = Game.Player.Character.Position;

            if (playerPos.DistanceTo(vehiclePos) > maxDistance)
                return false;

            Vector3 vehicleForward = target.ForwardVector;
            Vector3 dirToPlayer = (playerPos - vehiclePos).Normalized;

            float angle = Vector3.Angle(vehicleForward, dirToPlayer);

            return angle > (180f - angleThreshold);
        }
    }
}
