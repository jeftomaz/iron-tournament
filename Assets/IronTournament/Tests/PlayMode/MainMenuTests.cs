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
            var heading = canvas.GetComponentsInChildren<Text>().Single(text => text.name == "BriefingHeading");
            var topics = new[]
            {
                buttons.Single(button => button.name == "RulesButton"),
                buttons.Single(button => button.name == "ObjectiveButton"),
                buttons.Single(button => button.name == "ChallengesButton")
            };

            Assert.That(topics.All(button => button.interactable && button.GetComponent<Text>() != null), Is.True);
            Assert.That(topics.All(button => button.targetGraphic.raycastTarget), Is.True);
            Assert.That(((RectTransform)topics[0].transform).anchoredPosition.y - ((RectTransform)topics[0].transform).rect.height,
                Is.GreaterThan(((RectTransform)topics[1].transform).anchoredPosition.y));
            Assert.That(((RectTransform)topics[1].transform).anchoredPosition.y - ((RectTransform)topics[1].transform).rect.height,
                Is.GreaterThan(((RectTransform)topics[2].transform).anchoredPosition.y));

            topics[0].onClick.Invoke();
            Assert.That(heading.text, Is.EqualTo("REGRAS DA ARENA"));
            Assert.That(briefing.text, Does.Contain("Atacar"));
            Assert.That(topics[0].GetComponent<Text>().color, Is.Not.EqualTo(topics[1].GetComponent<Text>().color));
            topics[1].onClick.Invoke();
            Assert.That(heading.text, Is.EqualTo("OBJETIVO DA CAMPANHA"));
            Assert.That(briefing.text, Does.Contain("Goblin"));
            topics[2].onClick.Invoke();
            Assert.That(heading.text, Is.EqualTo("DESAFIOS"));
            Assert.That(briefing.text, Does.Contain("fúria"));
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
