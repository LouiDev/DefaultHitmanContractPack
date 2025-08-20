using System;
using GTA;
using GTA.UI;
using GTA.Math;
using HitmanMod.API;

namespace DefaultHitmanContractPack.Contracts
{
    [ContractAttributes("The soldier", 50000, EContractDifficulty.Suicidal)]
    class Soldier : IHitmanContract
    {
        Ped target;
        Blip blip;

        public void OnStart()
        {
            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                "~b~Ricky: ~w~Okay, so this one's a bit... tricky",
                "~b~Ricky: ~w~This guy is a soldier, and he's not just any soldier, he's a trained killer",
                "~b~Ricky: ~w~I want you to take him out, but be careful, he's not going to go down easy",
                "~b~Ricky: ~w~I've marked his location on your map, now get to work!"
            }, () =>
            {
                Vector3 spawnPos = new Vector3(-2144.8f, 3272.9f, 32f).Around(10f);
                target = World.CreatePed(new Model(PedHash.Armymech01SMY), spawnPos);
                target.Armor = 100;
                target.Weapons.Give(WeaponHash.AdvancedRifle, 9999, true, true);
                target.Accuracy = 99;
                target.IsPersistent = true;
                blip = Main.HitmanEngine.AttachTargetBlipToPed(target, true);
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();

            if (target != null)
                if (target.Exists() && target.IsDead)
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
            int cashBonus = 0;
            if (!Game.Player.Character.IsInVehicle())
            {
                cashBonus = new Random().Next(10000, 15001);
                Notification.PostTicker($"~g~Optional task completed: ~w~~n~Don't be inside a vehicle when assasinating the target ~n~~g~+${cashBonus} cash bonus", true);
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsCompleted);
            } else
                Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.SidequestsFailed);

            Main.HitmanEngine.ShowClientMessage("Ricky", "Contract completed", "Good job, sending money right now!");
            Cleanup();
            Main.HitmanEngine.CompleteContract(cashBonus);
            Main.HitmanEngine.IncreasePlayerStat(EPlayerStats.TargetsAssasinated);
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
