using System;
using GTA;
using GTA.UI;
using GTA.Math;
using HitmanMod.API;
using GTA.Native;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("The construction worker", 1260, EContractDifficulty.Normal)]
    class ConstructionWorker : IHitmanContract
    {
        Ped target;
        Ped[] peds;
        Blip searchBlip;
        Blip destBlip;
        int step;

        public void OnStart()
        {
            step = -1;

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Hey kiddo, I've got a job for you.",
                "~b~Ricky: ~w~A construction worker fucked up my cousin's new house BIG TIME.",
                "~b~Ricky: ~w~I want you to take him out!",
                "~b~Ricky: ~w~I've marked the location on your map, now get to work!"
            }, () =>
            {
                var destPos = new Vector3(122.9156f, -349.298f, 41.91484f);
                destBlip = World.CreateBlip(destPos);
                destBlip.Color = BlipColor.Yellow;
                destBlip.Name = "Destination";
                destBlip.ShowRoute = true;

                step = 0;
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
            {
                Main.HitmanEngine.ShowClientMessage("Ricky", "Contract failed", "You useless prick! We're done.");
                Fail();
            }

            if (step == 0)
            {
                Main.HitmanEngine.DrawMarker(destBlip.Position, 0.8f, 0.8f, System.Drawing.Color.Yellow);

                if (World.GetDistance(Game.Player.Character.Position, destBlip.Position) <= 4f)
                {
                    peds = new Ped[]
                    {
                        World.CreatePed(new Model(PedHash.Construct01SMY), new Vector3(63.9f, -326f, 67f)),
                        World.CreatePed(new Model(PedHash.Construct01SMY), new Vector3(73f, -351f, 67f)),
                        World.CreatePed(new Model(PedHash.Construct01SMY), new Vector3(58f, -346f, 67f)),
                        World.CreatePed(new Model(PedHash.Construct01SMY), new Vector3(57f, -337f, 67f))
                    };

                    foreach (var ped in peds)
                    {
                        ped.Task.StandStill(-1);
                        ped.IsPersistent = true;
                        ped.Heading = new Random().Next(100);
                    }

                    target = peds[new Random().Next(peds.Length)];
                    target.Task.ClearAll();
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, target, "WORLD_HUMAN_SMOKING", 0, true);

                    searchBlip = World.CreateBlip(new Vector3(67.8f, -338f, 67f), 80f);
                    searchBlip.Name = "Search area";
                    searchBlip.FlashInterval = 500;
                    searchBlip.FlashTimeLeft = 4000;
                    searchBlip.Color = BlipColor.Yellow;

                    if (destBlip != null && destBlip.Exists())
                        destBlip.Delete();

                    Main.HitmanEngine.ShowClientMessage("Ricky", "More information", "He's on top of that building ~b~smoking a cigarette~w~. Climb the crane to get a clear shot. Use a sniper rifle and I'll send you a little bonus.");

                    step = 1;
                }
            } else if (step == 1)
            {
                foreach (var ped in peds)
                {
                    if (ped == target)
                    {
                        if (ped != null && ped.Exists())
                            if (ped.IsDead)
                                Complete();
                    } else
                    {
                        if (ped != null && ped.Exists())
                            if (ped.IsDead)
                            {
                                Main.HitmanEngine.ShowClientMessage("Ricky", "Contract failed", "You useless prick! That was the wrong dude! We're done.");
                                Fail();
                            }
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
            var cashBonus = 0;
            if (IsSniper(target.CauseOfDeath))
            {
                cashBonus = new Random().Next(400, 601);
                Notification.PostTicker($"~g~Optional task completed: ~w~~n~Assasinate the target with a sniper rifle. ~n~~g~+${cashBonus} cash bonus", true);
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
            } else
            {
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);
            }

            Cleanup();
            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract complete", "Good job. Sending you the money now.");
            Main.HitmanEngine.CompleteContract(cashBonus);
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
        }

        void Cleanup()
        {
            if (destBlip != null && destBlip.Exists())
                destBlip.Delete();

            if (searchBlip != null && searchBlip.Exists())
                searchBlip.Delete();

            if (peds != null)
            {
                foreach (var ped in peds)
                {
                    if (ped != null && ped.Exists())
                        ped.IsPersistent = false;
                }
            }
        }

        bool IsSniper(WeaponHash hash)
        {
            return hash == WeaponHash.SniperRifle || hash == WeaponHash.HeavySniper || hash == WeaponHash.HeavySniperMk2;
        }

        public void OnHitmanVision()
        {
            if (step == 0)
            {
                Screen.ShowHelpText("You can't sense anything right now");
            } else if (step == 1)
            {
                Screen.ShowHelpText("You can sense that the target is smoking standing on a rooftop wearing a construction worker outfit");
            }
        }
    }
}
