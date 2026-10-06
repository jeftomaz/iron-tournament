#if UNITY_EDITOR
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using IronTournament.Bootstrap;
using IronTournament.Core;
using IronTournament.Content;
using UnityEditor;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace IronTournament.Presentation.Tests
{
    public sealed class BattleViewTests
    {
        private Scene scene;
        private GameObject canvas;
        private BattleBootstrap bootstrap;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            scene = EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Battle.unity",
                new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            canvas = scene.GetRootGameObjects().Single(root => root.name == "BattleCanvas");
            bootstrap = canvas.GetComponentInChildren<BattleBootstrap>();
            bootstrap.enabled = false;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator ConfiguredEnemiesUseSharedSceneAndCombatPerspective()
        {
            foreach (var id in new[] { CombatantId.Skeleton, CombatantId.Werewolf, CombatantId.Vampire, CombatantId.Necromancer, CombatantId.DemonKing })
            {
                if (id != CombatantId.Skeleton)
                {
                    yield return TearDown();
                    yield return SetUp();
                }
                var view = canvas.GetComponentInChildren<BattleView>();
                view.SelectEncounter(id);
                Assert.That(view.SelectedEncounter.Opponent.Id, Is.EqualTo(id));
                Assert.Throws<ArgumentException>(() => canvas.GetComponentInChildren<BattlePresenter>()
                    .Initialize(new Battle(PresentationState(), new SeededRandomSource(17))));
                Assert.That(view.GetCombatantRect(CombatantSide.Enemy).GetComponent<Image>().sprite.name, Is.EqualTo("west_0"));
                Assert.That(canvas.GetComponentsInChildren<Text>().Any(text => text.text == view.SelectedEncounter.Opponent.DisplayName), Is.True);
                string scenario = id switch
                {
                    CombatantId.Skeleton => "skeleton-graveyard",
                    CombatantId.Werewolf => "werewolf-ravine",
                    CombatantId.Vampire => "vampire-castle",
                    CombatantId.Necromancer => "necromancer-crypt",
                    _ => "demon-king-citadel"
                };
                Assert.That(canvas.GetComponentsInChildren<Image>().Any(image => image.sprite != null && image.sprite.name == scenario), Is.True);
                Assert.Throws<ArgumentOutOfRangeException>(() => view.SelectEncounter(CombatantId.None));
                Assert.That(view.SelectedEncounter.Opponent.Id, Is.EqualTo(id));
                StartPresentation();
                yield return null;
                var sprite = view.GetCombatantRect(CombatantSide.Enemy).GetComponent<Image>().sprite;
                Assert.That(sprite.name, Is.EqualTo("south-west_0"));
                Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.Throws<InvalidOperationException>(() => view.SelectEncounter(CombatantId.Goblin));
                foreach (var size in new[] { new Vector2(360, 640), new Vector2(1280, 720) })
                {
                    canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    canvas.GetComponent<RectTransform>().sizeDelta = size;
                    view.SendMessage("OnValidate");
                    yield return null;
                    var enemy = view.GetCombatantRect(CombatantSide.Enemy);
                    Assert.That(enemy.anchoredPosition.y, Is.GreaterThan(view.GetCombatantRect(CombatantSide.Player).anchoredPosition.y));
                }
            }
        }

        [UnityTest]
        public IEnumerator ConfiguredEnemiesUseExistingPresenterAndEffects()
        {
            foreach (var id in new[] { CombatantId.Skeleton, CombatantId.Werewolf, CombatantId.Vampire, CombatantId.Necromancer, CombatantId.DemonKing })
            {
                if (id != CombatantId.Skeleton)
                {
                    yield return TearDown();
                    yield return SetUp();
                }
                var view = canvas.GetComponentInChildren<BattleView>();
                view.SelectEncounter(id);
                var hero = ContentMapper.BuildCombatant(AssetDatabase.LoadAssetAtPath<CombatantDefinition>("Assets/IronTournament/Content/Warrior.asset"));
                var enemy = ContentMapper.BuildEncounter(view.SelectedEncounter).Opponent;
                var state = new BattleState(new CombatantState(hero, hero.BaseStats), new CombatantState(enemy, enemy.BaseStats));
                canvas.GetComponentInChildren<BattlePresenter>().Initialize(new Battle(state, new SeededRandomSource(17)));
                StartPresentation();
                yield return null;
                var expected = id switch
                {
                    CombatantId.Skeleton => (Health: 62, Attack: 20, Defense: 5),
                    CombatantId.Werewolf => (Health: 95, Attack: 30, Defense: 10),
                    CombatantId.Vampire => (Health: 110, Attack: 34, Defense: 12),
                    CombatantId.Necromancer => (Health: 125, Attack: 38, Defense: 14),
                    _ => (Health: 145, Attack: 44, Defense: 16)
                };
                int maximumHealth = expected.Health;
                Assert.That(enemy.BaseStats.Attack, Is.EqualTo(expected.Attack));
                Assert.That(enemy.BaseStats.Defense, Is.EqualTo(expected.Defense));
                Assert.That(HealthText("OpponentHealth"), Is.EqualTo($"HP {maximumHealth} / {maximumHealth}"));
                var menu = canvas.GetComponentInChildren<ActionMenu>();
                menu.GetComponentsInChildren<Button>().Single(button => button.name == "Attack").onClick.Invoke();
                yield return WaitForPlayback(canvas.GetComponentInChildren<BattleEventPlayer>());
                yield return null;
                Assert.That(HealthText("OpponentHealth"), Is.EqualTo($"HP {state.Opponent.CurrentHealth} / {maximumHealth}"));
                Assert.That(state.Opponent.CurrentHealth, Is.LessThan(maximumHealth));
                Assert.That(menu.GetComponentsInChildren<Button>().All(button => button.interactable), Is.True);
            }
        }

        [UnityTest]
        public IEnumerator StartButtonChangesIntroductionToCombatPerspective()
        {
            var button = canvas.GetComponentsInChildren<Button>(true).Single(item => item.name == "StartCombat");
            var images = canvas.GetComponentsInChildren<Image>();
            var player = images.Single(image => image.name == "Guerreiro");
            var opponent = images.Single(image => image.name == "Goblin");

            Assert.That(player.sprite.name, Is.EqualTo("east_0"));
            Assert.That(opponent.sprite.name, Is.EqualTo("west_0"));
            Assert.That(button.gameObject.activeInHierarchy, Is.True);
            button.onClick.Invoke();
            yield return null;

            Assert.That(player.sprite.name, Is.EqualTo("north-east_0"));
            Assert.That(opponent.sprite.name, Is.EqualTo("south-west_0"));
            Assert.That(button.gameObject.activeInHierarchy, Is.False);
            Assert.That(opponent.rectTransform.anchoredPosition.y,
                Is.GreaterThan(player.rectTransform.anchoredPosition.y));
            Assert.That(player.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(opponent.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(canvas.GetComponentInChildren<ActionMenu>().gameObject.activeInHierarchy, Is.True);
            Assert.That(canvas.GetComponentsInChildren<Button>().All(item => !item.interactable), Is.True);
        }

        [UnityTest]
        public IEnumerator DefaultSceneBootstrapsPlayableWarriorGoblin()
        {
            bootstrap.enabled = true;
            yield return null;

            StartPresentation();
            yield return null;

            var menu = canvas.GetComponentInChildren<ActionMenu>();
            Assert.That(HealthText("PlayerHealth"), Is.EqualTo("HP 120 / 120"));
            Assert.That(HealthText("OpponentHealth"), Does.StartWith("HP "));
            Assert.That(HealthText("OpponentHealth"), Does.Not.Contain("—"));
            Assert.That(menu.GetComponentsInChildren<Button>().Any(button => button.interactable), Is.True);
            menu.GetComponentsInChildren<Button>().Single(button => button.name == "Attack").onClick.Invoke();
            yield return WaitForPlayback(canvas.GetComponentInChildren<BattleEventPlayer>());
            Assert.That(HealthText("OpponentHealth"), Does.StartWith("HP "));
            Assert.That(HealthText("OpponentHealth"), Does.Not.Contain("—"));
        }

        [UnityTest]
        public IEnumerator HudDisplaysHealthWithoutAcceptingInvalidValues()
        {
            var hud = canvas.GetComponentInChildren<BattleHud>();
            var health = canvas.GetComponentsInChildren<Text>().Single(text => text.name == "PlayerHealth");
            var fill = canvas.GetComponentsInChildren<Image>().Single(image => image.name == "PlayerHealthFill");
            Assert.That(health.text, Is.EqualTo("HP — / —"));
            hud.ShowHealth(CombatantSide.Player, 60, 120);
            Assert.That(health.text, Is.EqualTo("HP 60 / 120"));
            Assert.That(fill.rectTransform.anchorMax.x, Is.EqualTo(0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => hud.ShowHealth(CombatantSide.Player, 121, 120));
            Assert.Throws<ArgumentOutOfRangeException>(() => hud.ShowHealth(CombatantSide.Player, -1, 120));
            Assert.Throws<ArgumentOutOfRangeException>(() => hud.ShowHealth(CombatantSide.Player, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => hud.ShowHealth(CombatantSide.None, 60, 120));
            Assert.That(health.text, Is.EqualTo("HP 60 / 120"));
            hud.ShowHealth(CombatantSide.Player, 0, 120);
            Assert.That(fill.rectTransform.anchorMax.x, Is.Zero);
            hud.ShowHealth(CombatantSide.Enemy, 45, 45);
            Assert.That(canvas.GetComponentsInChildren<Text>().Single(text => text.name == "OpponentHealth").text,
                Is.EqualTo("HP 45 / 45"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator HudShowsLiteralStatusAndRejectsUnsafeText()
        {
            var hud = canvas.GetComponentInChildren<BattleHud>();
            hud.SetStatus("<b>Vez do Guerreiro</b>");
            var status = canvas.GetComponentsInChildren<Text>(true).Single(text => text.name == "BattleStatus");
            Assert.That(status.supportRichText, Is.False);
            Assert.That(status.text, Is.EqualTo("<b>Vez do Guerreiro</b>"));
            Assert.Throws<ArgumentException>(() => hud.SetStatus("turno\nnovo"));
            Assert.Throws<ArgumentException>(() => hud.SetStatus(new string('x', 81)));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActionsRespectAvailabilityAndBlockRepeatedClicks()
        {
            canvas.GetComponentsInChildren<Button>(true).Single(item => item.name == "StartCombat").onClick.Invoke();
            yield return null;
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var attack = menu.GetComponentsInChildren<Button>().Single(item => item.name == "Attack");
            var guard = menu.GetComponentsInChildren<Button>().Single(item => item.name == "Guard");
            int selectionCount = 0;
            AbilityId selected = AbilityId.None;
            menu.ActionSelected += action => { selectionCount++; selected = action; };
            menu.SetAvailableActions(new[] { AbilityId.Guard }, false);
            Assert.That(attack.interactable, Is.False);
            Assert.That(guard.interactable, Is.True);
            attack.onClick.Invoke();
            Assert.That(selectionCount, Is.Zero);
            guard.onClick.Invoke();
            guard.onClick.Invoke();
            Assert.That(selectionCount, Is.EqualTo(1));
            Assert.That(selected, Is.EqualTo(AbilityId.Guard));
            Assert.That(guard.interactable, Is.False);
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack, AbilityId.Guard }, true);
            attack.onClick.Invoke();
            Assert.That(selectionCount, Is.EqualTo(1));
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack }, false);
            Assert.Throws<ArgumentOutOfRangeException>(() => menu.SetAvailableActions(new[] { (AbilityId)999 }, false));
            Assert.That(attack.interactable, Is.True);
            menu.gameObject.SetActive(false);
            menu.gameObject.SetActive(true);
            attack.onClick.Invoke();
            Assert.That(selectionCount, Is.EqualTo(2));
            Assert.That(selected, Is.EqualTo(AbilityId.BasicAttack));
            menu.SetAvailableActions(Array.Empty<AbilityId>(), false);
            Assert.That(attack.interactable || guard.interactable, Is.False);
        }

        private void StartPresentation()
            => canvas.GetComponentsInChildren<Button>(true).Single(item => item.name == "StartCombat").onClick.Invoke();

        private IEnumerator WaitForPlayback(BattleEventPlayer events)
        {
            float deadline = Time.realtimeSinceStartup + 3;
            while (events.IsPlaying && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(events.IsPlaying, Is.False, "Visual playback did not finish.");
        }

        [UnityTest]
        public IEnumerator VisualEventsPlayInOrderWithoutChangingHealth()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var hud = canvas.GetComponentInChildren<BattleHud>();
            var view = canvas.GetComponentInChildren<BattleView>();
            var attack = menu.GetComponentsInChildren<Button>().Single(item => item.name == "Attack");
            var guard = menu.GetComponentsInChildren<Button>().Single(item => item.name == "Guard");
            var playerRect = view.GetCombatantRect(CombatantSide.Player);
            var opponentRect = view.GetCombatantRect(CombatantSide.Enemy);
            var initialPlayerPosition = playerRect.anchoredPosition;
            var initialOpponentPosition = opponentRect.anchoredPosition;
            hud.ShowHealth(CombatantSide.Player, 80, 120);
            hud.ShowHealth(CombatantSide.Enemy, 30, 45);
            hud.SetStatus("Vez do Guerreiro");
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack, AbilityId.Guard }, false);
            events.Play(new[] { events.Guard(CombatantSide.Player), events.Attack(CombatantSide.Player),
                events.Damage(CombatantSide.Enemy, 12, true, true) });
            Assert.That(events.IsPlaying, Is.True);
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack }, false);
            Assert.That(attack.interactable || guard.interactable, Is.False);
            var statuses = new List<string>();
            float deadline = Time.realtimeSinceStartup + 3;
            while (events.IsPlaying && Time.realtimeSinceStartup < deadline)
            {
                statuses.Add(hud.StatusMessage);
                Assert.That(attack.interactable || guard.interactable, Is.False);
                yield return null;
            }
            Assert.That(events.IsPlaying, Is.False);
            int defenseIndex = statuses.IndexOf("Herói defende");
            int attackIndex = statuses.IndexOf("Ataque do herói");
            int damageIndex = statuses.IndexOf("12 de dano · Crítico · Penetração");
            Assert.That(defenseIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(attackIndex, Is.GreaterThan(defenseIndex));
            Assert.That(damageIndex, Is.GreaterThan(attackIndex));
            Assert.That(attack.interactable, Is.True);
            Assert.That(guard.interactable, Is.False);
            Assert.That(hud.StatusMessage, Is.EqualTo("Vez do Guerreiro"));
            Assert.That(playerRect.anchoredPosition, Is.EqualTo(initialPlayerPosition));
            Assert.That(opponentRect.anchoredPosition, Is.EqualTo(initialOpponentPosition));
            Assert.That(playerRect.GetComponent<Image>().color, Is.EqualTo(Color.white));
            Assert.That(opponentRect.GetComponent<Image>().color, Is.EqualTo(Color.white));
            Assert.That(canvas.GetComponentsInChildren<Text>().Single(text => text.name == "PlayerHealth").text,
                Is.EqualTo("HP 80 / 120"));
            Assert.That(canvas.GetComponentsInChildren<Text>().Single(text => text.name == "OpponentHealth").text,
                Is.EqualTo("HP 30 / 45"));
        }

        [UnityTest]
        public IEnumerator CancelingEventsResetsVisualsAndPreservesInputBlock()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var view = canvas.GetComponentInChildren<BattleView>();
            var opponent = view.GetCombatantRect(CombatantSide.Enemy);
            var initialPosition = opponent.anchoredPosition;
            var feedback = canvas.GetComponentsInChildren<Text>(true).Single(text => text.name == "CombatFeedback");
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack }, false);
            events.Play(new[] { events.Damage(CombatantSide.Enemy, 8) });
            yield return null;
            Assert.That(feedback.gameObject.activeInHierarchy, Is.True);
            menu.SetAvailableActions(new[] { AbilityId.Guard }, true);
            events.Cancel();
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(feedback.gameObject.activeInHierarchy, Is.False);
            Assert.That(opponent.anchoredPosition, Is.EqualTo(initialPosition));
            Assert.That(opponent.GetComponent<Image>().color, Is.EqualTo(Color.white));
            Assert.That(menu.GetComponentsInChildren<Button>().Any(button => button.interactable), Is.False);
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.That(opponent.anchoredPosition, Is.EqualTo(initialPosition));
            Assert.That(opponent.GetComponent<Image>().color, Is.EqualTo(Color.white));
            Assert.That(feedback.gameObject.activeInHierarchy, Is.False);
            menu.SetAvailableActions(new[] { AbilityId.Guard }, false);
            events.Play(new[] { events.Guard(CombatantSide.Player) });
            yield return null;
            events.enabled = false;
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(view.GetCombatantRect(CombatantSide.Player).GetComponent<Image>().color, Is.EqualTo(Color.white));
            Assert.That(menu.GetComponentsInChildren<Button>().Single(button => button.name == "Guard").interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator VisualEventsRejectInvalidInputBeforePlaying()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var view = canvas.GetComponentInChildren<BattleView>();
            Assert.Throws<ArgumentNullException>(() => events.Play(null));
            Assert.Throws<ArgumentException>(() => events.Play(new IEnumerator[] { null }));
            Assert.Throws<ArgumentOutOfRangeException>(() => events.Attack(CombatantSide.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => events.Guard((CombatantSide)999));
            Assert.Throws<ArgumentOutOfRangeException>(() => events.Damage(CombatantSide.Enemy, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.SetCombatantFeedback(CombatantSide.Player,
                new Vector2(float.NaN, 0), Color.white));
            Assert.That(events.IsPlaying, Is.False);
            events.Play(Array.Empty<IEnumerator>());
            Assert.That(events.IsPlaying, Is.False);
            events.Play(new[] { events.Guard(CombatantSide.Player) });
            Assert.Throws<InvalidOperationException>(() => events.Play(new[] { events.Attack(CombatantSide.Player) }));
            events.Cancel();
        }

        [UnityTest]
        public IEnumerator VictoryWaitsForImpactAndKeepsOutcomeAfterCleanup()
        {
            yield return VerifyOutcome(BattlePhase.Victory, CombatantSide.Enemy, "Vitória!");
        }

        [UnityTest]
        public IEnumerator DefeatWaitsForImpactAndKeepsOutcomeAfterCleanup()
        {
            yield return VerifyOutcome(BattlePhase.Defeat, CombatantSide.Player, "Derrota");
        }

        private IEnumerator VerifyOutcome(BattlePhase result, CombatantSide defeated, string message)
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var view = canvas.GetComponentInChildren<BattleView>();
            var hud = canvas.GetComponentInChildren<BattleHud>();
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            hud.ShowHealth(defeated, 0, 100);
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack, AbilityId.Guard }, false);
            events.Play(new[] { events.Damage(defeated, 20), events.End(new BattleEndedEvent(result)),
                events.Attack(CombatantSide.Player) });
            Assert.That(view.HasOutcome, Is.False);
            Assert.That(menu.gameObject.activeSelf, Is.True);
            yield return WaitForPlayback(events);
            Assert.That(view.HasOutcome, Is.True);
            Assert.That(hud.StatusMessage, Is.EqualTo(message));
            Assert.That(menu.gameObject.activeSelf, Is.False);
            Assert.That(menu.GetComponentsInChildren<Button>(true).Any(button => button.interactable), Is.False);
            Assert.That(view.GetCombatantRect(defeated).GetComponent<Image>().color.a, Is.LessThan(1));
            var winner = defeated == CombatantSide.Player ? CombatantSide.Enemy : CombatantSide.Player;
            Assert.That(view.GetCombatantRect(winner).GetComponent<Image>().color, Is.EqualTo(Color.white));
            events.Cancel();
            Assert.That(hud.StatusMessage, Is.EqualTo(message));
            Assert.Throws<InvalidOperationException>(() => events.Play(new[] { events.Guard(winner) }));
            foreach (var size in new[] { new Vector2(360, 640), new Vector2(1280, 720) })
            {
                canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                canvas.GetComponent<RectTransform>().sizeDelta = size;
                view.SendMessage("OnValidate");
                yield return null;
                var status = canvas.GetComponentsInChildren<Text>().Single(text => text.name == "BattleStatus");
                Assert.That(status.text, Is.EqualTo(message));
                Assert.That(status.rectTransform.rect.height, Is.GreaterThan(status.fontSize));
                Assert.That(status.rectTransform.anchoredPosition.y - status.rectTransform.rect.height,
                    Is.GreaterThanOrEqualTo(-size.y));
            }
        }

        [UnityTest]
        public IEnumerator CancelBeforeOutcomeDoesNotEndBattle()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var view = canvas.GetComponentInChildren<BattleView>();
            var hud = canvas.GetComponentInChildren<BattleHud>();
            Assert.Throws<ArgumentNullException>(() => events.End(null));
            events.Play(new[] { events.Damage(CombatantSide.Enemy, 20),
                events.End(new BattleEndedEvent(BattlePhase.Victory)) });
            events.Cancel();
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.That(view.HasOutcome, Is.False);
            Assert.That(hud.StatusMessage, Is.EqualTo("Preparando combate…"));
            Assert.That(canvas.GetComponentInChildren<ActionMenu>().gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator CoreEventsUpdateHealthInOrderAndPreserveCoreState()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var hud = canvas.GetComponentInChildren<BattleHud>();
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var state = PresentationState();
            hud.ShowHealth(CombatantSide.Player, 100, 100);
            hud.ShowHealth(CombatantSide.Enemy, 100, 100);
            var batch = new List<BattleEvent>
            {
                new AbilityUsedEvent(CombatantId.Warrior, AbilityId.BasicAttack),
                new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.Attack, 37, 63, true, true),
                new AbilityUsedEvent(CombatantId.Goblin, AbilityId.BasicAttack),
                new DamageDealtEvent(CombatantId.Goblin, CombatantId.Warrior, DamageKind.Attack, 9, 91, false, false)
            };
            menu.SetAvailableActions(new[] { AbilityId.BasicAttack }, false);
            events.PlayEvents(batch, state);
            batch.Clear();
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo("HP 100 / 100"));
            var statuses = new List<string>();
            float deadline = Time.realtimeSinceStartup + 3;
            while (events.IsPlaying && Time.realtimeSinceStartup < deadline)
            {
                statuses.Add(hud.StatusMessage);
                Assert.That(menu.GetComponentsInChildren<Button>().Any(button => button.interactable), Is.False);
                if (hud.StatusMessage == "37 de dano · Crítico · Penetração")
                {
                    Assert.That(HealthText("OpponentHealth"), Is.EqualTo("HP 63 / 100"));
                    Assert.That(HealthText("PlayerHealth"), Is.EqualTo("HP 100 / 100"));
                }
                yield return null;
            }
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(statuses.IndexOf("Ataque do inimigo"), Is.GreaterThan(statuses.IndexOf("37 de dano · Crítico · Penetração")));
            Assert.That(statuses, Does.Contain("37 de dano · Crítico · Penetração"));
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo("HP 63 / 100"));
            Assert.That(HealthText("PlayerHealth"), Is.EqualTo("HP 91 / 100"));
            Assert.That(state.Hero.CurrentHealth, Is.EqualTo(100));
            Assert.That(state.Opponent.CurrentHealth, Is.EqualTo(100));
            Assert.That(state.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
        }

        [UnityTest]
        public IEnumerator CoreReflectionIsDistinctAndOutcomeFollowsDamage()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var hud = canvas.GetComponentInChildren<BattleHud>();
            events.PlayEvents(new BattleEvent[]
            {
                new AbilityUsedEvent(CombatantId.Warrior, AbilityId.Guard),
                new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.Reflection, 4, 0, false, false),
                new BattleEndedEvent(BattlePhase.Victory)
            }, PresentationState());
            var statuses = new List<string>();
            float deadline = Time.realtimeSinceStartup + 3;
            while (events.IsPlaying && Time.realtimeSinceStartup < deadline)
            {
                statuses.Add(hud.StatusMessage);
                yield return null;
            }
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(statuses, Does.Contain("Herói defende"));
            Assert.That(statuses.IndexOf("Reflexão · 4 de dano"), Is.GreaterThan(statuses.IndexOf("Herói defende")));
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo("HP 0 / 100"));
            Assert.That(hud.StatusMessage, Is.EqualTo("Vitória!"));
        }

        [UnityTest]
        public IEnumerator InvalidCoreBatchDoesNotPartiallyChangePresentation()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var state = PresentationState();
            Assert.Throws<ArgumentNullException>(() => events.PlayEvents(null, state));
            Assert.Throws<ArgumentNullException>(() => events.PlayEvents(Array.Empty<BattleEvent>(), null));
            foreach (var invalid in new BattleEvent[]
            {
                null,
                new AbilityUsedEvent(CombatantId.Skeleton, AbilityId.BasicAttack),
                new DamageDealtEvent(CombatantId.Skeleton, CombatantId.Goblin, DamageKind.Attack, 1, 99, false, false),
                new DamageDealtEvent(CombatantId.Warrior, CombatantId.Skeleton, DamageKind.Attack, 1, 99, false, false),
                new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.Attack, 1, 101, false, false)
            })
                Assert.Catch<ArgumentException>(() => events.PlayEvents(new BattleEvent[]
                    { new AbilityUsedEvent(CombatantId.Warrior, AbilityId.BasicAttack), invalid }, state));
            Assert.Throws<NotSupportedException>(() => events.PlayEvents(new BattleEvent[]
                { new AbilityUsedEvent(CombatantId.Warrior, AbilityId.RevertTurn) }, state));
            Assert.Throws<ArgumentException>(() => events.PlayEvents(new BattleEvent[]
                { new BattleEndedEvent(BattlePhase.Victory), new AbilityUsedEvent(CombatantId.Warrior, AbilityId.Guard) }, state));
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(canvas.GetComponentInChildren<BattleHud>().StatusMessage, Is.EqualTo("Preparando combate…"));
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo("HP — / —"));
        }

        [UnityTest]
        public IEnumerator CancelCoreBatchDiscardsPendingHealthUpdates()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            canvas.GetComponentInChildren<BattleHud>().ShowHealth(CombatantSide.Enemy, 100, 100);
            events.PlayEvents(new BattleEvent[]
            {
                new AbilityUsedEvent(CombatantId.Warrior, AbilityId.BasicAttack),
                new DamageDealtEvent(CombatantId.Warrior, CombatantId.Goblin, DamageKind.Attack, 20, 80, false, false)
            }, PresentationState());
            events.Cancel();
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo("HP 100 / 100"));
        }

        [UnityTest]
        public IEnumerator MageUsesTheSharedSceneAgainstEveryConfiguredEnemyAndRevertsImmediately()
        {
            foreach (var opponent in new[]
            {
                CombatantId.Goblin,
                CombatantId.Skeleton,
                CombatantId.Werewolf,
                CombatantId.Vampire,
                CombatantId.Necromancer,
                CombatantId.DemonKing
            })
            {
                if (opponent != CombatantId.Goblin)
                {
                    yield return TearDown();
                    yield return SetUp();
                }
                var view = canvas.GetComponentInChildren<BattleView>();
                var battle = InitializeMageBattle(view, opponent);
                StartPresentation();
                yield return null;

                var menu = canvas.GetComponentInChildren<ActionMenu>();
                var buttons = menu.GetComponentsInChildren<Button>();
                Assert.That(view.SelectedPlayer.Id, Is.EqualTo(CombatantId.Mage));
                Assert.That(view.GetCombatantRect(CombatantSide.Player).GetComponent<Image>().sprite.name,
                    Is.EqualTo("north-east_0"));
                Assert.That(buttons.Length, Is.EqualTo(3));
                Assert.That(buttons.Single(button => button.name == "Attack").GetComponentInChildren<Text>().text,
                    Is.EqualTo("Ataque mágico"));
                Assert.That(buttons.Single(button => button.name == "RevertTurn").interactable, Is.False);

                var initialPlayerHealth = battle.State.Hero.CurrentHealth;
                var initialOpponentHealth = battle.State.Opponent.CurrentHealth;
                buttons.Single(button => button.name == "Attack").onClick.Invoke();
                yield return WaitForPlayback(canvas.GetComponentInChildren<BattleEventPlayer>());
                yield return null;
                Assert.That(battle.State.Opponent.CurrentHealth, Is.LessThan(initialOpponentHealth));
                buttons.Single(button => button.name == "RevertTurn").onClick.Invoke();
                Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(initialPlayerHealth));
                Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(initialOpponentHealth));
                Assert.That(battle.State.RevertCharges, Is.Zero);
                Assert.That(buttons.Single(button => button.name == "Attack").interactable, Is.True);
                Assert.That(buttons.Single(button => button.name == "RevertTurn").interactable, Is.False);
            }
        }

        [UnityTest]
        public IEnumerator MageRevertBattleRestoresTheEncounterAndReopensActions()
        {
            var view = canvas.GetComponentInChildren<BattleView>();
            var battle = InitializeMageBattle(view, CombatantId.Goblin);
            var initialPlayerHealth = battle.State.Hero.CurrentHealth;
            var initialOpponentHealth = battle.State.Opponent.CurrentHealth;
            var initialRound = battle.State.Round;
            StartPresentation();
            yield return null;

            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var buttons = menu.GetComponentsInChildren<Button>();
            buttons.Single(button => button.name == "Attack").onClick.Invoke();
            yield return WaitForPlayback(canvas.GetComponentInChildren<BattleEventPlayer>());
            yield return null;
            buttons.Single(button => button.name == "RevertBattle").onClick.Invoke();

            Assert.That(battle.State.Hero.CurrentHealth, Is.EqualTo(initialPlayerHealth));
            Assert.That(battle.State.Opponent.CurrentHealth, Is.EqualTo(initialOpponentHealth));
            Assert.That(battle.State.Round, Is.EqualTo(initialRound));
            Assert.That(battle.State.RevertCharges, Is.Zero);
            Assert.That(HealthText("PlayerHealth"), Is.EqualTo($"HP {initialPlayerHealth} / {initialPlayerHealth}"));
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo($"HP {initialOpponentHealth} / {initialOpponentHealth}"));
            Assert.That(buttons.Single(button => button.name == "Attack").interactable, Is.True);
            Assert.That(buttons.Single(button => button.name == "RevertBattle").interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator SharedVisualRegistrationCanShowGoblinAgainstNecromancer()
        {
            var view = canvas.GetComponentInChildren<BattleView>();
            view.SelectPlayer(CombatantId.Goblin);
            view.SelectEncounter(CombatantId.Necromancer);
            var goblin = ContentMapper.BuildCombatant(view.SelectedPlayer);
            var controlledGoblin = new CombatantConfiguration(goblin.Id, CombatantSide.Player, goblin.DisplayName,
                goblin.BaseStats, goblin.Abilities.ToArray());
            var necromancer = ContentMapper.BuildEncounter(view.SelectedEncounter).Opponent;
            var battle = new Battle(new BattleState(new CombatantState(controlledGoblin, controlledGoblin.BaseStats),
                new CombatantState(necromancer, necromancer.BaseStats)), new SeededRandomSource(17));
            canvas.GetComponentInChildren<BattlePresenter>().Initialize(battle);
            StartPresentation();
            yield return null;

            Assert.That(view.GetCombatantRect(CombatantSide.Player).GetComponent<Image>().sprite.name,
                Is.EqualTo("north-east_0"));
            Assert.That(canvas.GetComponentsInChildren<Image>().Any(image => image.sprite != null &&
                image.sprite.name == "necromancer-crypt"), Is.True);
            Assert.That(canvas.GetComponentsInChildren<Text>().Any(text => text.text == "Goblin"), Is.True);
            Assert.That(canvas.GetComponentInChildren<ActionMenu>().GetComponentsInChildren<Button>().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FuryEventsUseTheSharedEnemyPresentation()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            events.PlayEvents(new BattleEvent[]
            {
                new AbilityUsedEvent(CombatantId.Goblin, AbilityId.Fury),
                new DamageDealtEvent(CombatantId.Goblin, CombatantId.Warrior, DamageKind.Fury, 20, 80, false, false)
            }, PresentationState());
            yield return null;
            Assert.That(canvas.GetComponentInChildren<BattleHud>().StatusMessage, Does.StartWith("Fúria"));
            yield return WaitForPlayback(events);
            Assert.That(HealthText("PlayerHealth"), Is.EqualTo("HP 80 / 100"));
        }

        private string HealthText(string name)
            => canvas.GetComponentsInChildren<Text>().Single(text => text.name == name).text;

        private Battle InitializeMageBattle(BattleView view, CombatantId opponent)
        {
            view.SelectPlayer(CombatantId.Mage);
            view.SelectEncounter(opponent);
            var mage = ContentMapper.BuildCombatant(view.SelectedPlayer);
            var enemy = ContentMapper.BuildEncounter(view.SelectedEncounter).Opponent;
            var battle = new Battle(new BattleState(new CombatantState(mage, mage.BaseStats),
                new CombatantState(enemy, enemy.BaseStats)), new SeededRandomSource(17));
            canvas.GetComponentInChildren<BattlePresenter>().Initialize(battle);
            return battle;
        }

        private static BattleState PresentationState(int heroHealth = 100, int enemyHealth = 100)
        {
            CombatantState Combatant(CombatantId id, CombatantSide side, int maximumHealth)
            {
                var stats = new CombatantStats(maximumHealth, 15, 3);
                var attack = new AbilityConfiguration(AbilityId.BasicAttack, "Atacar", AbilityTarget.Opponent, true);
                var guard = new AbilityConfiguration(AbilityId.Guard, "Defender", AbilityTarget.Self, true);
                return new CombatantState(new CombatantConfiguration(id, side, id.ToString(), stats,
                    side == CombatantSide.Player ? new[] { attack, guard } : new[] { attack }), stats);
            }
            return new BattleState(Combatant(CombatantId.Warrior, CombatantSide.Player, heroHealth),
                Combatant(CombatantId.Goblin, CombatantSide.Enemy, enemyHealth));
        }

        [UnityTest]
        public IEnumerator PresenterSubmitsAttackAndGuardOncePerRound()
        {
            var state = PresentationState();
            var battle = new Battle(state, new SeededRandomSource(17));
            var presenter = canvas.GetComponentInChildren<BattlePresenter>();
            presenter.Initialize(battle);
            Assert.Throws<InvalidOperationException>(() => presenter.Initialize(battle));
            StartPresentation();
            yield return null;
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var attack = menu.GetComponentsInChildren<Button>().Single(button => button.name == "Attack");
            var guard = menu.GetComponentsInChildren<Button>().Single(button => button.name == "Guard");
            Assert.That(attack.interactable && guard.interactable, Is.True);
            attack.onClick.Invoke();
            attack.onClick.Invoke();
            Assert.That(state.Round, Is.EqualTo(2));
            Assert.That(attack.interactable || guard.interactable, Is.False);
            yield return WaitForPlayback(events);
            yield return null;
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo($"HP {state.Opponent.CurrentHealth} / 100"));
            Assert.That(attack.interactable && guard.interactable, Is.True);
            int previousHealth = state.Opponent.CurrentHealth;
            guard.onClick.Invoke();
            guard.onClick.Invoke();
            Assert.That(state.Round, Is.EqualTo(3));
            Assert.That(state.Opponent.CurrentHealth, Is.EqualTo(previousHealth - Battle.GuardDamage));
            yield return WaitForPlayback(events);
            yield return null;
            Assert.That(HealthText("PlayerHealth"), Is.EqualTo($"HP {state.Hero.CurrentHealth} / 100"));
            Assert.That(attack.interactable && guard.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator PresenterShowsVictoryFromRealCombat()
        {
            yield return VerifyPresenterOutcome(100, 1, BattlePhase.Victory, "Vitória!");
        }

        [UnityTest]
        public IEnumerator PresenterShowsDefeatFromRealCombat()
        {
            yield return VerifyPresenterOutcome(1, 100, BattlePhase.Defeat, "Derrota");
        }

        private IEnumerator VerifyPresenterOutcome(int heroHealth, int enemyHealth, BattlePhase expected, string message)
        {
            var state = PresentationState(heroHealth, enemyHealth);
            canvas.GetComponentInChildren<BattlePresenter>().Initialize(new Battle(state, new SeededRandomSource(17)));
            StartPresentation();
            yield return null;
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            menu.GetComponentsInChildren<Button>().Single(button => button.name == "Attack").onClick.Invoke();
            Assert.That(state.Phase, Is.EqualTo(expected));
            yield return WaitForPlayback(canvas.GetComponentInChildren<BattleEventPlayer>());
            yield return null;
            Assert.That(canvas.GetComponentInChildren<BattleHud>().StatusMessage, Is.EqualTo(message));
            Assert.That(menu.gameObject.activeSelf, Is.False);
            Assert.That(HealthText("PlayerHealth"), Is.EqualTo($"HP {state.Hero.CurrentHealth} / {heroHealth}"));
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo($"HP {state.Opponent.CurrentHealth} / {enemyHealth}"));
        }

        [UnityTest]
        public IEnumerator PresenterResumesAuthoritativeStateAfterInterruptedPlayback()
        {
            StartPresentation();
            yield return null;
            var presenter = canvas.GetComponentInChildren<BattlePresenter>();
            Assert.Throws<ArgumentNullException>(() => presenter.Initialize(null));
            var state = PresentationState();
            presenter.Initialize(new Battle(state, new SeededRandomSource(17)));
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var menu = canvas.GetComponentInChildren<ActionMenu>();
            var attack = menu.GetComponentsInChildren<Button>().Single(button => button.name == "Attack");
            attack.onClick.Invoke();
            presenter.enabled = false;
            Assert.That(events.IsPlaying, Is.False);
            Assert.That(attack.interactable, Is.False);
            presenter.enabled = true;
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(HealthText("OpponentHealth"), Is.EqualTo($"HP {state.Opponent.CurrentHealth} / 100"));
            Assert.That(attack.interactable, Is.True);
            attack.onClick.Invoke();
            Assert.That(state.Round, Is.EqualTo(3));
            yield return WaitForPlayback(events);
        }

        [UnityTest]
        public IEnumerator ResizingDuringImpactKeepsFeedbackInsideArenaAndRestoresPositions()
        {
            StartPresentation();
            yield return null;
            var events = canvas.GetComponentInChildren<BattleEventPlayer>();
            var view = canvas.GetComponentInChildren<BattleView>();
            events.Play(new[] { events.Damage(CombatantSide.Enemy, 10) });
            yield return null;
            var unityCanvas = canvas.GetComponent<Canvas>();
            unityCanvas.renderMode = RenderMode.WorldSpace;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1280, 720);
            view.SendMessage("OnValidate");
            yield return null;
            yield return null;
            var feedback = canvas.GetComponentsInChildren<Text>(true).Single(text => text.name == "CombatFeedback");
            var opponent = view.GetCombatantRect(CombatantSide.Enemy);
            var arena = (RectTransform)opponent.parent;
            Assert.That(feedback.rectTransform.anchoredPosition.x, Is.InRange(0, arena.rect.width));
            Assert.That(feedback.rectTransform.anchoredPosition.y, Is.InRange(0, arena.rect.height));
            yield return WaitForPlayback(events);
            Assert.That(opponent.anchoredPosition.x, Is.EqualTo(Mathf.Round(arena.rect.width * 0.68f)));
            Assert.That(opponent.anchoredPosition.y, Is.EqualTo(Mathf.Round(arena.rect.height * 0.34f)));
            Assert.That(opponent.GetComponent<Image>().color, Is.EqualTo(Color.white));
        }
    }
}
#endif
