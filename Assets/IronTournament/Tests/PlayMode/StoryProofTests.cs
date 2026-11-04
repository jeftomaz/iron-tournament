#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using IronTournament.Bootstrap;
using IronTournament.Core;
using IronTournament.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace IronTournament.Presentation.Tests
{
    public sealed class StoryProofTests
    {
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
                var rewards=view.GetComponentsInChildren<Button>(); Assert.That(rewards.Length,Is.EqualTo(2)); rewards[0].onClick.Invoke();
                yield return AdvanceTo(StoryPage.Finished);
                Assert.That(run.crystal,Is.True); Assert.That(run.reward,Is.Not.EqualTo(ItemId.None));
                var store=new StorySaveStore(Prefix); Assert.That(store.TryLoad(out var restored),Is.True);
                Assert.That(restored.page,Is.EqualTo(StoryPage.Finished)); Assert.That(restored.foodGiven,Is.True);
            }
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
                yield return AdvanceTo(StoryPage.Burial); Click("Continuar");
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
        [TearDown] public void Cleanup() { PlayerPrefs.DeleteKey(Prefix+".a"); PlayerPrefs.DeleteKey(Prefix+".b"); }
    }
}
#endif
