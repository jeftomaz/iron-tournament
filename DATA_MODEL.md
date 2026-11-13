# Modelo de dados

Não há banco de dados no escopo inicial. Este arquivo registra dados de jogo e persistência local.

## Definições estáticas

Definições são `ScriptableObject` em `Content`; o núcleo recebe valores de runtime sem referências a `UnityEngine`.

| Tipo | Responsabilidade |
|---|---|
| `CombatantDefinition` | Identidade estável, lado, atributos-base, capacidades e sprite opcional |
| `AbilityDefinition` | Identidade estável, nome de exibição, alvo e consumo de turno; a regra do efeito fica no `Core` |
| `ItemDefinition` | Peso de drop e modificadores aplicados |
| `EncounterDefinition` | Oponente, pool de drops e variação fixa de `±15%` dos atributos inimigos |
| `CampaignDefinition` | Ordem fixa dos encontros e condição de conclusão |

O adaptador `ContentMapper` cria `CombatantConfiguration`, `AbilityConfiguration`, `ItemConfiguration`, `EncounterConfiguration` e `CampaignConfiguration` imutáveis no `Core`. `Content` referencia `Core`; o sentido inverso é proibido.

### Valores canônicos

| Combatente | HP | ATK | DEF | Habilidades / regra |
|---|---:|---:|---:|---|
| Guerreiro (equipado) | 150 | 25 | 20 | `BasicAttack`, `Guard`, `UseHopeScroll` |
| Mago (equipado) | 120 | 30 | 5 | `BasicAttack`, `RevertTurn`, `RevertBattle`, `ArmGuardian` |
| Goblin | 40 | 15 | 15 | `BasicAttack` |
| Esqueleto | 50 | 20 | 20 | `BasicAttack` |
| Cavaleiro | 100 | 35 | 35 | `BasicAttack` |
| Lobisomem | 120 | 45 | 18 | `BasicAttack` |
| Vampiro | 130 | 55 | 20 | `BasicAttack`; fúria |
| Necromante | 160 | 65 | 22 | `BasicAttack`; fúria |
| Rei Demônio | 200 | 75 | 25 | `BasicAttack`; fúria |

| Item | Peso | Efeito | Exclusivo |
|---|---:|---|---|
| Poção de Cura (`HealingPotion`) | 3 | Cura 40 HP | — |
| Gema de Sangue (`AttackGem`) | 3 | Modificador `Attack` +10 | — |
| Runa do Guardião (`DefenseRune`) | 3 | Modificador `Defense` +5 | — |
| Cristal da Ruína (`PowerCrystal`) | 2 | Modificador `Attack` +15 | — |
| Elixir da Vida (`LifeElixir`) | 1 | Cura total | — |
| Defesa Divina (`DivineDefense`) | 1 | Modificador `Defense` +15 | — |
| Pergaminho Misterioso (`HopeScroll`) | 2 | +1 uso de `UseHopeScroll`: com HP ≤ 30%, cura 50% do máximo e gasta o turno | Guerreiro |
| Manto de Chamas (`FlameCloak`) | 2 | 5 de dano após a ação do herói e após o turno inimigo; não volta a ser oferecido a quem já o tem | Mago |
| Chifre da Irmandade (`BrotherhoodHorn`) | 2 | +1 uso de `ArmGuardian` (gasta o turno): o próximo golpe letal é interceptado com 30 + ATK do Mago; se vencer, HP sobe a 30%, senão fica em 1 | Mago |

No protótipo todos os encontros usam o mesmo pool; cada `EncounterDefinition` deve repetir o pool completo, e o `Core` filtra os exclusivos pela classe.

### Assets da cena de batalha

Local: `Assets/IronTournament/Content/`. `Campaign.asset` é a composição canônica; a cena `Battle` ainda usa o encontro isolado selecionado até a integração da campanha no bootstrap.

| Asset | HP | ATK | DEF | Habilidades |
|---|---:|---:|---:|---|
| `Warrior.asset` | 150 | 25 | 20 | `BasicAttack.asset`, `Guard.asset`, `UseHopeScroll.asset` |
| `Mage.asset` | 120 | 30 | 5 | `MagicAttack.asset`, `RevertTurn.asset`, `RevertBattle.asset`, `ArmGuardian.asset` |
| `Goblin.asset` | 40 | 15 | 15 | `BasicAttack.asset` |
| `Skeleton.asset` | 50 | 20 | 20 | `BasicAttack.asset` |
| `Knight.asset` | 100 | 35 | 35 | `BasicAttack.asset` |
| `Werewolf.asset` | 120 | 45 | 18 | `BasicAttack.asset` |
| `Vampire.asset` | 130 | 55 | 20 | `BasicAttack.asset` |
| `Necromancer.asset` | 160 | 65 | 22 | `BasicAttack.asset` |
| `DemonKing.asset` | 200 | 75 | 25 | `BasicAttack.asset` |

- `MagicAttack.asset` mantém o ID `BasicAttack`, mira o oponente e consome turno; o nome de exibição é `Ataque mágico`.
- `RevertTurn.asset` e `RevertBattle.asset` miram o próprio Mago e não consomem turno. Recursos de itens do Mago seguem a etapa de campanha.
- `Campaign.asset` ordena `GoblinEncounter`, `SkeletonEncounter`, `KnightEncounter`, `WerewolfEncounter`, `VampireEncounter`, `NecromancerEncounter` e `DemonKingEncounter`.
- Cada encontro declara variação de ±15% e o pool completo de nove itens. O núcleo filtra exclusivos pela classe e não oferece saque após o Rei Demônio; fúria continua sendo regra do núcleo.
- Ambas as ações consomem turno; Atacar mira o oponente e Defender mira o próprio combatente.
- Experiência por vitória: Goblin +2/+1, Esqueleto +3/+1, Cavaleiro +3/+2, Lobisomem +4/+2, Vampiro +4/+3 e Necromante +5/+3 (ATQ/DEF). Rei Demônio e modo inimigo não concedem esses ganhos.
- A seleção isolada atual da cena não consome drops; saque e avanço dependem da integração de `CampaignRun`.
- O registro de sprites direcionais e cenários fica na cena `Battle`, ligado a cada `CombatantDefinition`; não duplica definições de combate nem regras do `Core`.

## Estado de runtime

| Tipo | Responsabilidade |
|---|---|
| `CombatantState` | HP, atributos efetivos, efeitos e recursos atuais; hoje expõe HP, atributos, defesa temporária (`GuardBonus`) fúria (`CanRage`, `HasRaged`) e recursos de itens (pergaminhos, chifres, guardião armado, manto) |
| `BattleState` | Participantes, fase, turno, flags e histórico do encontro; hoje expõe herói, oponente, fase (`PlayerTurn`, `Victory`, `Defeat`), rodada e cargas de reversão |
| `CampaignRun` | Campanha ou modo inimigo (`Mode`): sequência de oponentes, herói persistente, encontro atual, oferta de drop, itens obtidos, fase (`Battle`, `DropChoice`, `Completed`, `Failed`) e `FinalSnapshot` ao concluir a campanha |
| `ActionResult` | Eventos produzidos por uma ação aceita ou motivo de rejeição (`BattleOver`, `UnavailableAction`) |
| Snapshot (`BattleState.Clone`, interno ao `Core`) | Cópia íntegra restaurável do estado do encontro, sem o estado do gerador aleatório |
| `ProgressData` | Imutável: um `HeroSnapshot` por classe concluída; o modo inimigo libera com Guerreiro e Mago concluídos; `WithCompletedCampaign` substitui o snapshot da classe |
| `HeroSnapshot` | Classe (Guerreiro ou Mago), nome e atributos finais com melhorias; cada atributo limitado a 9999 |
| `ExperienceGain` | Ganhos imutáveis de ATQ e DEF; `CampaignRun.LastExperience` expõe o último ganho aplicado |

## Integridade

- Definições são imutáveis durante a partida.
- IDs de combatente, habilidade e item são enums fechados; `None` e valores fora da enumeração são inválidos.
- O mapeamento falha antes de alcançar o núcleo se encontrar referência ausente, atributo inválido, duplicidade de habilidade/drop/modificador, peso não positivo ou texto de exibição inseguro.
- Construtores das configurações do núcleo repetem as validações estruturais para impedir dados inválidos por chamadas que não passam pelo adaptador.
- Nomes de exibição são limitados a 48 caracteres e não aceitam controles nem tags rich text.
- HP, ATK e DEF efetivos do inimigo são sorteados em ±15% dos valores-base uma vez na criação do encontro, com arredondamento e limite mínimo válido.
- Reversão restaura os atributos efetivos já sorteados; nunca recria nem sorteia novamente o inimigo.
- HP e atributos carregados são limitados a faixas válidas.
- Save local será versionado, validado e gravado de forma atômica.
- O histórico de reversão começa e termina dentro de cada encontro.
- Antes de cada ação do Mago que gasta turno, o núcleo guarda o `turnStartSnapshot`; ao iniciar o encontro, guarda o `encounterStartSnapshot`.
- `Reverter Turno` restaura o `turnStartSnapshot` anterior à última ação do Mago. Exemplo: ataque A, resposta B, reversão; o estado volta para antes de A e o Mago escolhe novamente.
- `Reverter Batalha` restaura o `encounterStartSnapshot` e também devolve o controle ao Mago.
- A restauração inclui participantes, HP, atributos efetivos, efeitos, recursos consumíveis, fase, flags e histórico. Depois dela, a carga compartilhada é marcada como consumida.
- O estado do gerador aleatório não pertence ao snapshot; resultados futuros são sorteados novamente.
- A reversão não pode ser acionada após a derrota do Mago.
- Com o Manto de Chamas equipado, cada turno que consome ação, do Mago ou do inimigo, aplica 5 de dano direto ao inimigo; ações sem consumo não aplicam o efeito.
- HP atual, itens e melhorias do jogador continuam entre encontros; ações reversíveis anteriores não.
- Apenas o snapshot final da campanha, já com as melhorias obtidas, pode ser persistido para outros modos.
- A progressão da campanha não usa sorteio para escolher ou ordenar inimigos.
- O `Core` define quais habilidades gastam turno: todas, exceto `RevertTurn` e `RevertBattle`; o `consumesTurn` dos assets deve coincidir com essa regra.
- `CampaignConfiguration` e o `ContentValidator` exigem exatamente os sete encontros na ordem canônica; campanhas truncadas, estendidas ou reordenadas são rejeitadas.

## Checkpoint da prova narrativa (versão 1)

- `StoryRun`: versão, seed, protagonista, página/índice do diálogo, permutação das cinco casas, máscara de casas visitadas, casa/quarto aberto, grito pendente, recusa final, comida/coleta/entrega, equipamento, Cristal, recompensa e atributos após saque.
- O checkpoint mantém os seis túmulos, máscara de visitas, limite de aparição, número do túmulo fixo quando há Cristal, resultado do Esqueleto, recursos e saque. Saves anteriores com Cristal reservam o túmulo vazio ao carregar, mesmo se já examinado; após o encontro, preservam o estado já alcançado.
- As cartas de Lucy e Samuel começam em túmulos adjacentes; sem Cristal, o Esqueleto substitui integralmente o conteúdo do túmulo em que surge, inclusive carta ou objeto. Seed, encontro e oferta permanecem estáveis ao carregar ou repetir.
- `StoryEncounter`: inimigo e oferta reproduzíveis pela seed, equipamentos da vila exclusivos por classe, Manto com 1 de dano no Goblin, Espada com 7% de crítico; saques excluem Manto, agora encontrado na vila. A campanha clássica conserva seu pool e Manto de 5 de dano.
- No Esqueleto, o herói mantém HP, experiência, recompensa e recursos do Goblin; o Manto causa 3 de dano e a Espada oferece 10% de crítico.
- Checkpoints preservam flags de experiência e aceitam atributos atuais ou legados somente quando reproduzem o saque salvo; valores adulterados continuam rejeitados. Em versões indistinguíveis, usar o HP máximo atual sem curar o HP salvo.
- `CombatantState` inclui dano do Manto e chance de crítico, copiados nas reversões; sem equipamento narrativo mantém os valores antigos.
- `StoryContent` referencia arte de abertura e `story-script.json`, com textos distintos para casas e quartos. Os PNGs originais permanecem intactos; sprites derivados usam limites do alpha.
- `StorySaveStore`: duas entradas PlayerPrefs alternadas com revisão e SHA-256 do JSON; seleciona a revisão válida mais recente e ignora versão, enums, máscaras, permutação, saques fora da oferta ou combinações inválidas. Checksum detecta corrupção, não impede adulteração pelo próprio jogador.
- Marcadores de segunda campanha e origem são validados no modelo. Ainda não há gravação de conclusão da campanha completa nessa prova; não confundir Finished do trecho com Completed de CampaignRun.
