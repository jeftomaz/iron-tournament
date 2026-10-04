# Progresso

## 2026-10-04

- Implementados progresso e modo inimigo: `CampaignRun.FinalSnapshot` guarda os atributos finais com melhorias; `ProgressData` registra uma classe por vez e libera o modo inimigo com as duas. `CampaignRun.StartEnemyMode` usa o inimigo escolhido como herói (com `Fury`), sorteia um oponente por tier (Goblin/Esqueleto, Cavaleiro/Lobisomem, Vampiro/Necromante) sem repetir o escolhido, oferece 3 itens base antes do chefe e termina contra o herói de uma classe concluída, sem variação nem fúria.
- Fúria do herói no modo inimigo: com HP ≤ 30%, `Fury` vira a única ação, gasta o turno e reduz o oponente a 30% do HP máximo, uma vez por encontro; Vampiro e Necromante atacam ignorando a DEF.
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
- Reduzido o uso do GameCI: toda PR faz somente a checagem de escopo; testes e build WebGL rodam em mudanças Unity/CI ou por acionamento manual, sem repetição automática após o merge na `main`.
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
