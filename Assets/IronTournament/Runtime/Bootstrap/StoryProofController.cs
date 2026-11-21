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
        private readonly IRandomSource explorationRandom = new SeededRandomSource(Environment.TickCount);
        public StoryRun Run => run;
        public IBattle ActiveBattle => battle;
        public string SavePrefix { private get; set; } = "iron.story.proof.v1";
        private CombatantDefinition Hero => run.hero == CombatantId.Warrior ? content.warrior : content.mage;
        private Sprite[] HeroPoses => run.hero == CombatantId.Warrior ? content.warriorPoses : content.magePoses;
        private EncounterConfiguration Encounter => ContentMapper.BuildEncounter(content.goblin);
        private EncounterConfiguration SkeletonEncounter => ContentMapper.BuildEncounter(content.skeleton);
        private EncounterConfiguration KnightEncounter => ContentMapper.BuildEncounter(content.knight);
        private EncounterConfiguration WerewolfEncounter => ContentMapper.BuildEncounter(content.werewolf);
        private string Inventory => run == null ? "" : "PROTAGONISTA\n" + Hero.DisplayName + "\n\nINVENTÁRIO\n" +
            (run.equipment ? (run.hero == CombatantId.Mage ? "Manto de Chamas · 1 dano no Goblin, 3 no Esqueleto\n" : "Espada do Caos · 7% crítico no Goblin, 10% no Esqueleto\n") : "") +
            (run.food ? "Porção de comida\n" : "") + (run.crystal ? "Cristal do Destino\n" : "") +
            (run.redKey ? "Chave Rubra\n" : "") + (run.blueKey ? "Chave Azul (provisória)\n" : "") +
            (run.purpleKey ? "Chave Púrpura\n" : "") + (run.flint ? "Pederneira e metal\n" : "") +
            (run.woodenStake ? "Estaca de madeira\n" : "") + (run.mirrorShield ? "Escudo Espelho\n" : "") +
            (run.amethystAmulet ? "Amuleto Ametista\n" : "") +
            (run.reward != ItemId.None ? "Saque do Goblin: " + StoryEncounter.Rewards(run, Encounter).First(x => x.Id == run.reward).DisplayName + "\n" : "") +
            (run.skeletonReward != ItemId.None ? "Saque do Esqueleto: " + StoryEncounter.SkeletonRewards(run, SkeletonEncounter).First(x => x.Id == run.skeletonReward).DisplayName + "\n" : "") +
            (run.knightReward != ItemId.None ? "Saque do Cavaleiro: " + StoryEncounter.LaterRewards(run, KnightEncounter).First(x => x.Id == run.knightReward).DisplayName + "\n" : "") +
            (run.werewolfReward != ItemId.None ? "Saque do Lobisomem: " + StoryEncounter.LaterRewards(run, WerewolfEncounter).First(x => x.Id == run.werewolfReward).DisplayName + "\n" : "") +
            (run.reward != ItemId.None ? $"HP: {(run.werewolfDefeated ? run.werewolfHealth : run.knightDefeated && run.knightHealth > 0 ? run.knightHealth : run.skeletonDefeated ? run.skeletonHealth : run.victoryHealth)} · " +
            $"ATQ: {(run.werewolfReward != ItemId.None ? run.werewolfAttack : run.knightReward != ItemId.None ? run.knightAttack : run.skeletonReward != ItemId.None ? run.skeletonAttack : run.rewardAttack)} · " +
            $"DEF: {(run.werewolfReward != ItemId.None ? run.werewolfDefense : run.knightReward != ItemId.None ? run.knightDefense : run.skeletonReward != ItemId.None ? run.skeletonDefense + (run.mirrorShield ? 10 : 0) : run.rewardDefense)}" : "");

        private void Start()
        {
            var cameraObject = new GameObject("StoryCamera", typeof(Camera), typeof(AudioListener));
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
            if (saved.knightDefeated && saved.knightHealth > 0)
            {
                var hero = ContentMapper.BuildCombatant(saved.hero == CombatantId.Warrior ? content.warrior : content.mage);
                StoryEncounter.RestoreKnightVictory(saved, hero, Encounter, SkeletonEncounter, KnightEncounter);
                if (saved.werewolfDefeated)
                    StoryEncounter.RestoreWerewolfVictory(saved, hero, Encounter, SkeletonEncounter, KnightEncounter, WerewolfEncounter);
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
            view.Draw(content.opening, "Até que o último caia", "", showChapter: false);
            view.SceneTitle("IRON\nTOURNAMENT", .48f);
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
            view.Actor(hero.Id == CombatantId.Warrior ? content.warriorPoses[0] : content.magePoses[0], .5f, hero.Id == CombatantId.Warrior ? .11f : .17f, .50f);
            var stats = hero.BaseStats;
            view.Text("Atributos", $"HP {stats.MaximumHealth} · ATQ {stats.Attack} · DEF {stats.Defense}\n" +
                (hero.Id == CombatantId.Warrior
                ? "Um combatente experiênte, calejado pelos longos anos de batalhas. Resistente, porém suceptível à defesas fortes."
                : "Jovem e pronto para testar seus conhecimentos arcanos. Poderoso, porém vulnerável."),
                heightOverride: hero.Id == CombatantId.Warrior ? .40f : (float?)null);
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
                    view.Choice("Ver inventário", () => Message("Inventário", Inventory, Render), 1, 3);
                    view.Choice("Menu", Home, 2, 3); break;
                case StoryPage.ColdForest:
                    Dialogue("skeletonIntro", run.dialogue >= 3 ? content.coldClearing : content.coldForest, "A floresta fria", () => { run.PrepareGraveyard(); Go(StoryPage.Graveyard); }); break;
                case StoryPage.Graveyard: Graveyard(); break;
                case StoryPage.Grave: Grave(); break;
                case StoryPage.SkeletonEmergence:
                    Draw(content.skeletonHand, "Algo se move sob a terra");
                    view.Text("Entre os túmulos", "Uma mão esquelética rompe a terra. Um esqueleto sai do túmulo.");
                    view.Choice("Enfrentar", StartSkeletonBattle); break;
                case StoryPage.SkeletonBattle: if (battle == null) StartSkeletonBattle(); else BattleScreen(); break;
                case StoryPage.SkeletonLetter:
                    Message("Carta encontrada", content.Script.skeletonLetter, () => Go(StoryPage.SkeletonReward)); break;
                case StoryPage.SkeletonReward: SkeletonReward(); break;
                case StoryPage.Castle:
                    Draw(content.mountain, "Além das árvores"); view.Actor(HeroPoses[1], .25f, .08f, .24f);
                    view.Text("O castelo", "Além do topo das árvores, uma montanha e um castelo muito distante surgem no horizonte.");
                    view.Choice("Voltar aos túmulos", () => { run.castleSeen = true; Go(StoryPage.Graveyard); }); break;
                case StoryPage.SkeletonFinished:
                    Draw(content.mountain, "O caminho para o castelo");
                    view.Text("A jornada continua", "A Ponte das Dores leva à entrada do castelo.");
                    view.Choice("Seguir para a ponte", () => { run.EnterBridge(); Save(); Render(); }, 0, 2);
                    view.Choice("Menu", Home, 1, 2); break;
                case StoryPage.Bridge: Bridge(); break;
                case StoryPage.BridgeCage: Cage(); break;
                case StoryPage.BridgeFall:
                    Draw(content.bridge, "A queda"); view.Text("Em desenvolvimento", "O caminho após a queda ainda está em desenvolvimento.");
                    view.Choice("Menu", Home); break;
                case StoryPage.Courtyard:
                    if (!run.courtyardTalked) Dialogue("courtyardIntro", content.courtyard, "Pátio dos Caídos", () => { run.courtyardTalked = true; run.dialogue = 0; Save(); Render(); });
                    else Dialogue("knightIntro", content.courtyard, "Encontro III · Cavaleiro", StartKnightBattle);
                    break;
                case StoryPage.KnightBattle: if (battle == null || battle.State.Opponent.Id != CombatantId.Knight) StartKnightBattle(); else BattleScreen(); break;
                case StoryPage.KnightReward: LaterReward(KnightEncounter); break;
                case StoryPage.Anteroom: AnteroomScreen(); break;
                case StoryPage.CarnalHall: CarnalHall(); break;
                case StoryPage.Cauldron: Cauldron(); break;
                case StoryPage.WerewolfEmergence: Dialogue("werewolfIntro", content.hall, "A criatura", StartWerewolfBattle); break;
                case StoryPage.WerewolfBattle: if (battle == null || battle.State.Opponent.Id != CombatantId.Werewolf) StartWerewolfBattle(); else BattleScreen(); break;
                case StoryPage.WerewolfReward: LaterReward(WerewolfEncounter); break;
            }
        }

        private string Branch => run.secondCampaign ? "D" : run.Fate == ChildFate.Saved ? "A" : run.Fate == ChildFate.Dying ? "B" : "C";
        private Sprite BackgroundForEncounter => content.swamp;
        private Sprite BackgroundForMessage => run.page == StoryPage.House ? (run.room ? content.bedrooms[run.activeHouse-2] : content.houses[run.activeHouse])
            : run.page == StoryPage.BridgeCage ? content.cages[run.activeCage]
            : run.page == StoryPage.Bridge || run.page == StoryPage.BridgeFall ? content.bridge
            : run.page == StoryPage.Anteroom ? content.anteroom
            : run.page == StoryPage.CarnalHall || run.page == StoryPage.Cauldron || run.page == StoryPage.WerewolfReward ? content.cauldrons
            : (int)run.page >= (int)StoryPage.Graveyard ? content.skeletonGraveyard : BackgroundForEncounter;
        private void Draw(Sprite image, string title)
        {
            view.Draw(image, title, notice ?? (run.secondCampaign ? "Segunda campanha" : "Primeira campanha"), Inventory,
                (int)run.page >= (int)StoryPage.ColdForest);
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
            var laterEncounter = id == "knightIntro" || id == "werewolfIntro";
            if (laterEncounter)
            {
                heroImage = view.Actor(HeroPoses[2], .27f, .12f, ApparentHeight(.36f, HeroPoses[2], run.hero, 4f));
                var sprite = id == "knightIntro" ? content.knightPose : content.werewolfPose;
                opponentImage = view.Actor(sprite, .70f, .30f, ApparentHeight(.36f, sprite,
                    id == "knightIntro" ? CombatantId.Knight : CombatantId.Werewolf, 5.5f));
            }
            var line = lines[run.dialogue];
            var speakingActor = !actors && !laterEncounter ? null : line.speaker == "Protagonista" ? heroImage :
                line.speaker == "Goblin" || line.speaker == "Cavaleiro" || line.speaker == "Lobisomem" ? opponentImage : line.speaker == "Criança" ? childImage : null;
            view.Text(line.speaker == "Protagonista" ? Hero.DisplayName : line.speaker,
                line.text.Replace("[classe do protagonista]", Hero.DisplayName).Replace("*coff*", "coff"), true, speakingActor,
                line.speaker == "Narrador" && (id == "intro" || id == "chapter"));
            view.Choice("Continuar", () => { if (view.CompleteText()) return; run.dialogue++; Save(); Render(); });
        }

        private void EncounterActors()
        {
            childImage = null;
            heroImage = view.Actor(HeroPoses[2], .25f, .13f, ApparentHeight(.36f, HeroPoses[2], run.hero, 4f));
            var before = run.page == StoryPage.Rescue;
            if (before) opponentImage = view.Actor(content.goblinPoses[1], .57f, .38f, ApparentHeight(.36f, content.goblinPoses[1], CombatantId.Goblin, 5.5f));
            else opponentImage = view.Actor(content.goblinDead, .51f, .32f, .12f);
            if (run.secondCampaign)
            {
                if (before) view.Actor(content.goblinDead, .76f, .30f, .10f);
                return;
            }
            var child = run.Fate == ChildFate.Saved ? (before ? content.child : content.childSaved)
                : run.Fate == ChildFate.Dying && (before || run.page == StoryPage.AfterBattle && run.dialogue < content.Dialogue("afterB").Length - 1)
                    ? content.childHurt : content.childDead;
            childImage = view.Actor(child, .78f, .35f, run.Fate == ChildFate.Saved
                ? ApparentHeight(.36f, child, .8f, child.rect.height, 5.5f) : .115f);
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
            var points = new[] { new Vector2(.23f,.63f), new Vector2(.68f,.55f), new Vector2(.08f,.40f),
                new Vector2(.50f,.37f), new Vector2(.10f,.18f), new Vector2(.74f,.23f) };
            for (var i = 0; i < points.Length; i++)
            {
                var slot = i; var visited = (run.gravesVisited & (1 << slot)) != 0;
                var grave = (GraveContent)run.graves[i];
                var reward = run.crystal && !visited && (grave == GraveContent.LucyLetter || grave == GraveContent.SamuelLetter);
                var danger = run.crystal && run.TriggersSkeleton(slot);
                view.Hotspot("Túmulo " + (slot + 1), points[i].x, points[i].y,
                    () => { run.EnterGrave(slot); Save(); Render(); }, visited, reward: reward, danger: danger);
            }
            view.Text("Investigar", $"Toque nos túmulos para examiná-los. Investigados: {run.GravesExplored}/6.");
            if (run.castleSeen)
            { view.Choice("Seguir viagem", () => Go(StoryPage.SkeletonFinished), 0, 2); view.Choice("Menu", Home, 1, 2); }
            else view.Choice("Menu", Home);
        }

        private void Bridge()
        {
            Draw(content.bridge, "Ponte das Dores");
            var points = new[] { new Vector2(.16f,.43f), new Vector2(.32f,.49f), new Vector2(.45f,.52f),
                new Vector2(.57f,.54f), new Vector2(.79f,.48f), new Vector2(.84f,.39f) };
            for (var i = 0; i < points.Length; i++)
            {
                var slot = i; var visited = (run.cagesVisited & (1 << run.cages[i])) != 0;
                view.Hotspot("Gaiola " + (i + 1), points[i].x, points[i].y,
                    () => { run.EnterCage(slot, explorationRandom); Save(); Render(); }, visited, display: "Examinar");
            }
            view.Text("Investigar", $"Gaiolas examinadas: {run.CagesExplored}/6. Você pode seguir sem examiná-las.");
            view.Choice("Entrar no castelo", () => { run.LeaveBridge(); run.dialogue = 0; Save(); Render(); }, 0, 2);
            view.Choice("Menu", Home, 1, 2, StoryButtonStyle.Secondary);
        }

        private void Cage()
        {
            var cage = (CageContent)run.activeCage;
            var collected = cage == CageContent.LetterAndAmulet && run.amethystAmulet || cage == CageContent.RedKey && run.redKey ||
                cage == CageContent.MirrorShield && run.mirrorShield || cage == CageContent.Flint && run.flint || cage == CageContent.BlueKey && run.blueKey;
            Draw(collected ? content.cagesCollected[run.activeCage] : content.cages[run.activeCage], "Dentro da gaiola");
            var descriptions = new[] { "Restos de uma pessoa, algemas e trapos repousam no fundo.",
                "Há ossos, uma carta e um amuleto arroxeado.", "Uma chave avermelhada está entre os ossos.",
                "Um corvo se alimenta dos restos. Há algo redondo e brilhante sob eles.",
                "Há pederneiras e um pedaço de metal.", "Uma chave azulada está envolta em tecidos. Arte provisória." };
            view.Text("Investigar", descriptions[run.activeCage]);
            if (cage == CageContent.LetterAndAmulet)
                view.Hotspot("Ler carta", .12f, .25f, () => Message("Carta do Prisioneiro", content.Script.prisonerLetter, Render));
            if (cage != CageContent.Remains)
                view.Choice("Recolher objeto", () =>
                {
                    var obtained = run.CollectBridgeItem(); Save();
                    var result = obtained ? "Objeto recolhido." : cage == CageContent.MirrorShield && run.hero == CombatantId.Mage
                        ? "O escudo é pesado demais para levantar." : cage == CageContent.LetterAndAmulet && run.hero == CombatantId.Warrior
                        ? "O amuleto se expande em espinhos e fere sua mão antes de cair na gaiola." : "Não há outro objeto para recolher.";
                    Message("Gaiola", result, Render);
                }, 0, 2);
            view.Choice("Voltar à ponte", () => { run.LeaveCage(); Save(); Render(); }, cage == CageContent.Remains ? 0 : 1,
                cage == CageContent.Remains ? 1 : 2, StoryButtonStyle.Secondary);
        }

        private void AnteroomScreen()
        {
            if (!run.anteroomTalked)
            {
                Dialogue(run.werewolfDefeated ? "anteroomReturn" : "anteroomIntro", content.anteroom, "Antessala Efêmera",
                    () => { run.anteroomTalked = true; run.dialogue = 0; Save(); Render(); }); return;
            }
            Draw(content.anteroom, "Antessala Efêmera");
            view.Text(run.hero == CombatantId.Mage ? "Fenda Espaço-Temporal" : "Portal Misterioso", "Escolha como abrir a porta.");
            var choices = new List<(string label, Action action)>();
            choices.Add(("Abrir sem chave", () => { run.ReturnToColdForest(); battle = null; Save(); Render(); }));
            if (run.redKey) choices.Add(("Usar Chave Rubra", () => { run.EnterCarnalHall(); Save(); Render(); }));
            if (run.purpleKey) choices.Add(("Usar Chave Púrpura", () => Message("Em desenvolvimento", "O caminho após o Salão Carnal ainda está em desenvolvimento.", Render)));
            if (run.blueKey) choices.Add(("Usar Chave Azul", () => Message("Em desenvolvimento", "O caminho da chave azul ainda está em desenvolvimento.", Render)));
            choices.Add(("Menu", Home));
            for (var i = 0; i < choices.Count; i++)
            { var choice = choices[i]; view.Choice(choice.label, choice.action, i, choices.Count); }
        }

        private void CarnalHall()
        {
            if (!run.hallIntroSeen)
            {
                Dialogue("hallIntro", content.hall, "Salão Carnal", () => { run.hallIntroSeen = true; run.dialogue = 0; Save(); Render(); });
                return;
            }
            Draw(content.cauldrons, "Salão Carnal");
            if (run.werewolfDefeated && !run.werewolfBurned) view.Actor(content.werewolfDead, .50f, .12f, .18f);
            var points = new[] { new Vector2(.10f,.48f), new Vector2(.27f,.48f), new Vector2(.43f,.48f),
                new Vector2(.60f,.48f), new Vector2(.76f,.48f) };
            for (var i = 0; i < points.Length; i++)
            {
                var slot = i; var visited = (run.cauldronsVisited & (1 << i)) != 0;
                view.Hotspot("Caldeirão " + (i + 1), points[i].x, points[i].y,
                    () => { run.EnterCauldron(slot, explorationRandom); Save(); Render(); }, visited, display: "Examinar");
            }
            view.Text("Investigar", run.werewolfDefeated ? "O lobisomem foi derrotado. A porta está aberta." : "As criaturas fugiram para a escuridão. A porta está fechada.");
            if (run.werewolfDefeated)
            {
                if (run.flint && !run.werewolfBurned)
                    view.Choice("Queimar o corpo", () => { run.BurnWerewolf(); Save(); Message("Pederneira", "Você ateia fogo ao corpo do lobisomem.", Render); }, 0, 2);
                view.Choice("Deixar o salão", () => { run.LeaveCarnalHall(); Save(); Render(); }, run.flint && !run.werewolfBurned ? 1 : 0,
                    run.flint && !run.werewolfBurned ? 2 : 1);
            }
            else view.Choice("Menu", Home);
        }

        private void Cauldron()
        {
            var slot = run.activeCauldron;
            var image = slot == 1 && run.woodenStake ? content.cauldronCollected[0] :
                slot == 4 && run.purpleKey ? content.cauldronCollected[1] : content.cauldronContents[slot];
            Draw(image, "Caldeirão " + (slot + 1));
            var descriptions = new[] { "Restos fétidos flutuam no líquido avermelhado.", "Há uma estaca de madeira na mistura rasa.",
                "Uma ninhada de ratos dorme no caldeirão.", "O caldeirão está vazio.", "Uma chave púrpura boia na mistura." };
            view.Text("Investigar", slot == 1 && run.woodenStake ? "A estaca foi recolhida." :
                slot == 4 && run.purpleKey ? "A chave foi recolhida." : descriptions[slot]);
            var hasItem = (slot == 1 && !run.woodenStake) || (slot == 4 && !run.purpleKey);
            if (hasItem) view.Choice("Recolher", () => { run.CollectCauldronItem(); Save(); Render(); }, 0, 2);
            view.Choice("Voltar ao salão", () => { run.LeaveCauldron(); Save(); Render(); }, hasItem ? 1 : 0, hasItem ? 2 : 1);
        }

        private void Grave()
        {
            var grave = (GraveContent)run.graves[run.activeGrave];
            if (grave == GraveContent.Skeleton)
            {
                Draw(content.coffin, "Dentro do túmulo");
                view.Actor(content.skeletonDead, .65f, .25f, .19f);
                view.Text("Investigar", "Restam apenas os ossos do Esqueleto.");
                view.Choice("Voltar aos túmulos", () => Go(StoryPage.Graveyard)); return;
            }
            Draw(content.coffin, "Dentro do túmulo");
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
            var experience = BattleExperience.For(CombatantId.Skeleton);
            view.Text("Experiência de batalha", $"ATQ +{experience.Attack} · DEF +{experience.Defense}\nEscolha um dos itens encontrados.");
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

        private void LaterReward(EncounterConfiguration encounter)
        {
            var knight = encounter.Opponent.Id == CombatantId.Knight;
            Draw(knight ? content.courtyard : content.hall, "Escolha um saque");
            var gain = BattleExperience.For(encounter.Opponent.Id);
            view.Text(knight ? "O Cavaleiro caiu" : "O Lobisomem caiu",
                knight ? $"Sua marca foi apagada. Descanse em paz.\nATQ +{gain.Attack} · DEF +{gain.Defense}" :
                $"A criatura caiu.\nATQ +{gain.Attack} · DEF +{gain.Defense}");
            var offer = StoryEncounter.LaterRewards(run, encounter);
            for (var i = 0; i < offer.Count; i++)
            {
                var item = offer[i];
                view.RewardChoice(item.DisplayName, DescribeItem(item), () =>
                {
                    var hero = ContentMapper.BuildCombatant(Hero);
                    var player = knight ? StoryEncounter.RestoreKnightVictory(run, hero, Encounter, SkeletonEncounter, KnightEncounter)
                        : StoryEncounter.RestoreWerewolfVictory(run, hero, Encounter, SkeletonEncounter, KnightEncounter, WerewolfEncounter);
                    StoryEncounter.ChooseLaterReward(run, encounter, item.Id, player); battle = null; Save(); Render();
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
            view.Choice("Inventário", () => Message("Inventário", Inventory, Render), 1, 2, StoryButtonStyle.Inventory);
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
            var experience = BattleExperience.For(CombatantId.Goblin);
            view.Text("Experiência de batalha", $"ATQ +{experience.Attack} · DEF +{experience.Defense}\nEscolha um dos itens encontrados.");
            for (var i = 0; i < offer.Count; i++)
            {
                var item = offer[i];
                view.RewardChoice(item.DisplayName, DescribeItem(item), () =>
                {
                    var rewarded = battle?.State.Hero ?? StoryEncounter.RestoreVictoryHero(run, ContentMapper.BuildCombatant(Hero));
                    StoryEncounter.ChooseReward(run, Encounter, item.Id);
                    StoryEncounter.ApplyGoblinExperience(run, rewarded);
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

        private void StartKnightBattle()
        {
            battle = StoryEncounter.CreateKnight(run, ContentMapper.BuildCombatant(Hero), Encounter, SkeletonEncounter, KnightEncounter);
            run.page = StoryPage.KnightBattle; run.dialogue = 0; Save(); BattleScreen();
        }

        private void StartWerewolfBattle()
        {
            run.page = StoryPage.WerewolfBattle; run.dialogue = 0;
            battle = StoryEncounter.CreateWerewolf(run, ContentMapper.BuildCombatant(Hero), Encounter, SkeletonEncounter, KnightEncounter, WerewolfEncounter);
            Save(); BattleScreen();
        }

        private void BattleScreen()
        {
            var skeleton = run.page == StoryPage.SkeletonBattle;
            var knight = run.page == StoryPage.KnightBattle;
            var werewolf = run.page == StoryPage.WerewolfBattle;
            Draw(skeleton ? content.skeletonGraveyard : knight ? content.courtyard : werewolf ? content.hall : BackgroundForEncounter,
                skeleton ? "Encontro II · Esqueleto" : knight ? "Encontro III · Cavaleiro" : werewolf ? "Encontro IV · Lobisomem" : "Encontro I · Goblin");
            heroImage = view.Actor(HeroPoses[3], .28f, .13f, ApparentHeight(.38f, HeroPoses[3], run.hero, 4f));
            var opponentId = skeleton ? CombatantId.Skeleton : knight ? CombatantId.Knight : werewolf ? CombatantId.Werewolf : CombatantId.Goblin;
            var opponentSprite = skeleton ? content.skeletonPose : knight ? content.knightPose : werewolf ? content.werewolfPose : content.goblinPoses[2];
            opponentImage = view.Actor(opponentSprite, .69f, skeleton ? .32f : .43f,
                ApparentHeight(.38f, opponentSprite, opponentId, 5.5f));
            var state = battle.State;
            heroHealth = view.Health(heroImage, Hero.DisplayName, state.Hero.CurrentHealth, state.Hero.Stats.MaximumHealth);
            opponentHealth = view.Health(opponentImage, skeleton ? "Esqueleto" : knight ? "Cavaleiro" : werewolf ? "Lobisomem" : "Goblin", state.Opponent.CurrentHealth, state.Opponent.Stats.MaximumHealth);
            view.Text("Sua vez · rodada " + state.Round, $"{(run.hero == CombatantId.Mage ? "Reversões: " + state.RevertCharges + ". " : "")}" +
                (run.equipment ? run.hero == CombatantId.Mage ? $"Manto: {(knight || werewolf ? 5 : skeleton ? 3 : 1)} de dano por ação." : $"Espada: {(knight || werewolf ? 14 : skeleton ? 10 : 7)}% de crítico." : "Escolha sua ação."));
            var abilities = Hero.Abilities;
            var visible = abilities.Where(x => battle.AvailableActions.Contains(x.Id)).ToArray();
            for (var i = 0; i < visible.Length; i++)
            { var ability = visible[i]; view.Choice(ability.DisplayName, () => SubmitBattle(ability.Id), i, visible.Length); }
        }

        private static float ApparentHeight(float warriorHeight, Sprite sprite, CombatantId id, float distance)
        {
            // Body pixels exclude transparent margins and weapons; depths are provisional camera calibration.
            var meters = id == CombatantId.Warrior ? 1.8f : id == CombatantId.Mage ? 1.7f :
                id == CombatantId.Goblin ? .8f : 1.5f;
            var bodyPixels = id == CombatantId.Warrior || id == CombatantId.Skeleton ? 46f :
                id == CombatantId.Mage ? 42f : 30f;
            return ApparentHeight(warriorHeight, sprite, meters, bodyPixels, distance);
        }

        private static float ApparentHeight(float warriorHeight, Sprite sprite, float meters, float bodyPixels, float distance) =>
            warriorHeight * (46f / 48f) * (meters / 1.8f) * (4f / distance) * sprite.rect.height / bodyPixels;

        public void SubmitBattle(AbilityId action)
        {
            if (busy || battle == null || (run.page != StoryPage.Battle && run.page != StoryPage.SkeletonBattle &&
                run.page != StoryPage.KnightBattle && run.page != StoryPage.WerewolfBattle)) return;
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
            var knight = run.page == StoryPage.KnightBattle;
            var werewolf = run.page == StoryPage.WerewolfBattle;
            if (battle.State.Phase == BattlePhase.Victory)
            {
                view.PlaySound("Defeat");
                if (skeleton)
                {
                    var hero = battle.State.Hero; run.skeletonDefeated = true; run.skeletonHealth = hero.CurrentHealth;
                    run.skeletonScrolls = hero.HopeScrolls; run.skeletonHorns = hero.GuardianHorns;
                    run.skeletonGuardianArmed = hero.IsGuardianArmed; Go(StoryPage.SkeletonLetter);
                }
                else if (knight || werewolf) { StoryEncounter.RecordLaterVictory(run, battle); Save(); Render(); }
                else { run.victoryHealth = battle.State.Hero.CurrentHealth; Go(StoryPage.AfterBattle); }
            }
            else if (battle.State.Phase == BattlePhase.Defeat)
            {
                Draw(skeleton ? content.skeletonGraveyard : knight ? content.courtyard : werewolf ? content.hall : content.swamp, "Derrota"); view.Text("Tentar novamente", "O estado anterior à batalha foi preservado. Você não precisa repetir a exploração.");
                view.Choice("Repetir batalha", skeleton ? (Action)StartSkeletonBattle : knight ? (Action)StartKnightBattle : werewolf ? (Action)StartWerewolfBattle : StartBattle, 0, 2); view.Choice("Menu", Home, 1, 2, StoryButtonStyle.Secondary);
            }
            else BattleScreen();
        }
    }
}
