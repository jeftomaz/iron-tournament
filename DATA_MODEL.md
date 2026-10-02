# Modelo de dados

Não há banco de dados no escopo inicial. Este arquivo registra dados de jogo e persistência local.

## Definições estáticas

Definições são `ScriptableObject` em `Content`; o núcleo recebe valores de runtime sem referências a `UnityEngine`.

| Tipo | Responsabilidade |
|---|---|
| `CombatantDefinition` | Identidade, atributos-base, sprite e capacidades |
| `AbilityDefinition` | Custo, alvo, disponibilidade e efeitos |
| `ItemDefinition` | Peso de drop e modificadores aplicados |
| `EncounterDefinition` | Oponentes, drops e regras do encontro |
| `CampaignDefinition` | Ordem fixa dos encontros e condição de conclusão |

## Estado de runtime

| Tipo | Responsabilidade |
|---|---|
| `CombatantState` | HP, atributos efetivos, efeitos e recursos atuais |
| `BattleState` | Participantes, fase, turno, flags e histórico do encontro |
| `BattleSnapshot` | Cópia íntegra restaurável do estado do encontro, sem o estado do gerador aleatório |
| `ProgressData` | Campanhas concluídas, desbloqueios e snapshots finais |

## Integridade

- Definições são imutáveis durante a partida.
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
