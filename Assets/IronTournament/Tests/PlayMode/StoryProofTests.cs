#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using IronTournament.Bootstrap;
using IronTournament.Core;
using IronTournament.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace IronTournament.Presentation.Tests
{
    public sealed class StoryProofTests
    {
        [Serializable] private sealed class SavedEnvelope { public int revision; public string payload; public string checksum; }
        private const string Prefix="iron.story.tests";
        private StoryProofController controller;
        private StoryView view;

        private IEnumerator Load()
        {
            PlayerPrefs.DeleteKey(Prefix+".a"); PlayerPrefs.DeleteKey(Prefix+".b");
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> configure = (loaded, mode) =>
            {
                if (loaded.path != "Assets/Scenes/StoryProof.unity") return;
                controller = loaded.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<StoryProofController>()).Single();
                controller.SavePrefix = Prefix;
            };
            SceneManager.sceneLoaded += configure;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/StoryProof.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            SceneManager.sceneLoaded -= configure;
            Assert.That(controller, Is.Not.Null);
            view=UnityEngine.Object.FindAnyObjectByType<StoryView>();
        }
        private void Click(string caption)
        {
            view.CompleteText();
            var button=view.GetComponentsInChildren<Button>().Single(x=>x.name==caption);
            Assert.That(button.interactable,Is.True); button.onClick.Invoke();
        }
        private IEnumerator AdvanceTo(StoryPage page)
        {
            for(var i=0;i<100 && controller.Run.page!=page;i++)
            {
                view.CompleteText();
                var next=view.GetComponentsInChildren<Button>().FirstOrDefault(x=>x.name=="Continuar" || x.name=="Entrar na vila" || x.name=="Avançar");
                Assert.That(next,Is.Not.Null,"No next button at "+controller.Run.page);
                next.onClick.Invoke(); yield return null;
            }
            Assert.That(controller.Run.page,Is.EqualTo(page));
        }

        [UnityTest]
        public IEnumerator CrystalRevealsLettersAndTheFixedDangerousGrave()
        {
            yield return Load();
            controller.Begin(CombatantId.Warrior, 12);
            var run = controller.Run; run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.page = StoryPage.Graveyard; run.skeletonTrigger = 3;
            var ordinary = Enumerable.Range(0, 6).Where(i => run.graves[i] >= (int)GraveContent.Rosary && run.graves[i] != (int)GraveContent.Empty).Take(2).ToArray();
            run.gravesVisited = (1 << ordinary[0]) | (1 << ordinary[1]);
            controller.Render();
            var neutral = Enumerable.Range(0, 6).Select(i =>
                view.GetComponentsInChildren<Button>().Single(x => x.name == "Túmulo " + (i + 1)).image.color).ToArray();
            run.crystal = true;
            run.skeletonGraveNumber = Array.FindIndex(run.graves, x => x == (int)GraveContent.Empty) + 1;
            controller.Render();
            for (var i = 0; i < 6; i++)
            {
                var button = view.GetComponentsInChildren<Button>().Single(x => x.name == "Túmulo " + (i + 1));
                var visited = (run.gravesVisited & (1 << i)) != 0;
                var letter = run.graves[i] == (int)GraveContent.LucyLetter || run.graves[i] == (int)GraveContent.SamuelLetter;
                var danger = !visited && i + 1 == run.skeletonGraveNumber;
                Assert.That(button.GetComponentsInChildren<Text>().Length, Is.EqualTo(1));
                if (danger) Assert.That(button.image.color.r, Is.GreaterThan(button.image.color.b));
                else if (!visited && letter) Assert.That(button.image.color.b, Is.GreaterThan(button.image.color.r));
                else Assert.That(button.image.color, Is.EqualTo(neutral[i]));
            }
            run.skeletonAppeared = true; run.activeGrave = run.skeletonGraveNumber - 1; controller.Render();
            var dangerButton = view.GetComponentsInChildren<Button>().Single(x => x.name == "Túmulo " + run.skeletonGraveNumber);
            Assert.That(dangerButton.image.color, Is.EqualTo(neutral[run.skeletonGraveNumber - 1]));
            var letterSlot = Array.FindIndex(run.graves, x => x == (int)GraveContent.LucyLetter);
            var letterButton = view.GetComponentsInChildren<Button>().Single(x => x.name == "Túmulo " + (letterSlot + 1));
            Assert.That(letterButton.image.color.b, Is.GreaterThan(letterButton.image.color.r));
            run.gravesVisited |= 1 << letterSlot; controller.Render();
            letterButton = view.GetComponentsInChildren<Button>().Single(x => x.name == "Túmulo " + (letterSlot + 1));
            Assert.That(letterButton.image.color, Is.EqualTo(neutral[ordinary[0]]));
        }

        [UnityTest]
        public IEnumerator LayoutOrientationFollowsTheGameCanvas()
        {
            yield return Load();
            var rect = view.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 360);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 640);
            Assert.That(view.Portrait, Is.True);
            view.Draw(null, "Teste", "");
            Assert.That(view.GetComponentsInChildren<RectTransform>().Any(x => x.name == "Journal"), Is.False);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 720);
            Assert.That(view.Portrait, Is.False);
            view.Draw(null, "Teste", "");
            Assert.That(view.GetComponentsInChildren<RectTransform>().Any(x => x.name == "Journal"), Is.True);
        }

        [UnityTest]
        public IEnumerator WarriorAttributesShowTheRequestedDescription()
        {
            yield return Load();
            Click("Escolher protagonista");
            view.GetComponentsInChildren<Button>().First(x => x.name == "Ver atributos").onClick.Invoke();
            var text = view.GetComponentsInChildren<Text>().Select(x => x.text).ToArray();
            Assert.That(text, Does.Contain("HP 120 · ATQ 30 · DEF 23\nUm combatente experiênte, calejado pelos longos anos de batalhas. Resistente, porém suceptível à defesas fortes."));
        }

        [UnityTest]
        public IEnumerator OpeningNarrationIsCenteredOnBlack()
        {
            yield return Load();
            var openingText = view.GetComponentsInChildren<Text>().Select(x => x.text).ToArray();
            Assert.That(openingText, Does.Contain("Até que o último caia"));
            Assert.That(openingText, Does.Not.Contain("CAPÍTULO I\nA VILA"));
            Assert.That(openingText, Does.Not.Contain("Uma jornada. Duas classes. Suas escolhas."));
            Assert.That(openingText, Does.Not.Contain("A JORNADA"));
            Assert.That(openingText, Does.Not.Contain("Escolha seu protagonista e descubra o que restou da vila."));
            Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("opening_screen"));
            Assert.That(view.GetComponent<AudioSource>(), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<AudioListener>(), Is.Not.Null);
            var button = view.GetComponentsInChildren<Button>().Single(x => x.name == "Escolher protagonista");
            Assert.That(button.image.sprite.texture, Is.EqualTo(Resources.Load<Texture2D>("StoryUi/Button")));
            foreach (var sound in new[] { "Confirm", "Slash", "Magic", "Hit", "Block", "Defeat", "Heal" })
                Assert.That(Resources.Load<AudioClip>("StoryAudio/" + sound), Is.Not.Null, sound);
            controller.Begin(CombatantId.Warrior, 12);
            var narration = view.SceneArea.Find("Narration");
            Assert.That(narration, Is.Not.Null);
            Assert.That(view.SceneArea.GetComponent<Image>().color, Is.EqualTo(Color.black));
            Assert.That(narration.GetComponentInChildren<Text>().alignment, Is.EqualTo(TextAnchor.MiddleCenter));
            Assert.That(((RectTransform)narration).anchorMin.y, Is.GreaterThan(.25f));
            yield return AdvanceTo(StoryPage.Arrival);
            Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("village_distant_view"));
            yield return AdvanceTo(StoryPage.Village);
            Assert.That(view.SceneArea.GetComponent<Image>().color, Is.EqualTo(Color.white));
        }

        [UnityTest]
        public IEnumerator StoryPresentsNarrationDialogueHealthAndChoiceHierarchy()
        {
            yield return Load(); controller.Begin(CombatantId.Warrior,12); yield return AdvanceTo(StoryPage.Village);
            Click("Casa 1");
            Assert.That(view.SceneArea.Find("Narration"), Is.Not.Null);
            var choices = view.GetComponentsInChildren<Button>();
            Assert.That(choices.Single(x => x.name == "Inventário").image.color,
                Is.Not.EqualTo(choices.Single(x => x.name == "Voltar à vila").image.color));
            Click("Voltar à vila"); Click("Investigar agora"); yield return AdvanceTo(StoryPage.Rescue);
            Assert.That(view.SceneArea.Find("Narration"), Is.Not.Null);
            Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("swamp_clearing"));
            var actors = view.SceneArea.GetComponentsInChildren<Image>().Where(x => x.name == "Actor").ToArray();
            var goblin = (RectTransform)actors[1].transform;
            var child = (RectTransform)actors[2].transform;
            var goblinBody = (goblin.anchorMax.y - goblin.anchorMin.y) * 30f / actors[1].sprite.rect.height;
            Assert.That((child.anchorMax.y - child.anchorMin.y) / goblinBody, Is.EqualTo(1f).Within(.001f));
            Click("Continuar"); yield return null;
            var childBubble = (RectTransform)view.SceneArea.Find("Dialogue");
            Assert.That(childBubble, Is.Not.Null);
            Assert.That(childBubble.anchorMax.x, Is.GreaterThan(.75f));
            Assert.That(childBubble.anchorMax.x-childBubble.anchorMin.x, Is.LessThan(.6f));
            Click("Continuar"); yield return null;
            var heroBubble = (RectTransform)view.SceneArea.Find("Dialogue");
            Assert.That(heroBubble, Is.Not.Null);
            Assert.That(heroBubble.anchorMin.x, Is.LessThan(.1f));
            yield return AdvanceTo(StoryPage.Battle);
            Assert.That(view.SceneArea.Find("Health_Guerreiro"), Is.Not.Null);
            Assert.That(view.SceneArea.Find("Health_Goblin"), Is.Not.Null);
            var goblinBar = (RectTransform)view.SceneArea.Find("Health_Goblin/HealthTrack/HealthFill");
            Assert.That(goblinBar, Is.Not.Null);
            Assert.That(goblinBar.anchorMax.x, Is.EqualTo(1f));
            controller.SubmitBattle(AbilityId.BasicAttack); yield return new WaitForSecondsRealtime(2f);
            Assert.That(controller.Run.page, Is.EqualTo(StoryPage.Battle));
            Assert.That(view.SceneArea.Find("Health_Goblin").GetComponentInChildren<Text>().text,
                Does.Contain(controller.ActiveBattle.State.Opponent.CurrentHealth.ToString()));
            goblinBar = (RectTransform)view.SceneArea.Find("Health_Goblin/HealthTrack/HealthFill");
            var opponent = controller.ActiveBattle.State.Opponent;
            Assert.That(goblinBar.anchorMax.x, Is.EqualTo((float)opponent.CurrentHealth / opponent.Stats.MaximumHealth).Within(.001f));
            var label = view.SceneArea.Find("Health_Goblin").GetComponentInChildren<Text>();
            view.SetHealth(label, 0, 100);
            Assert.That(goblinBar.anchorMax.x, Is.EqualTo(0f));
            view.SetHealth(label, 50, 100);
            Assert.That(goblinBar.anchorMax.x, Is.EqualTo(.5f));
            Assert.That(label.text, Is.EqualTo("HP 50/100"));
        }

        [UnityTest]
        public IEnumerator SpecialFindsDependOnClassAndVisitedHousesChangeColor()
        {
            foreach (var hero in new[] { CombatantId.Warrior, CombatantId.Mage })
            foreach (var house in new[] { 0, 1 })
            {
                yield return Load(); controller.Begin(hero, 42); yield return AdvanceTo(StoryPage.Village);
                var slot = Array.IndexOf(controller.Run.houses, house);
                var houseName = "Casa " + (slot + 1);
                var before = view.GetComponentsInChildren<Button>().Single(x => x.name == houseName).image.color;
                Click(houseName);
                var points = view.SceneArea.GetComponentsInChildren<Button>().Select(x => (RectTransform)x.transform).ToArray();
                foreach (var a in points) foreach (var b in points)
                {
                    if (a == b) continue;
                    Assert.That(a.anchorMin.x < b.anchorMax.x && a.anchorMax.x > b.anchorMin.x &&
                                a.anchorMin.y < b.anchorMax.y && a.anchorMax.y > b.anchorMin.y, Is.False);
                }
                Click("Buraco no chão"); view.CompleteText();
                var first = house == 0 ? "Você encontra uma espada antiga." : "Você encontra um manto antigo.";
                Assert.That(view.SceneArea.GetComponentsInChildren<Text>().Any(t => t.text == first), Is.True);
                Click("Continuar"); view.CompleteText();
                var compatible = (hero == CombatantId.Warrior) == (house == 0);
                var second = house == 0 ? (compatible ? "Ela é pesada, mas você consegue a levantar." : "Ela é pesada demais para você levantar.")
                    : (compatible ? "Ele está estranhamente quente, mas você o consegue pegar." : "Ele queima sua mão e você não consegue pegar ele.");
                Assert.That(view.SceneArea.GetComponentsInChildren<Text>().Any(t => t.text == second), Is.True);
                Assert.That(controller.Run.equipment, Is.EqualTo(compatible));
                Click("Continuar");
                if (compatible)
                {
                    view.CompleteText();
                    Assert.That(view.SceneArea.GetComponentsInChildren<Text>().Any(t => t.text == "CARTA"), Is.True);
                    Click("Continuar");
                }
                Click("Voltar à vila"); Click("Continuar explorando");
                var after = view.GetComponentsInChildren<Button>().Single(x => x.name == houseName).image.color;
                Assert.That(after, Is.Not.EqualTo(before));
                Click(houseName); Assert.That(controller.Run.ExploredCount, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator FirstEncounterCanBeCompletedWithBothClasses()
        {
            foreach(var hero in new[] {CombatantId.Warrior,CombatantId.Mage})
            {
                yield return Load(); controller.Begin(hero,417);
                yield return AdvanceTo(StoryPage.Village);
                var run=controller.Run;
                var foodSlot=Array.IndexOf(run.houses,3);
                Click("Casa "+(foodSlot+1)); Click("Caldeirão");
                while(view.GetComponentsInChildren<Button>().Any(x=>x.name=="Continuar")) { Click("Continuar"); yield return null; }
                Click("Voltar à vila"); Assert.That(run.pendingCry,Is.True);
                Click("Investigar agora"); yield return AdvanceTo(StoryPage.Battle);
                Assert.That(controller.ActiveBattle.State.Hero.Id,Is.EqualTo(hero));
                for(var turn=0;turn<12 && run.page==StoryPage.Battle;turn++)
                { controller.SubmitBattle(AbilityId.BasicAttack); yield return new WaitForSecondsRealtime(1.8f); }
                Assert.That(run.page,Is.EqualTo(StoryPage.AfterBattle));
                yield return AdvanceTo(StoryPage.Food);
                Click("Entregar"); yield return AdvanceTo(StoryPage.Reward);
                Assert.That(run.foodGiven,Is.True); Assert.That(run.food,Is.False);
                var rewards=view.GetComponentsInChildren<Button>(); Assert.That(rewards.Length,Is.EqualTo(2));
                foreach (var reward in rewards)
                    Assert.That(reward.GetComponentsInChildren<Text>().Length, Is.EqualTo(2), "Cada item deve exibir sua descrição.");
                rewards[0].onClick.Invoke();
                yield return AdvanceTo(StoryPage.Finished);
                Assert.That(run.crystal,Is.True); Assert.That(run.reward,Is.Not.EqualTo(ItemId.None));
                var store=new StorySaveStore(Prefix); Assert.That(store.TryLoad(out var restored),Is.True);
                Assert.That(restored.page,Is.EqualTo(StoryPage.Finished)); Assert.That(restored.foodGiven,Is.True);
                Click("Continuar");
                Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("forest_cold_closed"));
                for (var i = 0; i < 3; i++) { Click("Continuar"); yield return null; }
                Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("forest_cold_clearing_distant"));
                yield return AdvanceTo(StoryPage.Graveyard);
                Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("forest_cold_clearing_graveyard"));
                while (!run.skeletonAppeared)
                {
                    var slot = Enumerable.Range(0, 6).First(i => (run.gravesVisited & (1 << i)) == 0);
                    Click("Túmulo " + (slot + 1));
                    if (run.page == StoryPage.Grave)
                    {
                        Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("coffin_opened_straight_view"));
                        Click("Voltar aos túmulos");
                    }
                    yield return null;
                }
                Assert.That(run.activeGrave + 1, Is.EqualTo(run.skeletonGraveNumber));
                Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("forest_cold_skeleton_hand"));
                Assert.That(store.TryLoad(out restored), Is.True); Assert.That(restored.graves, Is.EqualTo(run.graves));
                Click("Enfrentar"); Assert.That(controller.ActiveBattle.State.Opponent.Id, Is.EqualTo(CombatantId.Skeleton));
                Assert.That(controller.ActiveBattle.State.Hero.CurrentHealth, Is.EqualTo(run.victoryHealth));
                for (var turn = 0; turn < 12 && run.page == StoryPage.SkeletonBattle; turn++)
                { controller.SubmitBattle(AbilityId.BasicAttack); yield return new WaitForSecondsRealtime(1.8f); }
                Assert.That(run.page, Is.EqualTo(StoryPage.SkeletonLetter));
                yield return AdvanceTo(StoryPage.SkeletonReward);
                view.GetComponentsInChildren<Button>()[0].onClick.Invoke();
                Assert.That(run.page, Is.EqualTo(StoryPage.Castle)); Click("Voltar aos túmulos");
                for (var slot = 0; slot < 6; slot++)
                {
                    if ((run.gravesVisited & (1 << slot)) != 0) continue;
                    Click("Túmulo " + (slot + 1)); Assert.That(run.page, Is.EqualTo(StoryPage.Grave));
                    Click("Voltar aos túmulos");
                }
                Assert.That(run.GravesExplored, Is.EqualTo(6)); Click("Seguir viagem");
                Assert.That(store.TryLoad(out restored), Is.True); Assert.That(restored.page, Is.EqualTo(StoryPage.SkeletonFinished));
            }
        }

        [UnityTest]
        public IEnumerator HouseAndBedroomHaveDistinctPaginatedObservations()
        {
            yield return Load(); controller.Begin(CombatantId.Warrior,12); yield return AdvanceTo(StoryPage.Village);
            var slot = Array.IndexOf(controller.Run.houses, 2);
            var houseButton = view.GetComponentsInChildren<Button>().Single(x => x.name == "Casa " + (slot + 1));
            Assert.That(houseButton.GetComponentInChildren<Text>().text, Does.Contain("Entrar"));
            Click("Casa " + (slot + 1)); Click("Observar"); view.CompleteText();
            Assert.That(view.SceneArea.GetComponentsInChildren<Text>().Any(t => t.text.Contains("móveis quebrados")), Is.True);
            Click("Continuar"); view.CompleteText();
            Assert.That(view.SceneArea.GetComponentsInChildren<Text>().Any(t => t.text.Contains("rastro de sangue")), Is.True);
            Click("Continuar"); view.CompleteText(); Click("Continuar");
            Click("Quarto"); Click("Observar"); view.CompleteText();
            Assert.That(view.SceneArea.GetComponentsInChildren<Text>().Any(t => t.text.Contains("preso à parede")), Is.True);
        }

        [UnityTest]
        public IEnumerator ThirdCryAndIgnoredCryReachDifferentPostBattleScenes()
        {
            foreach(var ignore in new[] {false,true})
            {
                yield return Load(); controller.Begin(CombatantId.Warrior,12);
                yield return AdvanceTo(StoryPage.Village);
                for(var i=0;i<3;i++)
                {
                    Click("Casa "+(i+1)); Click("Voltar à vila");
                    Assert.That(controller.Run.ExploredCount,Is.EqualTo(i+1));
                    if(i<2 || ignore) Click("Continuar explorando");
                }
                Click(ignore ? "Seguir para a floresta" : "Investigar agora");
                yield return AdvanceTo(StoryPage.Battle);
                Assert.That(controller.Run.Fate,Is.EqualTo(ignore?ChildFate.Dead:ChildFate.Dying));
                for(var turn=0;turn<10 && controller.Run.page==StoryPage.Battle;turn++)
                { controller.SubmitBattle(AbilityId.BasicAttack); yield return new WaitForSecondsRealtime(1.8f); }
                yield return AdvanceTo(StoryPage.Burial);
                Assert.That(view.SceneArea.GetComponent<Image>().sprite.name, Is.EqualTo("swamp_clearing"));
                var grave = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IronTournament/Content/Story/forest_child_grave-trimmed.asset");
                Assert.That(view.SceneArea.GetComponentsInChildren<Image>().Any(i => i.sprite == grave), Is.True);
                Click("Continuar");
                Assert.That(controller.Run.page,Is.EqualTo(StoryPage.Reward));
                view.GetComponentsInChildren<Button>()[0].onClick.Invoke();
                yield return AdvanceTo(StoryPage.Finished);
                Assert.That(controller.Run.crystal,Is.False);
            }
        }

        [UnityTest]
        public IEnumerator InterfaceKeepsSquareSceneAndCanReopenEveryHouse()
        {
            yield return Load(); controller.Begin(CombatantId.Mage,12); yield return AdvanceTo(StoryPage.Village);
            Canvas.ForceUpdateCanvases();
            Assert.That(view.SceneArea.rect.width,Is.EqualTo(view.SceneArea.rect.height).Within(1));
            for(var slot=0;slot<5;slot++)
            {
                Click("Casa "+(slot+1));
                var room=view.GetComponentsInChildren<Button>().FirstOrDefault(x=>x.name=="Quarto");
                if(room!=null) { room.onClick.Invoke(); Click("Voltar à sala"); }
                Click("Voltar à vila");
                if(controller.Run.pendingCry) Click("Continuar explorando");
            }
            Assert.That(controller.Run.ExploredCount,Is.EqualTo(5));
            Click("Casa 1"); Click("Voltar à vila"); Assert.That(controller.Run.ExploredCount,Is.EqualTo(5));

        }

        [Test]
        public void CheckpointJournalRecoversPreviousValidSave()
        {
            var prefix=Prefix+".journal";
            PlayerPrefs.DeleteKey(prefix+".a"); PlayerPrefs.DeleteKey(prefix+".b");
            var store=new StorySaveStore(prefix); var run=StoryRun.Create(CombatantId.Mage,7);
            Assert.That(store.Save(run),Is.True); run.page=StoryPage.Village; Assert.That(store.Save(run),Is.True);
            PlayerPrefs.SetString(prefix+".b","corrupted");
            Assert.That(store.TryLoad(out var restored),Is.True); Assert.That(restored.page,Is.EqualTo(StoryPage.Intro));
            PlayerPrefs.DeleteKey(prefix+".a"); PlayerPrefs.DeleteKey(prefix+".b");
        }
        [TestCase(false)]
        [TestCase(true)]
        public void LegacyCrystalCheckpointGainsAStableGrave(bool appeared)
        {
            var prefix = Prefix + ".legacy";
            PlayerPrefs.DeleteKey(prefix + ".a"); PlayerPrefs.DeleteKey(prefix + ".b");
            var run = StoryRun.Create(CombatantId.Warrior, 27);
            run.page = StoryPage.ColdForest; run.PrepareGraveyard();
            run.visited = 1; run.crystal = true; run.reward = ItemId.HealingPotion;
            run.victoryHealth = 70; run.rewardAttack = 30; run.rewardDefense = 23;
            if (appeared)
            {
                run.gravesVisited = (1 << (run.skeletonTrigger - 1)) - 1;
                run.activeGrave = run.skeletonTrigger - 1; run.skeletonAppeared = true; run.page = StoryPage.SkeletonEmergence;
            }
            else { run.gravesVisited = 1 << Array.IndexOf(run.graves, (int)GraveContent.Empty); run.page = StoryPage.Graveyard; }
            run.Validate();
            var payload = JsonUtility.ToJson(run).Replace(",\"skeletonGraveNumber\":0", "");
            Assert.That(payload, Does.Not.Contain("skeletonGraveNumber"));
            string checksum;
            using (var sha = System.Security.Cryptography.SHA256.Create())
                checksum = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload)));
            PlayerPrefs.SetString(prefix + ".a", JsonUtility.ToJson(new SavedEnvelope { revision = 1, payload = payload, checksum = checksum }));
            var store = new StorySaveStore(prefix);
            Assert.That(store.TryLoad(out var restored), Is.True);
            if (appeared)
            {
                Assert.That(restored.skeletonGraveNumber, Is.EqualTo(run.activeGrave + 1));
                Assert.That(restored.graves[run.activeGrave], Is.EqualTo((int)GraveContent.Skeleton));
            }
            else
            {
                Assert.That(restored.skeletonGraveNumber, Is.EqualTo(Array.IndexOf(run.graves, (int)GraveContent.Empty) + 1));
                Assert.That(restored.TriggersSkeleton(restored.skeletonGraveNumber - 1), Is.True);
            }
            var grave = restored.skeletonGraveNumber;
            Assert.That(store.Save(restored), Is.True); Assert.That(store.TryLoad(out restored), Is.True);
            Assert.That(restored.skeletonGraveNumber, Is.EqualTo(grave));
            PlayerPrefs.DeleteKey(prefix + ".a"); PlayerPrefs.DeleteKey(prefix + ".b");
        }

        [Test]
        public void CheckpointJournalRejectsContentThatDoesNotMatchItsOffer()
        {
            var prefix=Prefix+".content";
            PlayerPrefs.DeleteKey(prefix+".a"); PlayerPrefs.DeleteKey(prefix+".b");
            var checkedStore=new StorySaveStore(prefix, candidate => candidate.reward == ItemId.None);
            var run=StoryRun.Create(CombatantId.Warrior,8); Assert.That(checkedStore.Save(run),Is.True);
            run.reward=ItemId.AttackGem; Assert.That(new StorySaveStore(prefix).Save(run),Is.True);
            Assert.That(checkedStore.TryLoad(out var restored),Is.True); Assert.That(restored.reward,Is.EqualTo(ItemId.None));
            PlayerPrefs.DeleteKey(prefix+".a"); PlayerPrefs.DeleteKey(prefix+".b");
        }
        [TearDown] public void Cleanup() { PlayerPrefs.DeleteKey(Prefix+".a"); PlayerPrefs.DeleteKey(Prefix+".b"); }
    }
}
#endif
