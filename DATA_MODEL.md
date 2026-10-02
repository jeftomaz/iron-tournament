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
| `BattleSnapshot` | Cópia íntegra restaurável do estado do encontro, exceto a sequência aleatória |
| `ProgressData` | Campanhas concluídas, desbloqueios e snapshots finais |

## Integridade

- Definições são imutáveis durante a partida.
- HP e atributos carregados são limitados a faixas válidas.
- Save local será versionado, validado e gravado de forma atômica.
- O histórico de reversão começa e termina dentro de cada encontro.
- Reverter restaura participantes, HP, atributos efetivos, efeitos, recursos, turno, flags e histórico do encontro; a sequência aleatória não retrocede.
- HP atual, itens e melhorias do jogador continuam entre encontros; ações reversíveis anteriores não.
- Apenas o snapshot final da campanha pode ser persistido para outros modos.
- A progressão da campanha não usa sorteio para escolher ou ordenar inimigos.
