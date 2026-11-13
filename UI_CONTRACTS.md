# Contratos de UI

| Componente | Responsabilidade | Não deve fazer |
|---|---|---|
| `BattleView` | Exibir combatentes, cenário e detalhes da oferta de saque | Calcular ou aplicar dano |
| `BattleCombatantAnimator` | Reproduzir sequências de sprites e restaurar o estado visual estável | Interpretar eventos ou decidir resultados de combate |
| `BattleHud` | Exibir HP, estados e mensagens | Alterar o estado da batalha |
| `ActionMenu` | Mostrar ações ou saques disponíveis e enviar a escolha | Decidir disponibilidade ou efeitos |
| `BattleEventPlayer` | Apresentar eventos em sequência | Criar resultados de combate |
| `BattlePresenter` | Enviar escolhas ao `IBattle`, sincronizar a apresentação e informar o encerramento | Criar configurações ou implementar regras |
| `BattleBootstrap` | Mapear conteúdo, iniciar a campanha e fornecer sua batalha ativa | Alterar estado de combate ou regras |
| `MainMenuController` | Exibir tópicos do menu e carregar `Battle` pelo botão de início | Definir regras ou estado da campanha |

## Referências de tela

| Plataforma | Aspect ratio | Área de referência | Prioridade |
|---|:---:|:---:|---|
| Navegador mobile | `9:16` | `360 × 640` | Inicial |
| Navegador desktop | `16:9` | `1280 × 720` | Posterior |

## Regras vinculantes

- Direção aprovada em 2026-11-01 para as novas telas: gerar cenários quadrados (`1:1`) uma única vez para desktop e mobile. Manter a imagem inteira, sem esticar nem recortar para preencher a tela; os pontos clicáveis acompanham suas coordenadas na imagem.
- Desktop: cenário quadrado com menus e demais conteúdos nas áreas laterais disponíveis. Mobile em retrato: mesmo cenário quadrado, com menus e demais conteúdos acima e/ou abaixo. A interface ocupa o espaço excedente sem encobrir pontos de interação; o layout quadrado ainda precisa ser implementado nas telas existentes.

- Entrada da demonstração clássica: `Assets/Scenes/MainMenu.unity`, com os tópicos consultáveis `Regras`, `Objetivo` e `Desafios`, cabeçalho e texto informativo, e o botão `Iniciar campanha`; o tópico ativo só altera esse briefing e a cor do rótulo. A escala deve preservar todos os controles em telas largas; ele carrega `Battle` sem carregar estado de jogo.
- Cena-base de combate: `Assets/Scenes/Battle.unity`; o layout legado de `BattleView` alterna composição por orientação e recorta o cenário mantendo sua proporção. Sua substituição deve seguir a direção de cenários quadrados acima.
- `BattleView` mantém um registro visual por combatente configurado: definição de conteúdo, encontro quando for adversário, quatro sprites (`east`, `north-east`, `west`, `south-west`), cenário e legenda. `SelectPlayer` e `SelectEncounter` selecionam os dois participantes antes do combate, expõem `SelectedPlayer` e `SelectedEncounter` ao bootstrap e rejeitam referências inválidas ou mudanças durante combate. Guerreiro/Goblin permanecem o padrão.
- A cena `Battle` é a entrada habilitada no build desta vertical; a inicialização do combate continua sendo fornecida pelo bootstrap.
- Introdução: controlado `east` e adversário `west`, frente a frente, com o cenário do adversário. Ao clicar em `Iniciar combate`, usar controlado `north-east` em primeiro plano e adversário `south-west` acima e à direita; ocultar o botão. A transição é a mesma para Guerreiro, Mago e modo inimigo.
- `Assets/IronTournament/Prefabs/BattleCombatant.prefab` é compartilhado por ambos os lados; variar o sprite na instância, sem duplicar o prefab.
- Cada instância do prefab usa `BattleCombatantAnimator`. Perfis opcionais associam `Idle`, `Attack`, `Hurt`, `Wounded` e `Down` a sequências de sprites; um estado ausente conserva o idle ou sprite estático configurado. A orientação é definida pelo perfil visual, não pelo estado semântico.
- Assets usados na cena-base: filtro point, sem mipmaps/compressão e 48 pixels por unidade; personagens usam múltiplos inteiros em pixels de tela.
- Até a PR dedicada após a limpeza das branches, não alterar escala, proporção ou perspectiva dos combatentes. Essa PR definirá proporções-base por personagem e o fator de profundidade do inimigo, preservando sprites sem deformação.
- `BattleHud.ShowHealth` recebe lado, HP atual e máximo válidos; só apresenta valores. Sem estado recebido, exibe `HP — / —`. `SetStatus` apresenta mensagens literais de até 80 caracteres, sem controles nem interpretação rich text.
- `BattlePresenter.Initialize` configura `ActionMenu` pelas `AbilityConfiguration` do herói. Ao encerrar uma batalha, `EncounterConcluded` informa o resultado depois da reprodução; a próxima `IBattle` ativa entra somente por `BeginNextEncounter`. O menu cria botões a partir dos dois modelos existentes, usa os nomes do conteúdo e mantém `SetAvailableActions` como única fonte da disponibilidade das ações. `ActionSelected` envia o `AbilityId`; `ConfigureDrops` exibe a oferta recebida e `DropSelected` envia apenas o `ItemId` escolhido.
- `BattleEventPlayer.PlayEvents` recebe os `BattleEvent` aprovados e o `BattleState` do encontro: mapeia IDs pelos participantes, apresenta ataque mágico, defesa, fúria, cura, guardião, dano com crítico/penetração/reflexão e encerramento. O HP muda na interface somente quando o respectivo evento é apresentado, usando o estado recebido; não calcula resultados nem modifica o núcleo.
- O lote é validado integralmente antes da reprodução: rejeitar participantes externos, HP acima do máximo, eventos nulos e encerramento fora do fim. `Play` reutiliza as mesmas rotinas de efeitos; a lista recebida é copiada antes de iniciar.
- A sequência bloqueia o menu por `SetPresentationBlocked`; atualizações de disponibilidade durante a animação só aparecem após o desbloqueio, preservando o bloqueio recebido do controlador. `Cancel` e desativação restauram cores/posições, ocultam o feedback e descartam a sequência pendente.
- Cancelar também descarta atualizações de HP ainda não apresentadas; o controlador deve redesenhar o estado vigente antes de reabrir as ações.
- `BattleEventPlayer.End` recebe o `BattleEndedEvent` aprovado do núcleo e encerra a sequência após os efeitos anteriores; reutiliza a mensagem do HUD para vitória/derrota, oculta as ações e escurece o vencido. Não deduz resultado pelo HP, não altera a saúde e rejeita novas sequências após o encerramento. Cancelar antes do evento descarta também o resultado pendente.
- A UI consome estado somente para exibição.
- Na recompensa de batalha, apresentar “Experiência de batalha” com ganhos de ATQ e DEF junto à oferta de saque; na prova narrativa, a escolha confirma ambos.
- Ao receber `BattleView.CombatStarted`, `BattleBootstrap` mapeia o herói selecionado e a configuração `Campaign`, cria o `CampaignRun` e entrega sua batalha ativa ao `BattlePresenter`. Cada vitória é concluída pelo núcleo: o bootstrap mostra a oferta recebida, envia a escolha ao `CampaignRun` e troca a cena para o próximo encontro; derrota e conclusão final permanecem terminais. Escolhas passam por `Submit`, ficam bloqueadas durante os eventos e são reabertas com `AvailableActions`. Ao retomar uma apresentação interrompida, redesenhar o estado vigente do núcleo. Sem inicialização, as ações permanecem desabilitadas.
- Durante `DropChoice`, `BattleView` apresenta nome e efeito de cada item em `DropDetails`, numa área exclusiva acima das ações; esse texto não decide nem aplica efeitos e é ocultado ao iniciar o próximo encontro ou encerrar a campanha.
- `BattleBootstrap` usa as seleções validadas de `BattleView` e `ContentMapper`; a variação de atributos, fúria, drops e efeitos dos itens continuam no `Core`.
- `Presentation` pode referenciar `Core` e `Content`; o núcleo não referencia `UnityEngine` nem componentes de interface.
- Toda escolha passa pela validação do núcleo.
- Entrada fica bloqueada enquanto eventos do turno estão sendo apresentados.
- Ao concluir `Reverter Turno` ou `Reverter Batalha`, a apresentação descarta eventos visuais pendentes, redesenha o snapshot restaurado e reabre imediatamente o `ActionMenu` do Mago, com a carga atual recebida do núcleo.
- Cada orientação usa composição própria; preservar a proporção com barras, nunca deformar a interface.
- Sprites usam filtro point e escala inteira sempre que possível.
- Textos fornecidos pelo jogador terão limite e tratamento de caracteres de controle/rich text.

## Validação visual

- PR de interface inclui captura ou vídeo curto do fluxo alterado.
- Capturas versionadas ficam em `docs/evidence/` e são vinculadas no PR.
- Revisão manual usa ambas as resoluções de referência e verifica legibilidade, navegação e estados extremos.
- Testes PlayMode cobrem transições e interações; a aprovação visual continua humana.
- Quando disponível, a pipeline publica uma build como artefato do PR para validação da integração.

## Abertura local

- Abrir a raiz do repositório no Unity Hub com a versão definida em `PROJECT.md`, carregar a cena alterada e iniciar Play Mode.
- Acompanhar o resultado na Game View; Scene e Inspector mostram hierarquia e estado de runtime.
- Em tarefas de UI, o agente abre o projeto e inicia Play Mode quando tiver acesso ao Unity Editor; caso contrário, informa o procedimento ao desenvolvedor.

## Prova narrativa — StoryProof

- A abertura mostra “Até que o último caia” sem identificação de capítulo no menu inicial.
- Cores de referência e direção visual: `ART_STYLE_GUIDE.md`; paleta editável em `ArtSource/Palettes/IronTournament.gpl`. As famílias orientam a arte sem limitar rigidamente os pixels.
- Usar Deutsch Gothic no nome do jogo, Saltblack Combat em títulos e textos curtos e Pix32 em diálogos, explicações e inventário; caracteres ausentes recorrem à Inter incluída no projeto para preservar acentos no navegador.
- `StoryView` mantém o cenário em um quadrado com ajuste por aspecto, integralmente visível; atores e pontos clicáveis são filhos desse quadrado. Desktop tem painéis laterais; retrato tem informação e ações abaixo do cenário.
- A orientação acompanha as dimensões da área de jogo; mudanças de tamanho redesenham a composição.
- Botões de ação e investigação usam dimensões reduzidas, respeitando a distribuição responsiva.
- Casas visitadas usam marca de revisita; não alteram a contagem ao reabrir. Gritos bloqueiam a abertura de casas até escolher investigar ou continuar; a última recusa fixa o desfecho de morte anterior ao combate.
- Os seis túmulos são pontos clicáveis: com o Cristal, cartas não examinadas recebem azul; o único túmulo com o Esqueleto recebe vermelho. O túmulo do Esqueleto não contém outros objetos; após o combate, os túmulos restantes continuam acessíveis.
- A floresta alagada, floresta fria, clareira, túmulos, mão do Esqueleto e montanha usam as artes locais; pontos clicáveis acompanham os túmulos da imagem.
- Texto digitado: um toque completa a fala; o próximo avança. Descrições longas de ambientes e cartas são paginadas.
- A abertura e a narração do capítulo usam fundo preto e texto centralizado; falas de personagens usam balões próximos aos atores. Menus distinguem ações principais, secundárias e inventário.
- Os indicadores próximos aos combatentes exibem valores e barras de HP; ofertas de saque mostram uma descrição curta de cada item.
- A interface usa sprites CC0 do UI Pack da Kenney; sons da prova usam os efeitos selecionados do RPG Essentials.
- Botões de combate ficam bloqueados durante apresentação dos eventos. `StoryProofController` envia `Submit` ao mesmo núcleo da demonstração clássica; não reimplementa regras.
- Escala da prova: Guerreiro 1,80 m, Mago 1,70 m, Goblin e criança 0,80 m e Esqueleto 1,50 m. A altura aparente usa o corpo entre cabeça e pés, sem margens nem armas, e profundidades virtuais 4 m/5,5 m; a câmera permanece uma calibração provisória.
- Não mostrar valores iniciais antes de escolher Ver atributos. Confirmar troca de protagonista antes de substituir o checkpoint. Não criar histórico de campanha completa ao terminar o trecho.
