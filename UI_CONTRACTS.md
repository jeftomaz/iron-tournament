# Contratos de UI

| Componente | Responsabilidade | Não deve fazer |
|---|---|---|
| `BattleView` | Exibir combatentes e cenário | Calcular ou aplicar dano |
| `BattleHud` | Exibir HP, estados e mensagens | Alterar o estado da batalha |
| `ActionMenu` | Mostrar ações disponíveis e enviar a escolha | Decidir disponibilidade de ações |
| `BattleEventPlayer` | Apresentar eventos em sequência | Criar resultados de combate |
| `BattlePresenter` | Enviar escolhas ao `IBattle` e sincronizar a apresentação | Criar configurações ou implementar regras |

## Referências de tela

| Plataforma | Aspect ratio | Área de referência | Prioridade |
|---|:---:|:---:|---|
| Navegador mobile | `9:16` | `360 × 640` | Inicial |
| Navegador desktop | `16:9` | `1280 × 720` | Posterior |

## Regras vinculantes

- Cena-base: `Assets/Scenes/Battle.unity`; `BattleView` alterna composição por orientação e recorta o cenário mantendo sua proporção.
- Goblin, Esqueleto, Lobisomem e Vampiro são variantes da mesma cena e dos mesmos componentes. `BattleView.SelectEncounter` escolhe as referências visuais antes do combate e expõe `SelectedEncounter` para o bootstrap; rejeita inimigos não configurados e mudanças durante combate. O Inspector permite prévia pelo campo `Selected Opponent`; Goblin permanece o padrão.
- A cena `Battle` é a entrada habilitada no build desta vertical; a inicialização do combate continua sendo fornecida pelo bootstrap.
- Introdução: Guerreiro `east` e inimigo `west`, frente a frente. Ao clicar em `Iniciar combate`, usar Guerreiro `north-east` em primeiro plano e inimigo `south-west` acima e à direita; ocultar o botão. A transição atual é visual, aguardando integração com o núcleo.
- `Assets/IronTournament/Prefabs/BattleCombatant.prefab` é compartilhado por ambos os lados; variar o sprite na instância, sem duplicar o prefab.
- Assets usados na cena-base: filtro point, sem mipmaps/compressão e 48 pixels por unidade; personagens usam múltiplos inteiros em pixels de tela.
- A escala dos combatentes respeita a altura disponível acima do chão; sprites maiores devem caber inteiros na arena nas duas orientações.
- `BattleHud.ShowHealth` recebe lado, HP atual e máximo válidos; só apresenta valores. Sem estado recebido, exibe `HP — / —`. `SetStatus` apresenta mensagens literais de até 80 caracteres, sem controles nem interpretação rich text.
- `ActionMenu.SetAvailableActions` recebe ações disponíveis e bloqueio de entrada; apresenta `Atacar`/`Defender` nesta vertical. `ActionSelected` envia o `AbilityId` e bloqueia novos cliques até a próxima atualização; o controlador ainda deve validar a escolha no núcleo. Sem integração, ambos os botões ficam desabilitados.
- `BattleEventPlayer.PlayEvents` recebe os `BattleEvent` aprovados e o `BattleState` do encontro: mapeia IDs pelos participantes, apresenta ataque/defesa, dano com crítico/penetração/reflexão e encerramento. O HP muda na interface somente quando o respectivo evento é apresentado, usando `RemainingHealth` e o máximo recebido; não calcula resultados nem modifica o núcleo.
- O lote é validado integralmente antes da reprodução: rejeitar participantes externos, HP acima do máximo, eventos nulos, habilidades sem apresentação e encerramento fora do fim. `Play` reutiliza as mesmas rotinas de efeitos; a lista recebida é copiada antes de iniciar.
- A sequência bloqueia o menu por `SetPresentationBlocked`; atualizações de disponibilidade durante a animação só aparecem após o desbloqueio, preservando o bloqueio recebido do controlador. `Cancel` e desativação restauram cores/posições, ocultam o feedback e descartam a sequência pendente.
- Cancelar também descarta atualizações de HP ainda não apresentadas; o controlador deve redesenhar o estado vigente antes de reabrir as ações.
- `BattleEventPlayer.End` recebe o `BattleEndedEvent` aprovado do núcleo e encerra a sequência após os efeitos anteriores; reutiliza a mensagem do HUD para vitória/derrota, oculta as ações e escurece o vencido. Não deduz resultado pelo HP, não altera a saúde e rejeita novas sequências após o encerramento. Cancelar antes do evento descarta também o resultado pendente.
- A UI consome estado somente para exibição.
- `BattlePresenter.Initialize(IBattle)` recebe uma batalha ativa do bootstrap de Jeferson, uma vez por cena. `BattleView.CombatStarted` inicia a exibição; escolhas passam por `Submit`, ficam bloqueadas durante os eventos e são reabertas com `AvailableActions`. Ao retomar uma apresentação interrompida, redesenhar o estado vigente do núcleo. Sem inicialização, as ações permanecem desabilitadas.
- `Presentation` pode referenciar `Core` e `Content`; o núcleo não referencia `UnityEngine` nem componentes de interface.
- Toda escolha passa pela validação do núcleo.
- Entrada fica bloqueada enquanto eventos do turno estão sendo apresentados.
- Ao concluir `Reverter Turno`, a apresentação descarta eventos visuais pendentes da rodada desfeita, redesenha o snapshot restaurado e reabre imediatamente o `ActionMenu` do Mago.
- `Reverter Batalha` segue o mesmo fluxo visual, usando o estado de entrada do encontro.
- Cada orientação usa composição própria; preservar a proporção com barras, nunca deformar a interface.
- Sprites usam filtro point e escala inteira sempre que possível.
- Textos fornecidos pelo jogador terão limite e tratamento de caracteres de controle/rich text.

## Validação visual

- PR de interface inclui captura ou vídeo curto do fluxo alterado.
- Revisão manual usa ambas as resoluções de referência e verifica legibilidade, navegação e estados extremos.
- Testes PlayMode cobrem transições e interações; a aprovação visual continua humana.
- Quando disponível, a pipeline publica uma build como artefato do PR para validação da integração.

## Abertura local

- Abrir a raiz do repositório no Unity Hub com a versão definida em `PROJECT.md`, carregar a cena alterada e iniciar Play Mode.
- Acompanhar o resultado na Game View; Scene e Inspector mostram hierarquia e estado de runtime.
- Em tarefas de UI, o agente abre o projeto e inicia Play Mode quando tiver acesso ao Unity Editor; caso contrário, informa o procedimento ao desenvolvedor.
