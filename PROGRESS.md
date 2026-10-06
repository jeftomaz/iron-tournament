# Progresso

## 2026-10-06

- Adicionado o `Bootstrap` da cena `Battle`: a seleção padrão Guerreiro/Goblin é mapeada por `ContentMapper`, cria uma `IBattle` ativa no `Core` e a fornece ao `BattlePresenter`, sem transferir regras, variação ou fúria para a interface. Cobertos o factory e o início real da cena em PlayMode; `scripts/verify.sh` passou (EditMode, PlayMode e WebGL).

## 2026-10-05

- Reconstruídos os duelos do Mago contra Goblin, Esqueleto, Lobisomem, Vampiro, Necromante e Rei Demônio na única cena `Battle`: registro visual por combatente, cenário do adversário e sprites direcionais para introdução e combate. O menu passa a ler as habilidades configuradas; o `BattleEventPlayer` apresenta ataque mágico, reversões e fúria sem alterar o núcleo. Restaurada a prévia Guerreiro/Goblin, removida a alteração proibida de tamanho do inimigo e coberta `Reverter Batalha` em PlayMode; `scripts/verify.sh` passou (EditMode, PlayMode e WebGL).
- Tornado explícito o fluxo Git: `main` única branch permanente, uma tarefa por branch/PR, integração em fila linear, rebase único e excepcional para resgatar branch histórica e limpeza obrigatória após integração.
- Removida a orientação residual de rebase do template e do guard de PR; branches desatualizadas devem ser recriadas a partir da `main`.
- Separada a validação por etapa: testes afetados durante o desenvolvimento, `git diff --cached --check` em todo commit, suíte Unity completa em mudanças não documentais e novamente na CI interna.
- Adicionado o check `policy` para bloquear PR com destino incorreto, branch desatualizada ou empilhada e sem declaração de tarefa única; o template padroniza escopo e validação.
- Implementados progresso e modo inimigo: `CampaignRun.FinalSnapshot` guarda atributos finais com melhorias, limitado a 9999; `ProgressData` libera o modo com as duas classes concluídas; o modo usa o inimigo escolhido com `Fury`, oferece três itens antes do chefe e termina contra uma classe concluída.
- Adicionada a regressão (`RegressionTests`): 40 sementes por classe na campanha canônica e por personagem no modo inimigo, verificando término, HP dentro da faixa, evento final coerente, guarda zerada e determinismo por semente.
- Endurecidos contra overflow de `int` o crítico, o golpe do guardião, a DEF com guarda e o teto da variação de dano para atributos extremos de conteúdo ou save adulterado.
- Reconstruída a apresentação Guerreiro/Goblin: cena `Battle`, HUD, menu, eventos, presenter, prefab compartilhado, assets da vertical e testes PlayMode; o bootstrap permanece pendente.
- Reconstruída a apresentação Guerreiro/Esqueleto como variante da cena compartilhada, com assets, seleção validada, compatibilidade com o bootstrap e cobertura PlayMode; Goblin permanece o padrão e a escala será refinada após a organização.
- Reconstruída a variante Guerreiro/Lobisomem sem o limitador de escala legado; ajustes de proporção e perspectiva ficam proibidos até a tarefa dedicada após a limpeza.
- Reconstruída a variante Guerreiro/Vampiro na cena compartilhada, com assets, seleção validada e cobertura PlayMode; escala, proporção e perspectiva permanecem fora do escopo.
- Reconstruída a variante Guerreiro/Necromante na cena compartilhada, com sprites, assets, seleção validada e cobertura PlayMode; escala, proporção e perspectiva permanecem fora do escopo.
- Reconstruída a variante Guerreiro/Rei Demônio na cena compartilhada, com sprites, assets, seleção validada e cobertura PlayMode; escala, proporção e perspectiva permanecem fora do escopo.

## 2026-10-04 — Assets do duelo e build WebGL

- Criados os cinco assets da vertical com os tipos existentes, conforme `DATA_MODEL.md`; a cena `Battle` é a entrada habilitada no build. Conferidos os valores com o Java e os testes aprovados de João Lucas; sem novos commits na `main` durante esta etapa.
- Testes locais: EditMode 28/28 e PlayMode 19/19, incluindo mapeamento dos assets reais e ações aceitas pelo núcleo. Build WebGL de desenvolvimento gerado em `Builds/WarriorGoblin-WebGL`, sem erros/avisos; não publicado.
- Pendentes bootstrap de Jeferson para inicializar o duelo e aplicar a variação dos atributos, validação do fluxo na cena e revisão/commit/PR. Configurações, referências de sprites e build estão preparados; o build atual apresenta a cena sem iniciar o núcleo.

## 2026-10-04 — Ligação da interface ao núcleo

- Incorporada a `main` aprovada `f3c029a` com Guerreiro/Goblin; revisadas as regras de João Lucas, preservando o trabalho local. `BattlePresenter` ligado na cena envia escolhas ao `IBattle`, sincroniza o HUD e só reabre ações após os eventos, conforme `UI_CONTRACTS.md`.
- Testes locais: EditMode 27/27 e PlayMode 19/19; ataque/defesa com núcleo real, clique repetido, vitória/derrota e retomada após interrupção. Esses testes usam configurações próprias de teste; a cena aguarda inicialização pelo bootstrap.

## 2026-10-04 — Adaptação dos eventos do núcleo

- `BattleEventPlayer` adapta os eventos aprovados conforme `UI_CONTRACTS.md`; mostra HP na ordem dos impactos e distingue reflexão, crítico e penetração. Testes locais: EditMode 13/13 e PlayMode 15/15, incluindo lote inválido sem efeitos parciais e cancelamento das atualizações pendentes.
- Atualizado `PROJECT.md` com Unity UI e testes PlayMode; consultadas as branches remotas dos dois colaboradores, sem novos commits desde a revisão anterior.

## 2026-10-04 — Encerramento da batalha

- Preparada apresentação de vitória/derrota conforme `UI_CONTRACTS.md`, consumindo `BattleEndedEvent`; mantém o resultado após o impacto, oculta ações e não altera HP. Testes locais: EditMode 13/13 e PlayMode 11/11; conferidas as duas telas em mobile/desktop com dados de teste.
- Incorporada a `main` aprovada (`07fb329`), preservando o trabalho local; revisados contratos `d448db5`/`8a9731b` e correção de Defender `62fcc6b` (DEF ×1,5, arredondada para cima, e 4 de dano).

## 2026-10-04 — Efeitos de combate

- Preparado `BattleEventPlayer` na cena existente, conforme `UI_CONTRACTS.md`: efeitos sequenciais de ataque, defesa e impacto, com bloqueio de entrada e cancelamento; sem calcular dano ou alterar HP.
- Conferidas prévias visuais mobile/desktop; testes locais EditMode (2/2) e PlayMode (8/8) passaram, incluindo ordem dos efeitos, disponibilidade atualizada, cancelamento, entradas inválidas e troca de orientação durante impacto.

## 2026-10-04 — HUD e menu

- Preparados `BattleHud` e `ActionMenu` na cena existente, conforme `UI_CONTRACTS.md`; conferidas composições mobile/desktop e testes locais EditMode (2/2) e PlayMode (4/4), incluindo entradas inválidas e cliques repetidos.

## 2026-10-04

- Preparada `Assets/Scenes/Battle.unity` com cenário, prefab compartilhado e composições por orientação; adicionada a transição visual pelo botão `Iniciar combate`, conforme `UI_CONTRACTS.md`.
- Conferidas capturas antes/depois em `360x640` e `1280x720`, clique real e orientação em Play Mode; testes locais EditMode (2/2) e PlayMode (1/1) passaram.
- Registrado em `PROJECT.md` o padrão de branches solicitado pelo João Pedro.

## 2026-10-04

- Automatizada a validação antes de commits pelo hook versionado `pre-commit`; a CI interna também passou a executar a suíte completa em toda PR.
- Tornadas vinculantes as regras de integração: branches e PRs sempre partem da `main`, sem encadeamento entre features.
- Corrigido o contrato do Manto de Chamas após cotejo com o IronTurn: ele causa 5 de dano direto após cada turno que consome ação, tanto do Mago quanto do inimigo; ações sem consumo não o disparam.
- A ordem canônica dos sete inimigos virou contrato: `CampaignConfiguration.CanonicalOrder` é validada no construtor e no `ContentValidator`, com testes de rejeição; os testes de campanha passam a usar a sequência completa. As regras de ±20% no dano inimigo e de fúria foram confirmadas e registradas em `PROJECT.md` (revisão do PR #9).
- Concluída a cobertura da reversão: além de HP, atributos, defesa temporária, fase e rodada, os testes restauram a flag de fúria (`RevertTurnRestoresTheFuryFlag`) e os consumíveis e efeitos de itens (`RevertTurnRestoresConsumedItemsAndArmedEffects`, `RevertBattleReturnsTheUnusedHorn`).
- Implementados drops e itens: após cada vitória que não é a última, o `CampaignRun` oferece até 2 itens distintos por sorteio ponderado sem reposição, filtrados pela classe; `ChooseDrop` aceita só itens da oferta, aplica modificadores permanentes ou o efeito do item e inicia o próximo encontro; o Manto de Chamas não é oferecido a quem já o tem. Itens e recursos entram no snapshot da reversão.
- Implementada a campanha (`CampaignRun`): encontros na ordem do `CampaignConfiguration`, herói único entre encontros (HP e atributos persistem), atributos do inimigo sorteados uma vez em ±15% (HP, ATK, DEF independentes, arredondados, mínimo 1/1/0), golpe inimigo com variação de ±20% antes da DEF, e `ConcludeEncounter` para avançar, concluir ou falhar.
- Fúria (Vampiro, Necromante, Rei Demônio): uma vez por encontro, no turno do inimigo com HP ≤ 30%, substitui o ataque e reduz o HP do herói a 30% do máximo; a reversão restaura a flag.
- Cobertos em EditMode a restauração de HP, atributos, defesa temporária, fase e rodada, a carga consumida após restaurar e a proibição de reverter após derrota ou vitória. O item de cobertura segue `doing`: efeitos de item, consumíveis e flags de fúria ainda não existem neste ponto e serão cobertos quando entrarem.
- Implementados o Mago (110 HP, 30 ATK, 5 DEF; ataque igual ao ATK, sem rolagem) e a reversão: antes de cada ação que gasta turno o `Battle` guarda o início do turno, e ao começar o encontro guarda a entrada; uma carga por encontro, compartilhada, exige ao menos um turno jogado e é consumida após restaurar. `IBattle.State` mantém a mesma instância ao restaurar. Cada ação só fica disponível se o herói tiver a habilidade configurada (revisão do PR #7).
- Implementado Guerreiro x Goblin no `Core` (`Battle`): o ataque rola 10% de penetração (ignora DEF) e depois 7% de crítico (×2); Defender eleva a DEF a `ceil(DEF × 1,5)` durante a próxima ação inimiga e causa 4 de dano direto (regra definida na revisão do PR #6); o Goblin responde com `ATK − DEF` (mínimo 0); o golpe que vence encerra o encontro antes da resposta.
- Valores canônicos (ver `DATA_MODEL.md`) retirados do protótipo Java no histórico (`5016de1^:ironturn/`).
- Definidos os contratos do `Core` para a `Presentation`: `IBattle` expõe `BattleState` somente leitura, ações disponíveis (`AbilityId`) e `Submit`, que devolve `ActionResult` com eventos ou motivo de rejeição; a implementação chega na Fase 1.
- Só o `Core` cria novos tipos de evento (`AbilityUsedEvent`, `DamageDealtEvent`, `BattleEndedEvent`); o dano informa tipo, crítico e penetração.
- Aleatoriedade injetável por `IRandomSource`; `SeededRandomSource` é determinística por semente e fica fora do estado restaurável.

## 2026-10-03

- Registrado em `PROJECT.md` o mapa técnico vinculante: limites entre assemblies, fluxo `ação -> Core -> estado/eventos`, reversão e responsáveis de integração.
- Restaurada a cache de `Library` exclusivamente nos runners hospedados, separada entre EditMode e WebGL e invalidada por fontes/configuração; não inclui credenciais ou artefatos.
- Movidos os cenários para `Assets/Backgrounds/`, o único caminho de assets do Unity; a coexistência com `assets/` em minúsculas deixava o editor preso na importação no contêiner.
- Ampliado para 60 minutos o timeout do build WebGL enquanto a correção da importação é validada; os cancelamentos não envolveram licença ou segredos.
- Validados testes EditMode e build WebGL em contêineres GameCI hospedados; a `main` exige PR, checks `test` e `build` atualizados, resolução de conversas e aplica as regras a administradores.

## 2026-10-02

- Concluídos os contratos `Content -> Core`: definições Unity, configurações imutáveis sem `UnityEngine`, adaptador e validação de referências, IDs, faixas, duplicidades e texto de exibição.
- Adicionados testes EditMode para mapeamento válido e rejeição de variação inválida.
- Criados cenários estáticos de batalha para heróis e inimigos; arquivos e prompts estão em `Assets/Backgrounds/metadata.json` e aguardam integração nas cenas.
- Migrados os testes EditMode para o GameCI em contêiner hospedado: a licença local do Unity não disponibilizava entitlement para execução headless; testes e build agora usam os mesmos secrets e não dependem do runner macOS. A cobertura usa o padrão do GameCI porque a versão atual da CLI não aceita sua desativação.
- Provisionados `UNITY_LICENSE`, `UNITY_EMAIL` e `UNITY_PASSWORD` como secrets do GitHub; o GameCI recebe os três valores mascarados.
- Validado o build WebGL pelo GameCI; como a ação no macOS usa o editor local, o job foi movido para `ubuntu-latest` para executar de fato no contêiner.
- Removida a cache de `Library` da CI: a ação ficou bloqueada no runner antes do Unity; ela é apenas uma otimização e não participa da validação.
- Restaurado o formato serializado canônico do `TagManager.asset`, rejeitado pelo parser do Unity durante a primeira execução da CI no runner macOS.
- Iniciada a etapa de Jeferson para contratos de `Content`, validação e adaptação para o núcleo; regras de combate e apresentação permanecem fora deste escopo.
- Detalhada a divisão por membro e fase: Jeferson responde por arquitetura/infra/integração, João Lucas pelo núcleo e testes EditMode, e João Pedro pela apresentação e testes PlayMode.
- Confirmada a reversão de turno para o checkpoint anterior à última ação do Mago: ataque A e resposta B são desfeitos, o controle volta ao Mago e a carga é consumida sem retroceder a aleatoriedade.
- Mantida a variação de ±15% em HP, ATK e DEF dos inimigos por encontro; a reversão preserva os valores efetivos já sorteados.

## 2026-10-01

- Definidas telas de referência: mobile `360 × 640` (`9:16`) como foco e desktop `1280 × 720` (`16:9`), com composição responsiva por orientação.
- Definida a entrega Web pelo navegador, sem multiplayer, autenticação ou backend no escopo atual; a CI agora gera artefato WebGL.
- Corrigida a estrutura YAML de `TagManager.asset`; a automação local de testes continua pendente porque a sessão do agente não acessa o serviço de licença do Unity Hub.
- Confirmadas as regras canônicas: o IronTurn é referência revisável e a reversão restaura todo o encontro, exceto a sequência aleatória.
- Inicializado o projeto Unity 2D pixel-perfect com as assemblies `Core`, `Content`, `Presentation` e testes EditMode; o núcleo não referencia a engine.
- Adicionada pipeline de testes EditMode e build Web; a configuração de licença depende do runner macOS local.
- A validação local pelo editor permanece pendente: o ambiente atual não inicializou o serviço de licenças do Unity.
- Definidas responsabilidades da equipe, fluxo por Pull Request e Jeferson como integrador da `main`.
- Aprovado GameCI para a futura pipeline; segredos ficam restritos ao GitHub Actions e indisponíveis para PRs de forks.
- Definida revisão de UI por evidência visual no PR, teste manual, PlayMode e build de CI quando disponível.
- Registrado o procedimento de abertura e inspeção local da UI; agentes abrem o Unity Editor e Play Mode quando o ambiente permitir.
- Movido o protótipo Java para diretório externo e adicionado `ironturn/` ao `.gitignore`; ele não faz parte deste repositório.
- Preparada a base do repositório com instruções e assets; arquivos locais e gerados permanecem ignorados.
- Confirmadas sequência fixa e aleatoriedade em atributos efetivos dos inimigos, dano, crítico, penetração e drops.
- Definido histórico de reversão isolado por encontro, sem apagar HP, itens ou melhorias da campanha.
- Analisados o protótipo Java e os assets disponíveis; nenhum código do jogo foi alterado.
- Definidas premissas iniciais para Unity e propostas correções para estado, reversão, aleatoriedade, eventos e progresso.
