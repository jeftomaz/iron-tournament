using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IronTournament.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        private enum Topic
        {
            None,
            Rules,
            Objective,
            Challenges
        }

        private static readonly Color32 TopicColor = new Color32(183, 201, 170, 255);
        private static readonly Color32 ActiveTopicColor = new Color32(245, 198, 82, 255);

        [SerializeField] private Button startButton;
        [SerializeField] private Button rulesButton;
        [SerializeField] private Button objectiveButton;
        [SerializeField] private Button challengesButton;
        [SerializeField] private Text briefingHeading;
        [SerializeField] private Text briefingText;
        [SerializeField] private string gameplayScene = "Battle";

        private bool isLoading;

        private void Awake()
        {
            briefingHeading ??= briefingText != null
                ? briefingText.transform.parent.Find("BriefingHeading")?.GetComponent<Text>()
                : null;

            if (startButton != null && rulesButton != null && objectiveButton != null && challengesButton != null &&
                briefingHeading != null && briefingText != null)
            {
                ShowBriefing(Topic.None, "PRONTO PARA A ARENA",
                    "Consulte regras, objetivo e desafios. Depois, inicie a campanha contra o Goblin.");
                return;
            }

            Debug.LogError($"Main menu controls unavailable: start={startButton != null}, rules={rulesButton != null}, " +
                $"objective={objectiveButton != null}, challenges={challengesButton != null}, " +
                $"heading={briefingHeading != null}, briefing={briefingText != null}.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (startButton != null) startButton.onClick.AddListener(StartCampaign);
            if (rulesButton != null) rulesButton.onClick.AddListener(ShowRules);
            if (objectiveButton != null) objectiveButton.onClick.AddListener(ShowObjective);
            if (challengesButton != null) challengesButton.onClick.AddListener(ShowChallenges);
        }

        private void OnDisable()
        {
            if (startButton != null) startButton.onClick.RemoveListener(StartCampaign);
            if (rulesButton != null) rulesButton.onClick.RemoveListener(ShowRules);
            if (objectiveButton != null) objectiveButton.onClick.RemoveListener(ShowObjective);
            if (challengesButton != null) challengesButton.onClick.RemoveListener(ShowChallenges);
        }

        private void ShowRules()
        {
            ShowBriefing(Topic.Rules, "REGRAS DA ARENA",
                "Em cada turno, escolha Atacar ou Defender. O inimigo responde após sua ação. Ao vencer, escolha um saque.");
        }

        private void ShowObjective()
        {
            ShowBriefing(Topic.Objective, "OBJETIVO DA CAMPANHA",
                "Derrote Goblin, Esqueleto, Cavaleiro, Lobisomem, Vampiro, Necromante e, por fim, o Rei Demônio.");
        }

        private void ShowChallenges()
        {
            ShowBriefing(Topic.Challenges, "DESAFIOS",
                "Os atributos inimigos variam por encontro. Vampiro, Necromante e Rei Demônio entram em fúria quando estão feridos.");
        }

        private void ShowBriefing(Topic topic, string heading, string message)
        {
            briefingHeading.supportRichText = briefingText.supportRichText = false;
            briefingHeading.text = heading;
            briefingText.text = message;
            rulesButton.GetComponent<Text>().color = topic == Topic.Rules ? ActiveTopicColor : TopicColor;
            objectiveButton.GetComponent<Text>().color = topic == Topic.Objective ? ActiveTopicColor : TopicColor;
            challengesButton.GetComponent<Text>().color = topic == Topic.Challenges ? ActiveTopicColor : TopicColor;
        }

        public void StartCampaign()
        {
            if (isLoading) return;
            if (string.IsNullOrWhiteSpace(gameplayScene) || !Application.CanStreamedLevelBeLoaded(gameplayScene))
            {
                Debug.LogError("Main menu gameplay scene is not available in the build.", this);
                return;
            }

            isLoading = true;
            startButton.interactable = false;
            SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
        }
    }
}
