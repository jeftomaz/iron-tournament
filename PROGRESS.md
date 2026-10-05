# Progresso

## 2026-10-04 — Validação para commit do Mago

- EditMode 60/60, PlayMode 27/27 e build WebGL concluídos sem erros ou avisos; resultados em `Logs/PreCommit-*`, build em `Builds/Mage-WebGL` (não versionados). Prévia do Inspector atualizada fora de `OnValidate` para evitar avisos de alteração do layout.
- Revisados `ffc2dc4`/`414801e` de Jeferson: somente regras de branches a partir da `main` e validação completa, sem bootstrap. A base atual inclui seis entregas do Guerreiro ainda fora da `main`; aguardando decisão do usuário sobre exceção nesta entrega ou integração prévia.

## 2026-10-04 — Teste manual de duelos no Editor

- Adicionado `Iron Tournament > Testar duelo selecionado` na assembly de testes existente: liga os assets escolhidos ao apresentador e libera o teste de HP, ataque, defesa e reversões pelo núcleo. Não acompanha o build; bootstrap de Jeferson continua pendente.
- Prévia usa atributos-base e seed fixa, sem recursos de configuração da campanha. PlayMode 27/27 passou; cliques reais de início, ataque e reversão conferidos no Editor com Mago/Goblin. Remotas conferidas novamente; sem novos commits de Jeferson nem bootstrap publicado.

## 2026-10-04 — Mago contra os seis inimigos configurados

- Criada `feature/mage-vs-enemies` a partir do Rei Demônio `6364bde`; revisadas as branches de Jeferson/João Lucas e incorporada a `main` aprovada `e485d37`, incluindo campanha/fúria. Cena, menu e efeitos compartilhados adaptados para Mago, com sprites existentes e assets canônicos.
- Validação local: EditMode 60/60 e PlayMode 26/26; ataque nos seis inimigos, ambas as reversões com retorno imediato, descarte de efeitos pendentes, reversão da fúria e bloqueio após vitória/derrota. Introdução/combate conferidos em `360x640` e `1280x720`; capturas em `Logs/Mage-*.png` (não versionadas).
- Pendentes bootstrap de Jeferson, fluxo automático de campanha, Cavaleiro, seleção de classe e saque/itens. Alterações de apresentação ainda sem commit/push.

## 2026-10-04 — Apresentação Guerreiro x Rei Demônio

- Commit/push do Necromante confirmado em `c9a83f5`; criada `feature/warrior-vs-demon-king` e incorporada a `main` aprovada `accf883` (cobertura de reversão). Revisadas atualizações remotas de João Lucas; campanha/fúria continuam fora da `main`. Sprites de Jeferson (`4a795b6`) importados e configurados na cena compartilhada.
- Validação local: EditMode 38/38 e PlayMode 21/21, incluindo seleção, perspectiva, limites da arena e ataque do Rei Demônio com núcleo real. Introdução/combate conferidos em `360x640` e `1280x720`, capturas em `Logs/DemonKing-*.png` (não versionadas).
- Pendentes integração/apresentação da fúria, bootstrap, variação dos atributos, transição automática da campanha e validação WebGL.

## 2026-10-04 — Apresentação Guerreiro x Necromante

- Push do Vampiro confirmado em `c3887f5`; criada `feature/warrior-vs-necromancer`. Revisados commits remotos de João Lucas e `4a795b6` de Jeferson; incorporados somente os sprites do Necromante, com importação Unity e configuração na cena compartilhada. Alterações de regras fora da `main` permanecem para integração aprovada.
- Validação local: EditMode 34/34 e PlayMode 21/21, incluindo seleção, perspectiva, limites da arena e ataque do Necromante com núcleo real. Introdução/combate conferidos em `360x640` e `1280x720`, capturas em `Logs/Necromancer-*.png` (não versionadas).
- Pendentes integração/apresentação da fúria, bootstrap, variação dos atributos, transição automática da campanha e validação WebGL.

## 2026-10-04 — Apresentação Guerreiro x Vampiro

- Criada `feature/warrior-vs-vampire` a partir do Lobisomem `b9348b3`; conferidas branches remotas, sem novos commits. Revisada a fúria em `origin/feat/core-campaign`, ainda fora da `main`; configuração do Vampiro reutiliza cena/componentes existentes.
- Validação local: EditMode 34/34 e PlayMode 21/21, incluindo o Vampiro nos testes compartilhados de seleção, perspectiva, limites da arena e ataque com núcleo real. Introdução/combate conferidos em `360x640` e `1280x720`, capturas em `Logs/Vampire-*.png` (não versionadas).
- Pendentes integração/apresentação da fúria após aprovação do núcleo, bootstrap, variação dos atributos, transição automática e validação WebGL.

## 2026-10-04 — Apresentação Guerreiro x Lobisomem

- Criada `feature/warrior-vs-werewolf` a partir do commit Esqueleto `3902c8e`; preparação parcial do Cavaleiro retirada por orientação do usuário. Sem novos commits remotos; Lobisomem configurado na cena compartilhada, conforme `DATA_MODEL.md` e `UI_CONTRACTS.md`.
- Validação local: EditMode 34/34 e PlayMode 21/21; testes compartilhados cobrem seleção, perspectiva, ausência de corte do sprite e ataque com núcleo real. Introdução/combate conferidos em `360x640` e `1280x720`, capturas em `Logs/Werewolf-*.png` (não versionadas).
- Pendentes bootstrap, variação dos atributos, transição automática da campanha e validação WebGL; Goblin continua como padrão da cena. O asset Warrior permanece no lado do herói; não configurado como inimigo nem alterada a regra do power-up.

## 2026-10-04 — Apresentação Guerreiro x Esqueleto

- Criada `feature/warrior-vs-skeleton` a partir da entrega Goblin `92d81e0`, incorporando a `main` aprovada `4e2b9d3`; revisados os commits de Mago/reversão e a campanha remota ainda não integrada. Esqueleto preparado como variante da cena existente, conforme `DATA_MODEL.md` e `UI_CONTRACTS.md`.
- Validação local: EditMode 34/34 e PlayMode 21/21; seleção inválida, incompatibilidade com `IBattle`, ataque com núcleo real e perspectiva do Esqueleto. Introdução/combate conferidos em `360x640` e `1280x720`, capturas em `Logs/Skeleton-*.png` (não versionadas).
- Pendentes bootstrap de Jeferson, variação dos atributos e transição automática da campanha; a cena mantém Goblin como padrão. Revisão/commit/PR desta etapa pendentes; build WebGL desta variante ainda não executado.

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

- Tornadas vinculantes as regras de integração: branches e PRs sempre partem da `main`, sem encadeamento entre features; antes de commit de código/configuração, executar EditMode, PlayMode e build WebGL.
- Preparada `Assets/Scenes/Battle.unity` com cenário, prefab compartilhado e composições por orientação; adicionada a transição visual pelo botão `Iniciar combate`, conforme `UI_CONTRACTS.md`.
- Conferidas capturas antes/depois em `360x640` e `1280x720`, clique real e orientação em Play Mode; testes locais EditMode (2/2) e PlayMode (1/1) passaram.
- Registrado em `PROJECT.md` o padrão de branches solicitado pelo João Pedro.

## 2026-10-04

- Corrigido o contrato do Manto de Chamas após cotejo com o IronTurn: ele causa 5 de dano direto após cada turno que consome ação, tanto do Mago quanto do inimigo; ações sem consumo não o disparam.
- A ordem canônica dos sete inimigos virou contrato: `CampaignConfiguration.CanonicalOrder` é validada no construtor e no `ContentValidator`, com testes de rejeição; os testes de campanha passam a usar a sequência completa. As regras de ±20% no dano inimigo e de fúria foram confirmadas e registradas em `PROJECT.md` (revisão do PR #9).
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
