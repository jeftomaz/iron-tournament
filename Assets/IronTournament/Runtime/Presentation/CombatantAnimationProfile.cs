using System;
using System.Collections.Generic;
using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Presentation
{
    public enum CombatantAnimationState
    {
        None = 0,
        Idle = 1,
        Attack = 2,
        Hurt = 3,
        Wounded = 4,
        Down = 5
    }

    [Serializable]
    public sealed class SpriteAnimationSequence
    {
        [SerializeField] private CombatantAnimationState state;
        [SerializeField] private Sprite[] frames;
        [Min(1)] [SerializeField] private float framesPerSecond = 4;

        public CombatantAnimationState State => state;
        public IReadOnlyList<Sprite> Frames => frames ?? Array.Empty<Sprite>();
        public float FramesPerSecond => framesPerSecond;
    }

    [CreateAssetMenu(menuName = "Iron Tournament/Combatant Animation Profile", fileName = "CombatantAnimation")]
    public sealed class CombatantAnimationProfile : ScriptableObject
    {
        [SerializeField] private CombatantId combatant;
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private SpriteAnimationSequence[] sequences;

        public CombatantId Combatant => combatant;
        public Sprite FallbackSprite => fallbackSprite;

        public bool TryGet(CombatantAnimationState state, out SpriteAnimationSequence sequence)
        {
            sequence = null;
            if (state == CombatantAnimationState.None || sequences == null) return false;

            foreach (var candidate in sequences)
            {
                if (candidate == null || candidate.State != state) continue;
                sequence = candidate;
                return true;
            }

            return false;
        }

        public void Validate()
        {
            if (combatant == CombatantId.None || !Enum.IsDefined(typeof(CombatantId), combatant))
                throw new InvalidOperationException("A valid combatant is required by the animation profile.");
            if (fallbackSprite == null)
                throw new InvalidOperationException("A fallback sprite is required by the animation profile.");

            var registered = new HashSet<CombatantAnimationState>();
            if (sequences == null) return;
            foreach (var sequence in sequences)
            {
                if (sequence == null)
                    throw new InvalidOperationException("Animation sequences cannot contain null entries.");
                if (sequence.State == CombatantAnimationState.None ||
                    !Enum.IsDefined(typeof(CombatantAnimationState), sequence.State))
                    throw new InvalidOperationException("Every animation sequence requires a valid state.");
                if (!registered.Add(sequence.State))
                    throw new InvalidOperationException($"Animation state {sequence.State} is duplicated.");
                if (sequence.Frames.Count == 0)
                    throw new InvalidOperationException($"Animation state {sequence.State} requires frames.");
                if (sequence.FramesPerSecond <= 0 || float.IsNaN(sequence.FramesPerSecond) ||
                    float.IsInfinity(sequence.FramesPerSecond))
                    throw new InvalidOperationException($"Animation state {sequence.State} requires a valid speed.");
                foreach (var frame in sequence.Frames)
                    if (frame == null)
                        throw new InvalidOperationException($"Animation state {sequence.State} contains a null frame.");
            }
        }
    }
}
