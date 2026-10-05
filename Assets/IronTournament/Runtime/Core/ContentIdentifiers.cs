namespace IronTournament.Core
{
    public enum CombatantId
    {
        None,
        Warrior,
        Mage,
        Goblin,
        Skeleton,
        Knight,
        Werewolf,
        Vampire,
        Necromancer,
        DemonKing
    }

    public enum CombatantSide
    {
        None,
        Player,
        Enemy
    }

    public enum AbilityId
    {
        None,
        BasicAttack,
        Guard,
        RevertTurn,
        RevertBattle,
        UseHopeScroll,
        ArmGuardian,
        Fury
    }

    public enum AbilityTarget
    {
        None,
        Self,
        Opponent
    }

    public enum ItemId
    {
        None,
        HopeScroll,
        FlameCloak,
        BrotherhoodHorn,
        HealingPotion,
        AttackGem,
        DefenseRune,
        PowerCrystal,
        LifeElixir,
        DivineDefense
    }

    public enum ItemModifierKind
    {
        None,
        MaximumHealth,
        Attack,
        Defense
    }
}
