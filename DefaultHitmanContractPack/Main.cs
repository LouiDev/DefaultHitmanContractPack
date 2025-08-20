using System.Collections.Generic;
using HitmanMod.API;
using DefaultHitmanContractPack.Contracts;

namespace DefaultHitmanContractPack
{
    [ContractPackAttributes("Ricky's contracts", "louidev", "1.1.0.0")]
    class Main : IHitmanContractPack
    {
        public static IHitmanEngine HitmanEngine;

        public List<IHitmanContract> Initialize(IHitmanEngine engine)
        {
            HitmanEngine = engine;

            return new List<IHitmanContract>
            {
                new DriveBy(),
                new WifeBanger(),
                new ConstructionWorker(),
                new Hacker(),
                new TheDate(),
                new UnknownTarget(),
                new PrivateJet(),
                new CarBomb(),
                new Celebrity(),
                new Soldier()
            };
        }
    }
}
