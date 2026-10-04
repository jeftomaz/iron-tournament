namespace IronTournament.Core
{
    internal static class ItemRules
    {
        public const int HealingPotionAmount = 40;
        public const int HopeScrollHealPercent = 50;
        public const int HopeScrollThresholdPercent = 30;
        public const int FlameCloakBurn = 5;
        public const int GuardianWarriorAttack = 30;
        public const int GuardianRecoveryPercent = 30;

        public static bool IsAvailableTo(ItemId item, CombatantId hero)
        {
            switch (item)
            {
                case ItemId.HopeScroll:
                    return hero == CombatantId.Warrior;
                case ItemId.FlameCloak:
                case ItemId.BrotherhoodHorn:
                    return hero == CombatantId.Mage;
                default:
                    return true;
            }
        }

        public static void Apply(ItemConfiguration item, CombatantState hero)
        {
            for (var index = 0; index < item.Modifiers.Count; index++)
            {
                hero.ApplyModifier(item.Modifiers[index]);
            }

            switch (item.Id)
            {
                case ItemId.HealingPotion:
                    hero.Heal(HealingPotionAmount);
                    break;
                case ItemId.LifeElixir:
                    hero.Heal(hero.Stats.MaximumHealth);
                    break;
                case ItemId.HopeScroll:
                    hero.AddHopeScroll();
                    break;
                case ItemId.FlameCloak:
                    hero.EquipFlameCloak();
                    break;
                case ItemId.BrotherhoodHorn:
                    hero.AddGuardianHorn();
                    break;
            }
        }
    }
}
