using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IronTournament.Content;
using IronTournament.Core;
using IronTournament.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace IronTournament.Bootstrap
{
    public sealed class StoryProofController : MonoBehaviour
    {
        [SerializeField] private StoryContent content;
        private StoryView view;
        private StoryRun run;
        private StorySaveStore saves;
        private IBattle battle;
        private bool busy;
        private bool atHome = true;
        private string notice;
        private Image heroImage, opponentImage, childImage;
        private Text heroHealth, opponentHealth;
        private Action resizeScreen;
        public StoryRun Run => run;
        public IBattle ActiveBattle => battle;
        public string SavePrefix { private get; set; } = "iron.story.proof.v1";
        private CombatantDefinition Hero => run.hero == CombatantId.Warrior ? content.warrior : content.mage;
        private Sprite[] HeroPoses => run.hero == CombatantId.Warrior ? content.warriorPoses : content.magePoses;
        private EncounterConfiguration Encounter => ContentMapper.BuildEncounter(content.goblin);
        private EncounterConfiguration SkeletonEncounter => ContentMapper.BuildEncounter(content.skeleton);
        private string Inventory => run == null ? "" : "PROTAGONISTA\n" + Hero.DisplayName + "\n\nBOLSA\n" +
            (run.equipment ? (run.hero == CombatantId.Mage ? "Manto de Chamas · 1 dano no Goblin, 3 no Esqueleto\n" : "Espada do Caos · 7% crítico no Goblin, 10% no Esqueleto\n") : "") +
            (run.food ? "Porção de comida\n" : "") + (run.crystal ? "Cristal do Destino\n" : "") +
            (run.reward != ItemId.None ? "Saque do Goblin: " + StoryEncounter.Rewards(run, Encounter).First(x => x.Id == run.reward).DisplayName + "\n" : "") +
            (run.skeletonReward != ItemId.None ? "Saque do Esqueleto: " + StoryEncounter.SkeletonRewards(run, SkeletonEncounter).First(x => x.Id == run.skeletonReward).DisplayName + "\n" : "") +
            (run.reward != ItemId.None ? $"HP: {(run.skeletonDefeated ? run.skeletonHealth : run.victoryHealth)} · ATQ: {(run.skeletonReward != ItemId.None ? run.skeletonAttack : run.rewardAttack)} · DEF: {(run.skeletonReward != ItemId.None ? run.skeletonDefense : run.rewardDefense)}" : "");

        private void Start()
        {
            var cameraObject = new GameObject("StoryCamera", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.localPosition = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            saves = new StorySaveStore(SavePrefix, ValidSave);
            var canvas = new GameObject("StoryProofCanvas", typeof(RectTransform));
            canvas.transform.SetParent(transform, false);
            view = canvas.AddComponent<StoryView>(); view.Initialize(); view.Resized += Resize;
            Home();
        }

        private void Resize() { if (!busy) (resizeScreen ?? Render)(); }
        private bool ValidSave(StoryRun saved)
        {
            if (saved.reward != ItemId.None && !StoryEncounter.Rewards(saved, Encounter).Any(x => x.Id == saved.reward)) return false;
            if (saved.skeletonReward != ItemId.None && !StoryEncounter.SkeletonRewards(saved, SkeletonEncounter).Any(x => x.Id == saved.skeletonReward)) return false;
            if (saved.skeletonAppeared && saved.skeletonReward == ItemId.None)
            {
                var hero = saved.hero == CombatantId.Warrior ? content.warrior : content.mage;
                StoryEncounter.RestoreSkeletonHero(saved, ContentMapper.BuildCombatant(hero), Encounter, saved.skeletonDefeated);
            }
            return true;
        }
        private void OnDestroy() { if (view != null) view.Resized -= Resize; }
        private void Save() { if (!saves.Save(run)) notice = saves.LastError; }
        private void Go(StoryPage page) { run.page = page; run.dialogue = 0; Save(); Render(); }
        private void Home()
        {
            atHome = true;
            resizeScreen = Home;
            view.Draw(content.opening, "Um chamado nas trevas", "Uma jornada. Duas classes. Suas escolhas.");
            view.SceneTitle("IRON\nTOURNAMENT", .48f);
            view.Text("A jornada", "Escolha seu protagonista e descubra o que restou da vila.");
            if (saves.TryLoad(out var saved))
            {
                view.Choice("Continuar", () => { run = saved; atHome = false; if (run.page == StoryPage.Battle) StartBattle(); else if (run.page == StoryPage.SkeletonBattle) StartSkeletonBattle(); else Render(); }, 0, 2);
                view.Choice("Nova jornada", ConfirmNew, 1, 2);
            }
            else view.Choice("Escolher protagonista", Selection);
        }

        private void ConfirmNew()
        {
            resizeScreen = ConfirmNew;
            view.Draw(content.distantVillage, "Nova jornada", "O progresso atual será substituído ao confirmar a classe.");
            view.Text("Confirmar", "Deseja iniciar outra jornada? A partida salva atual será substituída.");
            view.Choice("Escolher classe", Selection, 0, 2); view.Choice("Voltar", Home, 1, 2, StoryButtonStyle.Secondary);
        }

        private void Selection()
        {
            resizeScreen = Selection;
            view.Draw(null, "Escolha seu protagonista", "Guerreiro ou Mago");
            view.Actor(content.warriorPoses[0], .28f, .27f, .43f);
            view.Actor(content.magePoses[0], .72f, .27f, .40f);
            view.Button(view.SceneArea, "Guerreiro", .04f, .15f, .43f, .08f, () => ConfirmHero(CombatantId.Warrior));
            view.Button(view.SceneArea, "Mago", .53f, .15f, .43f, .08f, () => ConfirmHero(CombatantId.Mage));
            view.Button(view.SceneArea, "Ver atributos", .04f, .035f, .43f, .08f, () => Attributes(content.warrior), style: StoryButtonStyle.Secondary);
            view.Button(view.SceneArea, "Ver atributos", .53f, .035f, .43f, .08f, () => Attributes(content.mage), style: StoryButtonStyle.Secondary);
            view.Text("Seu caminho", "A classe permanece com você durante toda a jornada.");
            view.Choice("Voltar", Home, style: StoryButtonStyle.Secondary);
        }

        private void Attributes(CombatantDefinition hero)
        {
            resizeScreen = () => Attributes(hero);
            view.Draw(null, hero.DisplayName, "Características iniciais");
            view.Actor(hero.Id == CombatantId.Warrior ? content.warriorPoses[0] : content.magePoses[0], .5f, .17f, .50f);
            var stats = hero.BaseStats;
            view.Text("Atributos", $"HP {stats.MaximumHealth} · ATQ {stats.Attack} · DEF {stats.Defense}\n" +
                (hero.Id == CombatantId.Warrior ? "Resistente. Atacar e Defender; Pergaminho com HP ≤ 30%." : "Magia ignora defesa. Uma reversão por encontro; Chifre intercepta golpe letal."));
            view.Choice("Selecionar", () => ConfirmHero(hero.Id), 0, 2); view.Choice("Voltar", Selection, 1, 2, StoryButtonStyle.Secondary);
        }

        private void ConfirmHero(CombatantId hero)
        {
            resizeScreen = () => ConfirmHero(hero);
            view.Draw(null, hero == CombatantId.Warrior ? "Guerreiro" : "Mago", "Confirmar protagonista");
            view.Actor(hero == CombatantId.Warrior ? content.warriorPoses[0] : content.magePoses[0], .5f, .17f, .50f);
            view.Text("Iniciar jornada", "A escolha inicia uma nova partida e substitui o progresso salvo. Confirmar?");
            view.Choice("Confirmar", () => Begin(hero, Environment.TickCount), 0, 2);
            view.Choice("Voltar", Selection, 1, 2, StoryButtonStyle.Secondary);
        }

        public void Begin(CombatantId hero, int seed)
        {
            // The proof endpoint is not a completed campaign and must never unlock the second campaign.
            run = StoryRun.Create(hero, seed); atHome = false; battle = null; notice = null; Save(); Render();
        }

        public void Render()
        {
            if (view == null || busy) return;
            if (atHome) { Home(); return; }
            resizeScreen = null;
            switch (run.page)
            {
                case StoryPage.Intro: Dialogue("intro", null, "O chamado", () => Go(StoryPage.Title)); break;
                case StoryPage.Title:
                    Draw(null, "Iron Tournament"); view.SceneTitle("IRON\nTOURNAMENT");
                    view.Text("", ""); view.Choice("Continuar", () => Go(StoryPage.Chapter)); break;
                case StoryPage.Chapter: Dialogue("chapter", null, "Capítulo I · A Vila", () => Go(StoryPage.Arrival)); break;
                case StoryPage.Arrival:
                    Draw(content.distantVillage, "A vila distante"); view.Actor(HeroPoses[1], .25f, .06f, .24f);
                    view.Text("A chegada", "Uma vila em ruínas jaz sob a sombra da montanha.");
                    view.Choice("Entrar na vila", () => Go(StoryPage.Village)); break;
                case StoryPage.Village: Village(); break;
                case StoryPage.House: House(); break;
                case StoryPage.Approach:
                    Draw(content.forestPath, "Entre as árvores"); view.Actor(HeroPoses[1], .33f, .06f, .25f);
                    view.Text("Floresta alagada", run.secondCampaign || run.Fate != ChildFate.Saved ? "Grunhidos ecoam entre as árvores." : "Os gritos vêm de um trecho alagado da floresta.");
                    view.Choice("Avançar", () => Go(StoryPage.Rescue)); break;
                case StoryPage.Rescue: Dialogue("rescue" + Branch, BackgroundForEncounter, "Encontro I · Goblin", StartBattle, true); break;
                case StoryPage.Battle: if (battle == null) StartBattle(); else BattleScreen(); break;
                case StoryPage.AfterBattle:
                    Dialogue("after" + Branch, BackgroundForEncounter, "Após a batalha", () => Go(run.Fate == ChildFate.Saved ? StoryPage.Food : run.Fate == ChildFate.Absent ? StoryPage.Reward : StoryPage.Burial), true); break;
                case StoryPage.Food: Food(); break;
                case StoryPage.LootLead: Dialogue("loot", content.swamp, "O que ficou", () => Go(StoryPage.Reward), true); break;
                case StoryPage.Burial:
                    Draw(content.swamp, "Um enterro digno"); view.Actor(HeroPoses[1], .28f, .08f, .24f);
                    view.Text(Hero.DisplayName, "O protagonista se afasta do monte de terra onde enterrou a criança.");
                    view.Choice("Continuar", () => Go(StoryPage.Reward)); break;
                case StoryPage.Reward: Reward(); break;
                case StoryPage.Farewell:
                    if (run.Fate == ChildFate.Saved)
                        Dialogue("farewell", content.swamp, "O Cristal do Destino", () => { run.crystal = true; Go(StoryPage.Exit); }, true);
                    else Go(StoryPage.Exit);
                    break;
                case StoryPage.Exit:
                    Draw(content.forestPath, "O caminho continua"); view.Actor(HeroPoses[1], .5f, .18f, .22f);
                    view.Text("A jornada", "O protagonista avança entre a floresta.");
                    view.Choice("Continuar", () => Go(StoryPage.Finished)); break;
                case StoryPage.Finished:
                    Draw(content.forestPath, "Fim do primeiro encontro");
                    view.Text("A jornada continua", "O protagonista segue pela floresta.");
                    view.Choice("Continuar", () => Go(StoryPage.ColdForest), 0, 3);
                    view.Choice("Ver inventário", () => Message("Bolsa", Inventory, Render), 1, 3);
                    view.Choice("Menu", Home, 2, 3); break;
                case StoryPage.ColdForest:
                    Dialogue("skeletonIntro", content.forestPath, "A floresta fria", () => { run.PrepareGraveyard(); Go(StoryPage.Graveyard); }); break;
                case StoryPage.Graveyard: Graveyard(); break;
                case StoryPage.Grave: Grave(); break;
                case StoryPage.SkeletonEmergence:
                    Draw(content.skeletonGraveyard, "Algo se move sob a terra");
                    view.Annotation("Placeholder: mão esquelética emergindo", .04f, .60f);
                    view.Text("Entre os túmulos", "Uma mão esquelética rompe a terra. Um esqueleto sai do túmulo.");
                    view.Choice("Enfrentar", StartSkeletonBattle); break;
                case StoryPage.SkeletonBattle: if (battle == null) StartSkeletonBattle(); else BattleScreen(); break;
                case StoryPage.SkeletonLetter:
                    Message("Carta encontrada", content.Script.skeletonLetter, () => Go(StoryPage.SkeletonReward)); break;
                case StoryPage.SkeletonReward: SkeletonReward(); break;
                case StoryPage.Castle:
                    Draw(content.skeletonGraveyard, "Além das árvores"); view.Actor(HeroPoses[1], .25f, .08f, .24f);
                    view.Annotation("Placeholder: montanha e castelo distante", .10f, .70f);
                    view.Text("O castelo", "Além do topo das árvores, uma montanha e um castelo muito distante surgem no horizonte.");
                    view.Choice("Voltar aos túmulos", () => { run.castleSeen = true; Go(StoryPage.Graveyard); }); break;
                case StoryPage.SkeletonFinished:
                    Draw(content.skeletonGraveyard, "Fim do segundo encontro");
                    view.Text("Trecho concluído", "Sua jornada foi salva.");
                    view.Choice("Ver inventário", () => Message("Bolsa", Inventory, Render), 0, 2);
                    view.Choice("Menu", Home, 1, 2); break;
            }
        }

        private string Branch => run.secondCampaign ? "D" : run.Fate == ChildFate.Saved ? "A" : run.Fate == ChildFate.Dying ? "B" : "C";
        private Sprite BackgroundForEncounter => content.swamp;
        private Sprite BackgroundForMessage => run.page == StoryPage.House ? (run.room ? content.bedrooms[run.activeHouse-2] : content.houses[run.activeHouse])
            : (int)run.page >= (int)StoryPage.Graveyard ? content.skeletonGraveyard : BackgroundForEncounter;
        private void Draw(Sprite image, string title)
        {
            view.Draw(image, title, notice ?? (run.secondCampaign ? "Segunda campanha" : "Primeira campanha"), Inventory);
            if (image == content.swamp && ((run.secondCampaign && !run.previousChildSaved) ||
                (run.page == StoryPage.Burial || run.page == StoryPage.Reward) &&
                (run.Fate == ChildFate.Dying || run.Fate == ChildFate.Dead)))
                view.Actor(content.grave, .70f, .29f, .13f);
        }

        private void Dialogue(string id, Sprite background, string title, Action done, bool actors = false)
        {
            var lines = content.Dialogue(id);
            if (run.dialogue >= lines.Length) { done(); return; }
            Draw(background, title);
            if (actors) EncounterActors();
            if (id == "skeletonIntro") view.Annotation("Placeholder: floresta fria", .04f, .74f);
            var line = lines[run.dialogue];
            var speakingActor = !actors ? null : line.speaker == "Protagonista" ? heroImage :
                line.speaker == "Goblin" ? opponentImage : line.speaker == "Criança" ? childImage : null;
            view.Text(line.speaker == "Protagonista" ? Hero.DisplayName : line.speaker,
                line.text.Replace("[classe do protagonista]", Hero.DisplayName).Replace("*coff*", "coff"), true, speakingActor,
                line.speaker == "Narrador" && (id == "intro" || id == "chapter"));
            view.Choice("Continuar", () => { if (view.CompleteText()) return; run.dialogue++; Save(); Render(); });
        }

        private void EncounterActors()
        {
            childImage = null;
            heroImage = view.Actor(HeroPoses[2], .25f, .13f, run.hero == CombatantId.Warrior ? .36f : .34f);
            var before = run.page == StoryPage.Rescue;
            if (before) opponentImage = view.Actor(content.goblinPoses[1], .57f, .38f, .13f);
            else opponentImage = view.Actor(content.goblinDead, .51f, .32f, .12f);
            if (run.secondCampaign)
            {
                if (before) view.Actor(content.goblinDead, .76f, .30f, .10f);
                return;
            }
            var child = run.Fate == ChildFate.Saved ? (before ? content.child : content.childSaved)
                : run.Fate == ChildFate.Dying && (before || run.page == StoryPage.AfterBattle && run.dialogue < content.Dialogue("afterB").Length - 1)
                    ? content.childHurt : content.childDead;
            childImage = view.Actor(child, .78f, .35f, run.Fate == ChildFate.Saved ? .21f : .115f);
        }

        private void Village()
        {
            Draw(content.village, "A vila em ruínas");
            view.Actor(HeroPoses[1], .46f, .015f, .16f);
            if (run.pendingCry)
            {
                var cry = run.ExploredCount == 1 ? "Ahhhhhhhh! Alguém ainda habita aquele local?" : "Outro grito emerge da floresta.";
                view.Text(run.ExploredCount == 3 ? "Terceiro e último grito" : "Um grito na floresta", cry);
                view.Choice("Investigar agora", () => { run.BeginRescue(); Save(); Render(); }, 0, 2);
                view.Choice("Continuar explorando", () => { run.ContinueExploration(); Save(); Render(); }, 1, 2, StoryButtonStyle.Secondary);
                return;
            }
            var points = new[] { new Vector2(.08f,.55f), new Vector2(.36f,.65f), new Vector2(.75f,.49f), new Vector2(.08f,.22f), new Vector2(.68f,.20f) };
            for (var i = 0; i < 5; i++)
            {
                var slot = i; var house = run.houses[i];
                view.Hotspot("Casa " + (i + 1), points[i].x, points[i].y, () => { run.EnterHouse(slot); Save(); Render(); },
                    (run.visited & (1 << house)) != 0, run.crystal && !run.equipment && house == (run.hero == CombatantId.Warrior ? 0 : 1), display: "Entrar");
            }
            view.Text("Explorar", $"Toque em uma entrada para investigar. Casas visitadas: {run.ExploredCount}/5.");
            if (run.ExploredCount > 0 || run.secondCampaign)
            {
                view.Choice(run.lastCallDeclined ? "Seguir para a floresta" : "Investigar o som", () => { run.BeginRescue(); Save(); Render(); }, 0, 2);
                view.Choice("Menu", Home, 1, 2, StoryButtonStyle.Secondary);
            }
            else view.Choice("Menu", Home, style: StoryButtonStyle.Secondary);
        }

        private void Graveyard()
        {
            Draw(content.skeletonGraveyard, "A clareira dos túmulos");
            var points = new[] { new Vector2(.04f,.62f), new Vector2(.38f,.62f), new Vector2(.72f,.62f),
                new Vector2(.04f,.36f), new Vector2(.38f,.36f), new Vector2(.72f,.36f) };
            for (var i = 0; i < points.Length; i++)
            {
                var slot = i; var visited = (run.gravesVisited & (1 << slot)) != 0;
                var danger = run.crystal && !visited && !run.skeletonAppeared && run.GravesExplored + 1 == run.skeletonTrigger;
                view.Hotspot("Túmulo " + (slot + 1), points[i].x, points[i].y,
                    () => { run.EnterGrave(slot); Save(); Render(); }, visited, danger: danger);
            }
            view.Text("Investigar", $"Toque nos túmulos para examiná-los. Investigados: {run.GravesExplored}/6.");
            if (run.castleSeen)
            { view.Choice("Seguir viagem", () => Go(StoryPage.SkeletonFinished), 0, 2); view.Choice("Menu", Home, 1, 2); }
            else view.Choice("Menu", Home);
        }

        private void Grave()
        {
            var grave = (GraveContent)run.graves[run.activeGrave];
            var labels = new[] { "carta de Lucy", "carta de Samuel", "rosário e flores", "boneca de pano", "restos de um gato", "restos mortais" };
            Draw(content.skeletonGraveyard, "Dentro do túmulo");
            view.Annotation("Placeholder: " + labels[(int)grave], .05f, .68f);
            var letter = grave == GraveContent.LucyLetter || grave == GraveContent.SamuelLetter;
            view.Text("Investigar", letter ? "Há uma carta junto aos restos mortais." : content.Script.graves[(int)grave]);
            if (letter)
            {
                view.Choice("Ler carta", () => Message("Carta", content.Script.graves[(int)grave], Render), 0, 2);
                view.Choice("Voltar aos túmulos", () => Go(StoryPage.Graveyard), 1, 2);
            }
            else view.Choice("Voltar aos túmulos", () => Go(StoryPage.Graveyard));
        }

        private void SkeletonReward()
        {
            Draw(content.skeletonGraveyard, "Escolha um saque");
            view.Actor(content.skeletonDead, .65f, .25f, .19f);
            var offer = StoryEncounter.SkeletonRewards(run, SkeletonEncounter);
            view.Text("Leve apenas um", "Escolha um dos itens encontrados.");
            for (var i = 0; i < offer.Count; i++)
            {
                var item = offer[i];
                view.RewardChoice(item.DisplayName, DescribeItem(item), () =>
                {
                    var hero = battle != null && battle.State.Opponent.Id == CombatantId.Skeleton && battle.State.Phase == BattlePhase.Victory
                        ? battle.State.Hero : StoryEncounter.RestoreSkeletonHero(run, ContentMapper.BuildCombatant(Hero), Encounter, true);
                    StoryEncounter.ChooseSkeletonReward(run, SkeletonEncounter, item.Id, hero); Save(); Render();
                }, i, offer.Count);
            }
        }

        private void House()
        {
            var id = run.activeHouse;
            Draw(run.room ? content.bedrooms[id-2] : content.houses[id], run.room ? "O quarto" : "Dentro da casa");
            view.Text("Investigar", run.room ? "O silêncio pesa neste cômodo." : id < 2 ? "Há tábuas quebradas no chão. Um sopro de ar escapa pela abertura." : "Investigue os vestígios ou siga até o quarto.");
            view.Hotspot("Observar", .04f, .04f, () => Message("O que restou", run.room ? content.Script.bedrooms[id-2] : content.Script.houses[id], Render));
            if (!run.room && id < 2)
                view.Hotspot("Buraco no chão", .61f, .14f, InspectEquipment);
            if (!run.room && id == 3 && !run.foodCollected)
                view.Hotspot("Caldeirão", .41f, .38f, () => { run.CollectFood(); Save(); Message("Comida", "Você recolheu uma porção de comida. Talvez alguém precise dela.", Render); });
            if (!run.room && id >= 2)
                view.Hotspot("Quarto", id == 3 ? .05f : .72f, .50f, () => { run.room = true; Save(); Render(); });
            view.Choice(run.room ? "Voltar à sala" : "Voltar à vila", () =>
            { if (run.room) run.room = false; else run.LeaveHouse(); Save(); Render(); }, 0, 2, StoryButtonStyle.Secondary);
            view.Choice("Bolsa", () => Message("Bolsa", Inventory, Render), 1, 2, StoryButtonStyle.Inventory);
        }

        private void InspectEquipment()
        {
            var sword = run.activeHouse == 0;
            var compatible = sword == (run.hero == CombatantId.Warrior);
            if (compatible && run.equipment)
            { Message("Buraco no chão", "Você já recolheu o equipamento. A carta dizia: A Ordem e a Irmandade cairão. O Rei vive.", Render); return; }
            var opening = sword ? "Você encontra uma espada antiga." : "Você encontra um manto antigo.";
            var result = sword
                ? (compatible ? "Ela é pesada, mas você consegue a levantar." : "Ela é pesada demais para você levantar.")
                : (compatible ? "Ele está estranhamente quente, mas você o consegue pegar." : "Ele queima sua mão e você não consegue pegar ele.");
            if (!compatible) { Message("Buraco no chão", opening + "\n\n" + result, Render); return; }
            if (!run.CollectEquipment()) return;
            Save(); Message("Buraco no chão", opening + "\n\n" + result,
                () => Message("Carta", "A Ordem e a Irmandade cairão.\n…\nO Rei vive.", Render));
        }

        private void Food()
        {
            if (!run.food) { Go(StoryPage.LootLead); return; }
            Draw(content.swamp, "Um pouco de comida"); EncounterActors();
            view.Text("Sua escolha", "Entregar a comida que encontrou à criança?");
            view.Choice("Entregar", () => { run.page = StoryPage.AfterBattle; run.GiveFood(); run.page = StoryPage.Food; Save();
                Message("Criança", "Obrigado!", () => Go(StoryPage.LootLead)); }, 0, 2);
            view.Choice("Guardar", () => Go(StoryPage.LootLead), 1, 2, StoryButtonStyle.Secondary);
        }

        private void Reward()
        {
            Draw(BackgroundForEncounter, "Escolha um saque");
            if (run.Fate == ChildFate.Saved) EncounterActors();
            var offer = StoryEncounter.Rewards(run, Encounter);
            view.Text("Leve apenas um", "Escolha um dos itens encontrados.");
            for (var i = 0; i < offer.Count; i++)
            {
                var item = offer[i];
                view.RewardChoice(item.DisplayName, DescribeItem(item), () =>
                {
                    var rewarded = battle?.State.Hero ?? StoryEncounter.RestoreVictoryHero(run, ContentMapper.BuildCombatant(Hero));
                    StoryEncounter.ChooseReward(run, Encounter, item.Id);
                    StoryEncounter.ApplyReward(run, Encounter, rewarded);
                    run.victoryHealth = rewarded.CurrentHealth;
                    run.rewardAttack = rewarded.Stats.Attack;
                    run.rewardDefense = rewarded.Stats.Defense;
                    Save(); Render();
                }, i, offer.Count);
            }
        }

        private static string DescribeItem(ItemConfiguration item)
        {
            switch (item.Id)
            {
                case ItemId.HealingPotion: return "recupera 40 HP";
                case ItemId.LifeElixir: return "recupera todo o HP";
                case ItemId.HopeScroll: return "cura 50% do HP máximo com HP ≤ 30%";
                case ItemId.BrotherhoodHorn: return "Ao ser armado, protege de um golpe letal.";
                default: return string.Join(", ", item.Modifiers.Select(x => (x.Kind == ItemModifierKind.Attack ? "ATQ" : x.Kind == ItemModifierKind.Defense ? "DEF" : "HP máximo") + " +" + x.Amount));
            }
        }

        private void Message(string title, string text, Action done, int offset = 0)
        {
            resizeScreen = () => Message(title, text, done, offset);
            var paragraphEnd = text.IndexOf("\n\n", offset, StringComparison.Ordinal);
            var end = paragraphEnd >= 0 ? paragraphEnd : text.Length;
            if (end - offset > 150)
            {
                end = offset + 150;
                var space = text.LastIndexOf(' ', end - 1, end - offset);
                if (space > offset) end = space;
            }
            var next = end;
            while (next < text.Length && char.IsWhiteSpace(text[next])) next++;
            Draw(BackgroundForMessage, title);
            if (run.page == StoryPage.SkeletonLetter)
            { view.Actor(content.skeletonDead, .65f, .25f, .19f); view.Annotation("Placeholder: carta do esqueleto", .05f, .68f); }
            var actor = (Image)null;
            if (title == "Criança" && run.page == StoryPage.Food) { EncounterActors(); actor = childImage; }
            view.Text(title, text.Substring(offset, end - offset).Trim(), true, actor);
            view.Choice("Continuar", () => { if (view.CompleteText()) return; if (next >= text.Length) done(); else Message(title, text, done, next); });
        }

        private void StartBattle()
        {
            battle = StoryEncounter.Create(run, ContentMapper.BuildCombatant(Hero), Encounter);
            run.page = StoryPage.Battle; run.dialogue = 0; Save(); BattleScreen();
        }

        private void StartSkeletonBattle()
        {
            battle = StoryEncounter.CreateSkeleton(run, ContentMapper.BuildCombatant(Hero), Encounter, SkeletonEncounter);
            run.page = StoryPage.SkeletonBattle; run.dialogue = 0; Save(); BattleScreen();
        }

        private void BattleScreen()
        {
            var skeleton = run.page == StoryPage.SkeletonBattle;
            Draw(skeleton ? content.skeletonGraveyard : BackgroundForEncounter, skeleton ? "Encontro II · Esqueleto" : "Encontro I · Goblin");
            heroImage = view.Actor(HeroPoses[3], .28f, .13f, run.hero == CombatantId.Warrior ? .38f : .359f);
            // Apparent enemy height = physical height ratio × camera-depth ratio (4m / 5.5m).
            var enemyHeight = .38f * ((skeleton ? 1.5f : .8f) / 1.8f) * (4f / 5.5f);
            opponentImage = view.Actor(skeleton ? content.skeletonPose : content.goblinPoses[2], .69f, .43f, enemyHeight);
            var state = battle.State;
            heroHealth = view.Health(heroImage, Hero.DisplayName, state.Hero.CurrentHealth, state.Hero.Stats.MaximumHealth);
            opponentHealth = view.Health(opponentImage, skeleton ? "Esqueleto" : "Goblin", state.Opponent.CurrentHealth, state.Opponent.Stats.MaximumHealth);
            view.Text("Sua vez · rodada " + state.Round, $"{(run.hero == CombatantId.Mage ? "Reversões: " + state.RevertCharges + ". " : "")}" +
                (run.equipment ? run.hero == CombatantId.Mage ? $"Manto: {(skeleton ? 3 : 1)} de dano por ação." : $"Espada: {(skeleton ? 10 : 7)}% de crítico." : "Escolha sua ação."));
            var abilities = Hero.Abilities;
            var visible = abilities.Where(x => battle.AvailableActions.Contains(x.Id)).ToArray();
            for (var i = 0; i < visible.Length; i++)
            { var ability = visible[i]; view.Choice(ability.DisplayName, () => SubmitBattle(ability.Id), i, visible.Length); }
        }

        public void SubmitBattle(AbilityId action)
        {
            if (busy || battle == null || (run.page != StoryPage.Battle && run.page != StoryPage.SkeletonBattle)) return;
            var result = battle.Submit(action);
            if (!result.IsAccepted) return;
            StartCoroutine(Playback(result));
        }

        private IEnumerator Playback(ActionResult result)
        {
            busy = true;
            foreach (var button in view.GetComponentsInChildren<Button>()) button.interactable = false;
            foreach (var change in result.Events)
            {
                if (change is DamageDealtEvent damage)
                {
                    view.PlaySound("Hit");
                    var image = damage.Target == run.hero ? heroImage : opponentImage;
                    if (damage.Target == run.hero) view.SetHealth(heroHealth, damage.RemainingHealth, battle.State.Hero.Stats.MaximumHealth);
                    else view.SetHealth(opponentHealth, damage.RemainingHealth, battle.State.Opponent.Stats.MaximumHealth);
                    image.color = new Color(1, .35f, .3f);
                    view.Annotation((damage.IsCritical ? "Crítico! " : "") + "−" + damage.Amount, damage.Target == run.hero ? .12f : .58f, damage.Target == run.hero ? .35f : .56f);
                    yield return new WaitForSecondsRealtime(.22f);
                    image.color = Color.white;
                }
                else if (change is HealthRestoredEvent healed)
                {
                    view.PlaySound("Heal");
                    if (healed.Target == run.hero) view.SetHealth(heroHealth, healed.CurrentHealth, battle.State.Hero.Stats.MaximumHealth);
                    else view.SetHealth(opponentHealth, healed.CurrentHealth, battle.State.Opponent.Stats.MaximumHealth);
                }
                else if (change is AbilityUsedEvent used)
                {
                    if (used.Ability == AbilityId.BasicAttack)
                        view.PlaySound(used.Actor == run.hero && run.hero == CombatantId.Mage ? "Magic" : "Slash");
                    else if (used.Ability == AbilityId.Guard) view.PlaySound("Block");
                    else if (used.Ability == AbilityId.RevertTurn || used.Ability == AbilityId.RevertBattle) view.PlaySound("Magic");
                    var image = used.Actor == run.hero ? heroImage : opponentImage;
                    var origin = image.rectTransform.anchoredPosition;
                    image.rectTransform.anchoredPosition += new Vector2(used.Actor == run.hero ? 10 : -10, 4);
                    yield return new WaitForSecondsRealtime(.12f);
                    image.rectTransform.anchoredPosition = origin;
                }
            }
            busy = false;
            var skeleton = run.page == StoryPage.SkeletonBattle;
            if (battle.State.Phase == BattlePhase.Victory)
            {
                view.PlaySound("Defeat");
                if (skeleton)
                {
                    var hero = battle.State.Hero; run.skeletonDefeated = true; run.skeletonHealth = hero.CurrentHealth;
                    run.skeletonScrolls = hero.HopeScrolls; run.skeletonHorns = hero.GuardianHorns;
                    run.skeletonGuardianArmed = hero.IsGuardianArmed; Go(StoryPage.SkeletonLetter);
                }
                else { run.victoryHealth = battle.State.Hero.CurrentHealth; Go(StoryPage.AfterBattle); }
            }
            else if (battle.State.Phase == BattlePhase.Defeat)
            {
                Draw(skeleton ? content.skeletonGraveyard : content.swamp, "Derrota"); view.Text("Tentar novamente", "O estado anterior à batalha foi preservado. Você não precisa repetir a exploração.");
                view.Choice("Repetir batalha", skeleton ? (Action)StartSkeletonBattle : StartBattle, 0, 2); view.Choice("Menu", Home, 1, 2, StoryButtonStyle.Secondary);
            }
            else BattleScreen();
        }
    }
}
