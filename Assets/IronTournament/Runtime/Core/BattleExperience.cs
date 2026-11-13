using System;

namespace IronTournament.Core
{
    public readonly struct ExperienceGain
    {
        public ExperienceGain(int attack, int defense) { Attack = attack; Defense = defense; }
        public int Attack { get; }
        public int Defense { get; }
    }

    public static class BattleExperience
    {
        public static ExperienceGain For(CombatantId opponent)
        {
            switch (opponent)
            {
                case CombatantId.Goblin: return new ExperienceGain(2, 1);
                case CombatantId.Skeleton: return new ExperienceGain(3, 1);
                case CombatantId.Knight: return new ExperienceGain(3, 2);
                case CombatantId.Werewolf: return new ExperienceGain(4, 2);
                case CombatantId.Vampire: return new ExperienceGain(4, 3);
                case CombatantId.Necromancer: return new ExperienceGain(5, 3);
                case CombatantId.DemonKing: return new ExperienceGain(0, 0);
                default: throw new ArgumentOutOfRangeException(nameof(opponent));
            }
        }

        internal static void Apply(CombatantState hero, CombatantId opponent)
        {
            var gain = For(opponent);
            if (gain.Attack > 0) hero.ApplyModifier(new ItemModifier(ItemModifierKind.Attack, gain.Attack));
            if (gain.Defense > 0) hero.ApplyModifier(new ItemModifier(ItemModifierKind.Defense, gain.Defense));
        }
    }
}
