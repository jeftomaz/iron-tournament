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
| Guerreiro (equipado) | 120 | 30 | 23 | `BasicAttack`, `Guard`, `UseHopeScroll` |
| Mago (equipado) | 110 | 30 | 5 | `BasicAttack`, `RevertTurn`, `RevertBattle`, `ArmGuardian` |
| Goblin | 45 | 15 | 3 | `BasicAttack` |
| Esqueleto | 62 | 20 | 5 | `BasicAttack` |
| Cavaleiro | 80 | 26 | 8 | `BasicAttack` |
| Lobisomem | 95 | 30 | 10 | `BasicAttack` |
| Vampiro | 110 | 34 | 12 | `BasicAttack`; fúria |
| Necromante | 125 | 38 | 14 | `BasicAttack`; fúria |
| Rei Demônio | 145 | 44 | 16 | `BasicAttack`; fúria |

| Item | Peso | Efeito | Exclusivo |
|---|---:|---|---|
| Poção de Cura (`HealingPotion`) | 3 | Cura 40 HP | — |
| Gema de Sangue (`AttackGem`) | 3 | Modificador `Attack` +10 | — |
| Runa do Guardião (`DefenseRune`) | 3 | Modificador `Defense` +5 | — |
| Cristal da Ruína (`PowerCrystal`) | 2 | Modificador `Attack` +15 | — |
| Elixir da Vida (`LifeElixir`) | 1 | Cura total | — |
| Defesa Divina (`DivineDefense`) | 1 | Modificador `Defense` +15 | — |
| Pergaminho Misterioso (`HopeScroll`) | 2 | +1 uso de `UseHopeScroll`: com HP ≤ 30%, cura 50% do máximo e gasta o turno | Guerreiro |
| Manto de Chamas (`FlameCloak`) | 2 | 5 de dano após a ação do herói e após o turno inimigo | Mago |
| Chifre da Irmandade (`BrotherhoodHorn`) | 2 | +1 uso de `ArmGuardian` (gasta o turno): o próximo golpe letal é interceptado com 30 + ATK do Mago; se vencer, HP sobe a 30%, senão fica em 1 | Mago |

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
- HP atual, itens e melhorias do jogador continuam entre encontros; ações reversíveis anteriores não.
- Apenas o snapshot final da campanha, já com as melhorias obtidas, pode ser persistido para outros modos.
- A progressão da campanha não usa sorteio para escolher ou ordenar inimigos.
