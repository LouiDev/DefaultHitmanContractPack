using System;
using GTA;
using GTA.UI;
using HitmanMod.API;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("Celebrity trouble", 2200, EContractDifficulty.Difficult)]
    class Celebrity : IHitmanContract
    {
        Ped target;
        Ped guard1;
        Ped guard2;
        Blip blip;

        public void OnStart()
        {
            var pos = Game.Player.Character.Position.Around(200f);
            var spawnPos = World.GetNextPositionOnSidewalk(World.GetNextPositionOnStreet(pos, true));

            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Aaargh! I'm so fucking angry right now!",
                "~b~Ricky: ~w~This prick stole MY TV-Show! I want you to take him out.",
                "~b~Ricky: ~w~But be careful, he has some bodyguards with him.",
                "~b~Ricky: ~w~I marked his position on your map, now get to work!"
            }, () =>
            {
                target = World.CreatePed(new Model(PedHash.Business01AMM), spawnPos);
                target.IsPersistent = true;
                target.Weapons.Give(WeaponHash.Pistol, 20, false, true);
                target.Task.Wander();

                guard1 = World.CreatePed(new Model(PedHash.CartelGuards01GMM), spawnPos.Around(2f));
                guard1.IsPersistent = true;
                guard1.Weapons.Give(WeaponHash.SMG, 200, true, true);
                guard2 = World.CreatePed(new Model(PedHash.CartelGuards01GMM), spawnPos.Around(2f));
                guard2.Weapons.Give(WeaponHash.AssaultShotgun, 200, true, true);
                guard2.IsPersistent = true;

                new PedGroup
                {
                    { target, true },
                    { guard1, false },
                    { guard2, false }
                };

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
                        Complete();
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
            var cashBonus = 0;
            if (guard1 != null && guard1.Exists() && guard1.IsDead)
                if (guard2 != null && guard2.Exists() && guard2.IsDead)
                    cashBonus = new Random().Next(500, 701);

            if (cashBonus > 0)
            {
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
                Notification.PostTicker($"~g~Optional task completed: ~w~~n~Kill the guards beforehand~n~~g~+${cashBonus} cash bonus", true);
            } else
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);

            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "Good job! Money is on the way to your bank account rn!");

            Cleanup();
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
            Main.HitmanEngine.CompleteContract(cashBonus);
        }

        void Cleanup()
        {
            if (target != null && target.Exists())
                target.IsPersistent = false;
            if (guard1 != null && guard1.Exists())
                guard1.IsPersistent = false;
            if (guard2 != null && guard2.Exists())
                guard2.IsPersistent = false;
            if (blip != null && blip.Exists())
                blip.Delete();
        }

        public void OnHitmanVision()
        {
            if (target != null && target.Exists())
                if (Game.Player.Character.Position.DistanceTo2D(target.Position) < 25f)
                    Screen.ShowHelpText("You can sense that his bodyguards are very closely walking behind him");
                else
                    Screen.ShowHelpText("You can't sense anything right now");
            else
                Screen.ShowHelpText("You can't sense anything right now");
        }
    }
}
