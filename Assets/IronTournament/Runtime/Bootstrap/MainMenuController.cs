using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IronTournament.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button rulesButton;
        [SerializeField] private Button objectiveButton;
        [SerializeField] private Button challengesButton;
        [SerializeField] private Text briefingText;
        [SerializeField] private string gameplayScene = "Battle";

        private bool isLoading;

        private void Awake()
        {
            if (startButton != null && rulesButton != null && objectiveButton != null && challengesButton != null && briefingText != null)
            {
                ShowBriefing("Selecione um topico para consultar antes de iniciar a campanha.");
                return;
            }

            Debug.LogError("Main menu requires its controls and briefing text.", this);
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
            ShowBriefing("Escolha Atacar ou Defender a cada turno. O inimigo reage depois de cada acao. Ao vencer, escolha um saque para evoluir.");
        }

        private void ShowObjective()
        {
            ShowBriefing("Venca os sete encontros da campanha: Goblin, Esqueleto, Cavaleiro, Lobisomem, Vampiro, Necromante e Rei Demonio.");
        }

        private void ShowChallenges()
        {
            ShowBriefing("Os atributos inimigos variam a cada encontro. Vampiro, Necromante e Rei Demonio entram em furia quando estao feridos.");
        }

        private void ShowBriefing(string message)
        {
            briefingText.text = message;
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
