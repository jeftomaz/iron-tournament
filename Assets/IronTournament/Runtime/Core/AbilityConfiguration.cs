using System;

namespace IronTournament.Core
{
    public sealed class AbilityConfiguration
    {
        public AbilityConfiguration(
            AbilityId id,
            string displayName,
            AbilityTarget target,
            bool consumesTurn)
        {
            if (!Enum.IsDefined(typeof(AbilityId), id) || id == AbilityId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (!Enum.IsDefined(typeof(AbilityTarget), target) || target == AbilityTarget.None)
            {
                throw new ArgumentOutOfRangeException(nameof(target));
            }

            Id = id;
            DisplayName = ConfigurationGuard.DisplayName(displayName, nameof(displayName));
            Target = target;
            ConsumesTurn = consumesTurn;
        }

        public AbilityId Id { get; }

        public string DisplayName { get; }

        public AbilityTarget Target { get; }

        public bool ConsumesTurn { get; }
    }
}
