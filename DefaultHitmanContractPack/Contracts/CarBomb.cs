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
    [ContractAttributes("Car Bomb", 1644, EContractDifficulty.Normal)]
    class CarBomb : IHitmanContract
    {
        

        public void OnStart()
        {
            
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
    }
}
