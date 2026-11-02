using System;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class BattleCombatantAnimator : MonoBehaviour
    {
        private Image target;
        private CombatantAnimationProfile profile;
        private Sprite fallbackSprite;
        private SpriteAnimationSequence sequence;
        private CombatantAnimationState stableState = CombatantAnimationState.Idle;
        private float elapsed;
        private int frameIndex;
        private bool transient;

        public bool IsPlayingTransient => transient;
        public CombatantAnimationState State { get; private set; }

        public void Configure(CombatantAnimationProfile animationProfile, Sprite fallback)
        {
            if (fallback == null) throw new ArgumentNullException(nameof(fallback));
            if (animationProfile != null)
            {
                animationProfile.Validate();
            }

            EnsureTarget();
            profile = animationProfile;
            fallbackSprite = fallback;
            stableState = CombatantAnimationState.Idle;
            SetStableState(stableState);
        }

        public void ShowStatic(Sprite sprite)
        {
            if (sprite == null) throw new ArgumentNullException(nameof(sprite));
            EnsureTarget();
            profile = null;
            fallbackSprite = sprite;
            sequence = null;
            transient = false;
            State = CombatantAnimationState.None;
            target.sprite = sprite;
        }

        public void SetStableState(CombatantAnimationState state)
        {
            if (state != CombatantAnimationState.Idle && state != CombatantAnimationState.Wounded &&
                state != CombatantAnimationState.Down)
                throw new ArgumentOutOfRangeException(nameof(state));

            stableState = state;
            transient = false;
            StartState(state, true);
        }

        public bool PlayTransient(CombatantAnimationState state)
        {
            if (state != CombatantAnimationState.Attack && state != CombatantAnimationState.Hurt)
                throw new ArgumentOutOfRangeException(nameof(state));
            if (!TrySequence(state, out var animation)) return false;

            transient = true;
            Begin(animation, state);
            return true;
        }

        public void Cancel()
        {
            if (fallbackSprite == null) return;
            transient = false;
            StartState(stableState, true);
        }

        private void Awake() => EnsureTarget();

        private void OnDisable() => Cancel();

        private void Update()
        {
            if (sequence == null) return;

            elapsed += Time.unscaledDeltaTime;
            var frameDuration = 1f / sequence.FramesPerSecond;
            while (elapsed >= frameDuration)
            {
                elapsed -= frameDuration;
                frameIndex++;
                if (frameIndex < sequence.Frames.Count)
                {
                    target.sprite = sequence.Frames[frameIndex];
                    continue;
                }

                if (transient)
                {
                    transient = false;
                    StartState(stableState, true);
                }
                else
                {
                    frameIndex = 0;
                    target.sprite = sequence.Frames[frameIndex];
                }
                break;
            }
        }

        private void StartState(CombatantAnimationState state, bool useFallback)
        {
            if (TrySequence(state, out var animation))
            {
                Begin(animation, state);
                return;
            }

            if (state != CombatantAnimationState.Idle && TrySequence(CombatantAnimationState.Idle, out animation))
            {
                Begin(animation, CombatantAnimationState.Idle);
                return;
            }

            sequence = null;
            elapsed = 0;
            frameIndex = 0;
            State = state;
            if (useFallback)
            {
                EnsureTarget();
                target.sprite = fallbackSprite != null ? fallbackSprite : profile?.FallbackSprite;
            }
        }

        private bool TrySequence(CombatantAnimationState state, out SpriteAnimationSequence animation)
        {
            if (profile != null) return profile.TryGet(state, out animation);
            animation = null;
            return false;
        }

        private void Begin(SpriteAnimationSequence animation, CombatantAnimationState state)
        {
            EnsureTarget();
            sequence = animation;
            elapsed = 0;
            frameIndex = 0;
            State = state;
            target.sprite = animation.Frames[0];
        }

        private void EnsureTarget()
        {
            if (target == null) target = GetComponent<Image>();
        }
    }
}
