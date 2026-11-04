#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using IronTournament.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace IronTournament.Presentation.Tests
{
    public sealed class CombatantAnimationTests
    {
        private GameObject gameObject;
        private Texture2D texture;
        private Sprite first;
        private Sprite second;
        private CombatantAnimationProfile profile;

        [SetUp]
        public void SetUp()
        {
            texture = new Texture2D(2, 1);
            first = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
            second = Sprite.Create(texture, new Rect(1, 0, 1, 1), Vector2.zero);
            profile = ScriptableObject.CreateInstance<CombatantAnimationProfile>();
            Set(profile, "combatant", CombatantId.Warrior);
            Set(profile, "fallbackSprite", first);
            Set(profile, "sequences", new[]
            {
                Sequence(CombatantAnimationState.Idle, 20, first, second),
                Sequence(CombatantAnimationState.Attack, 20, second)
            });
            gameObject = new GameObject("Combatant", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(BattleCombatantAnimator));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
            UnityEngine.Object.DestroyImmediate(profile);
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [UnityTest]
        public IEnumerator ProfileLoopsIdleAndReturnsAfterTransientAnimation()
        {
            var animator = gameObject.GetComponent<BattleCombatantAnimator>();
            var image = gameObject.GetComponent<Image>();
            animator.Configure(profile, first);

            Assert.That(animator.State, Is.EqualTo(CombatantAnimationState.Idle));
            Assert.That(image.sprite, Is.EqualTo(first));
            yield return new WaitForSecondsRealtime(.06f);
            Assert.That(image.sprite, Is.EqualTo(second));

            Assert.That(animator.PlayTransient(CombatantAnimationState.Attack), Is.True);
            Assert.That(animator.IsPlayingTransient, Is.True);
            Assert.That(image.sprite, Is.EqualTo(second));
            yield return new WaitForSecondsRealtime(.11f);
            Assert.That(animator.IsPlayingTransient, Is.False);
            Assert.That(animator.State, Is.EqualTo(CombatantAnimationState.Idle));
        }

        [Test]
        public void MissingTransientStateUsesTheCurrentStableAnimation()
        {
            var animator = gameObject.GetComponent<BattleCombatantAnimator>();
            animator.Configure(profile, first);

            Assert.That(animator.PlayTransient(CombatantAnimationState.Hurt), Is.False);
            Assert.That(animator.State, Is.EqualTo(CombatantAnimationState.Idle));
            Assert.That(animator.IsPlayingTransient, Is.False);
        }

        [Test]
        public void InvalidProfileIsRejectedBeforeChangingTheSprite()
        {
            var invalid = ScriptableObject.CreateInstance<CombatantAnimationProfile>();
            try
            {
                Assert.Throws<InvalidOperationException>(() =>
                    gameObject.GetComponent<BattleCombatantAnimator>().Configure(invalid, first));
                Assert.That(gameObject.GetComponent<Image>().sprite, Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(invalid);
            }
        }

        private static SpriteAnimationSequence Sequence(
            CombatantAnimationState state,
            float framesPerSecond,
            params Sprite[] frames)
        {
            var sequence = new SpriteAnimationSequence();
            Set(sequence, "state", state);
            Set(sequence, "framesPerSecond", framesPerSecond);
            Set(sequence, "frames", frames);
            return sequence;
        }

        private static void Set(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Field {fieldName} was not found.");
            field.SetValue(target, value);
        }
    }
}
#endif
