# Iron Tournament

## Objetivo

RPG 2D de combate por turnos em Unity, com apresentação inspirada em RPGs 16-bit. A primeira etapa não inclui exploração.

## Referência

- `../ironturn-exemplo/`: especificação funcional externa em Java; opcional para consulta e não versionada neste repositório. Suas regras serão revisadas, não portadas literalmente.
- `Assets/`: sprites direcionais estáticos para personagens e conteúdo do Unity.
- `Assets/Backgrounds/`: cenários estáticos de batalha e o respectivo `metadata.json`.

## Stack

- Unity `6000.5.7f1`
- C#
- Projeto 2D pixel-perfect
- Interface de batalha em Unity UI (uGUI), na assembly `Presentation`; contratos dos componentes e decisões visuais em `UI_CONTRACTS.md`.
- GameCI executa testes EditMode e build WebGL em contêiner na pipeline do GitHub Actions
- Build Web para execução no navegador
- CI: testes EditMode e build WebGL usam GameCI em contêiner no runner hospedado Ubuntu, com `UNITY_LICENSE`, `UNITY_EMAIL` e `UNITY_PASSWORD` como secrets.

## Plataformas e telas

- Foco inicial: navegador mobile em `9:16`, com área de referência `360 × 640`.
- Desktop: navegador em `16:9`, com área de referência `1280 × 720`.
- Cada orientação terá composição própria; a interface não deve apenas esticar a outra.
- A primeira publicação Web é cliente estático: multiplayer, autenticação e backend não fazem parte deste escopo.

## Estrutura inicial

- `Core`: regras e estado de runtime sem referência ao Unity.
- `Content`: definições em `ScriptableObject` e referências de assets.
- `Presentation`: cenas, UI e adaptação dos dados de conteúdo ao núcleo.
- `Tests/EditMode`: testes do núcleo sem carregar cenas.
- `Tests/PlayMode`: testes da apresentação carregando a cena de batalha.
- `Tests/PlayMode/Editor`: prévia manual de duelos pelo menu `Iron Tournament`, exclusiva do Editor; reaproveita as assemblies e o contrato de inicialização existentes.

## Mapa técnico

| Limite | Pode depender de | Responsabilidade e contrato |
|---|---|---|
| `Core` | .NET | Estado somente leitura, ações válidas e eventos do combate; sem `UnityEngine`. |
| `Content` | `Core`, Unity | `ScriptableObject`, validação e conversão para configurações puras do núcleo. |
| `Presentation` | `Core`, `Content`, Unity | Cenas, interface e reprodução de eventos; envia ações, sem calcular ou alterar o estado. |
| `Tests/EditMode` | `Core` e/ou `Content` | Regras puras e contratos de mapeamento sem cenas. |
| CI | GitHub Actions, GameCI | Valida EditMode e o build WebGL em contêineres Linux. |

- Dependências seguem `Content -> Core` e `Presentation -> Content/Core`; referências circulares não são aceitas.
- A fronteira pública é `ação -> Core -> estado/eventos`: adaptadores convertem conteúdo na inicialização e a interface apenas apresenta a resposta.
- O bootstrap fornece a batalha via `IBattle` ao apresentador da cena; `Presentation` não instancia o núcleo com atributos ou fonte aleatória próprios. Detalhes da ligação em `UI_CONTRACTS.md`.
- A cena concentra as variantes visuais dos encontros; definições e regras permanecem em `Content/Core`.
- Estado restaurável e fonte aleatória são isolados para que a reversão não retroceda a sequência aleatória.
- Decisões que mudem assemblies, contratos públicos ou esse fluxo exigem PR isolado antes das implementações dependentes.

## Premissas confirmadas

- Combates sequenciais, inicialmente `1 x 1`.
- A campanha segue a ordem fixa: Goblin, Esqueleto, Cavaleiro, Lobisomem, Vampiro, Necromante e Rei Demônio.
- Cada inimigo tem atributos-base canônicos; ao criar o encontro, HP, ATK e DEF efetivos variam independentemente em ±15% e permanecem fixos até seu encerramento.
- Variação de dano, crítico, penetração e drops também permanecem probabilísticos.
- Regras de combate independentes da interface e das animações.
- Configurações em `ScriptableObject`; estado da partida em objetos de runtime.
- O núcleo deve ser testável sem carregar cenas.
- O protótipo Java é referência de design, não fonte de verdade para comportamentos defeituosos.

## Decisões confirmadas

- O Mago mantém `Reverter Turno` e `Reverter Batalha`, com uma carga compartilhada por encontro e sem gastar turno.
- `Reverter Turno` retorna ao início da vez anterior do Mago e devolve imediatamente o controle a ele; toda a rodada desfeita é restaurada.
- `Reverter Batalha` retorna ao estado de entrada do encontro. Ambas as reversões restauram o estado integral, mas não a sequência aleatória, e ficam indisponíveis após a morte.
- Arquitetura idiomática para Unity, sem preservar os padrões GoF apenas por equivalência acadêmica.
- Primeiro marco: Guerreiro contra Goblin, com fluxo completo e testes.
- A entrega navegável usa o build Web do Unity; a hospedagem será definida antes da publicação.
- O golpe de cada inimigo varia ±20% sobre o ATK antes da DEF (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Vampiro, Necromante e Rei Demônio entram em fúria uma vez por encontro: no turno do inimigo com HP ≤ 30%, em vez de atacar, reduzem o HP do herói a 30% do máximo, se estiver acima (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Defender: a DEF efetiva vira `ceil(DEF × 1,5)` durante a próxima ação inimiga e o inimigo sofre 4 de dano direto (definido na revisão do PR #6; confirmado por João Lucas, 2026-10-04).
- Manto de Chamas: quando equipado pelo Mago, causa 5 de dano direto ao inimigo ao término de cada turno que consome ação, seja do Mago ou do inimigo; ações sem consumo de turno não o disparam (confirmado no IronTurn, 2026-10-04).

## Restrições

- Não implementar exploração no escopo inicial.
- Não instalar dependências sem aprovação.
- Não colocar regras de combate em componentes de UI ou animação.

## Colaboração

| Membro | Entregas próprias | Limite de responsabilidade |
|---|---|---|
| Jeferson Tomaz | Arquitetura entre assemblies, tipos de `Content`, adaptação `Content -> Core`, bootstrap, persistência Web, CI, build WebGL e integração da `main` | Não implementa regra de combate na integração nem comportamento visual nos adaptadores |
| João Lucas | Estado e regras puras do `Core`, fluxo de turnos, aleatoriedade injetável, Guerreiro, Mago, reversão, inimigos, itens, drops, campanha, modo inimigo e testes EditMode | Não referencia `UnityEngine`, cenas, animações ou componentes de UI |
| João Pedro | Cenas, prefabs, assets configurados, layouts mobile/desktop, HUD, menus, apresentação dos eventos, animações, áudio e testes PlayMode da interface | Não calcula resultados nem altera diretamente o estado de batalha |

### Contrato de integração

- João Lucas expõe estado somente leitura, ações válidas e eventos resultantes; João Pedro envia apenas a escolha do jogador e apresenta a resposta.
- Jeferson define e revisa as fronteiras públicas entre `Core`, `Content` e `Presentation`; mudanças nessas fronteiras exigem PR isolado antes das implementações dependentes.
- João Pedro instancia os `ScriptableObject`; Jeferson mantém seus tipos e validação; João Lucas define quais valores o núcleo requer.
- Testes EditMode das regras pertencem a João Lucas; testes PlayMode e evidência visual pertencem a João Pedro; Jeferson mantém a execução de ambos na CI.
- Cada branch nasce da `main` atualizada e cada PR aponta para `main`; branches e PRs empilhadas sobre outra feature são proibidas. Trabalho dependente espera o merge e então faz rebase na `main`.
- Branches de trabalho seguem `feature/<descricao-em-ingles>`.
- Antes de cada etapa, buscar e revisar os commits novos de Jeferson e João Lucas, incluindo branches remotas; dependências seguem o fluxo de aprovação e integração na `main`.
- Antes de subir commit que altere código, assets, `Packages/`, `ProjectSettings/` ou CI, executar EditMode, PlayMode e build WebGL; mudanças exclusivamente documentais exigem `git diff --check`. Todo PR interno roda CI com validações e build.
- O estado visual é acompanhado localmente pelo Unity Editor, em Play Mode, nas janelas Game, Scene e Inspector.

## CI

- Os runners hospedados `ubuntu-latest` executam os contêineres GameCI para testes EditMode e build WebGL.
- O runner macOS e a sessão do Unity Hub não são dependências da CI; ficam disponíveis apenas para desenvolvimento local.
- Cada job restaura e salva somente `Library`, com chave separada por alvo, sistema e fontes/configuração do Unity; a primeira execução continua fria.
- A workflow executa uma verificação leve em toda PR; GameCI só roda se mudarem `Assets/`, `Packages/`, `ProjectSettings/` ou a própria workflow. Uma execução completa adicional é manual (`workflow_dispatch`), não no push pós-merge.
- A `main` só recebe código já validado pela PR atualizada; novas pushes na mesma PR cancelam a execução anterior.

## Segurança do repositório público

- Segredos ficam somente em GitHub Actions Secrets; nunca em arquivos, logs, exemplos ou histórico Git.
- A licença Personal e as credenciais da conta Unity ficam exclusivamente nos secrets `UNITY_LICENSE`, `UNITY_EMAIL` e `UNITY_PASSWORD`; nunca em arquivo versionado, log ou artefato.
- O runner atende somente este repositório e código de colaboradores confiáveis; PRs de forks não executam CI nele.
- A cache não inclui arquivos de credencial, artefatos ou secrets: apenas o diretório transitório `Library` do Unity.
- PRs de forks podem executar apenas a verificação de escopo, sem GameCI, cache ou acesso aos secrets.
- Antes do merge, revisar o diff e a saída da CI para detectar chaves, tokens ou dados pessoais.
