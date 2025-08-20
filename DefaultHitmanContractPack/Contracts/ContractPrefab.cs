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
    [ContractAttributes("PREFAB", 0, EContractDifficulty.Easy)]
    class ContractPrefab : IHitmanContract
    {
        public void OnStart()
        {
            Main.HitmanEngine.StartPhoneInstructions(new string[]
            {
                ""
            }, () =>
            {
                // Here you can spawn your target, blip, etc.
            });
        }

        public void OnUpdate()
        {
            if (Game.Player.IsDead)
                Fail();
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
        }

        void Cleanup()
        {

        }

        public void OnHitmanVision()
        {
            Screen.ShowHelpText("You can't sense anything right now");
        }
    }
}
