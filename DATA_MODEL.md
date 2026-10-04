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
| Guerreiro (equipado) | 120 | 30 | 23 | `BasicAttack`, `Guard` |
| Mago (equipado) | 110 | 30 | 5 | `BasicAttack`, `RevertTurn`, `RevertBattle` |
| Goblin | 45 | 15 | 3 | `BasicAttack` |
| Esqueleto | 62 | 20 | 5 | `BasicAttack` |
| Cavaleiro | 80 | 26 | 8 | `BasicAttack` |
| Lobisomem | 95 | 30 | 10 | `BasicAttack` |
| Vampiro | 110 | 34 | 12 | `BasicAttack`; fúria |
| Necromante | 125 | 38 | 14 | `BasicAttack`; fúria |
| Rei Demônio | 145 | 44 | 16 | `BasicAttack`; fúria |

## Estado de runtime

| Tipo | Responsabilidade |
|---|---|
| `CombatantState` | HP, atributos efetivos, efeitos e recursos atuais; hoje expõe HP, atributos, defesa temporária (`GuardBonus`) e fúria (`CanRage`, `HasRaged`) |
| `BattleState` | Participantes, fase, turno, flags e histórico do encontro; hoje expõe herói, oponente, fase (`PlayerTurn`, `Victory`, `Defeat`), rodada e cargas de reversão |
| `CampaignRun` | Sequência fixa de encontros, herói persistente, encontro atual e fase (`Battle`, `Completed`, `Failed`) |
| `ActionResult` | Eventos produzidos por uma ação aceita ou motivo de rejeição (`BattleOver`, `UnavailableAction`) |
| Snapshot (`BattleState.Clone`, interno ao `Core`) | Cópia íntegra restaurável do estado do encontro, sem o estado do gerador aleatório |
| `ProgressData` | Campanhas concluídas, desbloqueios e snapshots finais |

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
- `CampaignConfiguration` e o `ContentValidator` exigem exatamente os sete encontros na ordem canônica; campanhas truncadas, estendidas ou reordenadas são rejeitadas.
