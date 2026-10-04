using System.Collections.Generic;

namespace IronTournament.Core
{
    public interface IBattle
    {
        BattleState State { get; }

        IReadOnlyList<AbilityId> AvailableActions { get; }

        ActionResult Submit(AbilityId action);
    }
}
