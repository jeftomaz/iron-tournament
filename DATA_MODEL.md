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

## Assets da vertical Guerreiro/Goblin

Local: `Assets/IronTournament/Content/`. Valores conferidos com a referência Java e os testes aprovados do núcleo; Guerreiro já inclui equipamento inicial, sem reaplicar bônus no bootstrap.

| Asset | HP | ATK | DEF | Habilidades |
|---|---:|---:|---:|---|
| `Warrior.asset` | 120 | 30 | 23 | `BasicAttack.asset`, `Guard.asset` |
| `Goblin.asset` | 45 | 15 | 3 | `BasicAttack.asset` |

- `GoblinEncounter.asset` referencia Goblin e declara variação de ±15%; o bootstrap deve sortear os atributos efetivos na criação do encontro.
- Ambas as ações consomem turno; Atacar mira o oponente e Defender mira o próprio combatente.
- Pool de drops vazio nesta vertical; campanha e saque permanecem nas fases posteriores.

## Estado de runtime

| Tipo | Responsabilidade |
|---|---|
| `CombatantState` | HP, atributos efetivos, efeitos e recursos atuais; hoje expõe HP, atributos e defesa temporária (`GuardBonus`) |
| `BattleState` | Participantes, fase, turno, flags e histórico do encontro; hoje expõe herói, oponente, fase (`PlayerTurn`, `Victory`, `Defeat`) e rodada |
| `ActionResult` | Eventos produzidos por uma ação aceita ou motivo de rejeição (`BattleOver`, `UnavailableAction`) |
| `BattleSnapshot` | Cópia íntegra restaurável do estado do encontro, sem o estado do gerador aleatório |
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
- HP atual, itens e melhorias do jogador continuam entre encontros; ações reversíveis anteriores não.
- Apenas o snapshot final da campanha, já com as melhorias obtidas, pode ser persistido para outros modos.
- A progressão da campanha não usa sorteio para escolher ou ordenar inimigos.
