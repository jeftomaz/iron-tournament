#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using IronTournament.Bootstrap;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace IronTournament.Presentation.Tests
{
    public sealed class MainMenuTests
    {
        [UnityTest]
        public IEnumerator TopicsAreAvailableBeforeTheCampaignStarts()
        {
            var scene = EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/MainMenu.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            var canvas = scene.GetRootGameObjects().Single(root => root.name == "MainMenuCanvas");
            var buttons = canvas.GetComponentsInChildren<Button>();
            var briefing = canvas.GetComponentsInChildren<Text>().Single(text => text.name == "BriefingText");

            Assert.That(buttons.Single(button => button.name == "RulesButton").interactable, Is.True);
            Assert.That(buttons.Single(button => button.name == "ObjectiveButton").interactable, Is.True);
            Assert.That(buttons.Single(button => button.name == "ChallengesButton").interactable, Is.True);

            buttons.Single(button => button.name == "RulesButton").onClick.Invoke();
            Assert.That(briefing.text, Does.Contain("Atacar"));
            buttons.Single(button => button.name == "ObjectiveButton").onClick.Invoke();
            Assert.That(briefing.text, Does.Contain("Goblin"));
            buttons.Single(button => button.name == "ChallengesButton").onClick.Invoke();
            Assert.That(briefing.text, Does.Contain("furia"));
        }

        [UnityTest]
        public IEnumerator StartButtonLoadsTheBattleScene()
        {
            var scene = EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/MainMenu.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            var start = scene.GetRootGameObjects().Single(root => root.name == "MainMenuCanvas")
                .GetComponentsInChildren<Button>().Single(button => button.name == "StartCampaign");
            start.onClick.Invoke();
            yield return null;

            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo("Assets/Scenes/Battle.unity"));
            Assert.That(Object.FindFirstObjectByType<MainMenuController>(), Is.Null);
        }
    }
}
#endif
