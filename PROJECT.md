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
- `Presentation`: cenas, interface e reprodução de eventos.
- `Tests/EditMode`: testes do núcleo sem carregar cenas.
- `Tests/PlayMode`: testes da apresentação carregando o menu e a cena de batalha.

## Mapa técnico

| Limite | Pode depender de | Responsabilidade e contrato |
|---|---|---|
| `Core` | .NET | Estado somente leitura, ações válidas e eventos do combate; sem `UnityEngine`. |
| `Content` | `Core`, Unity | `ScriptableObject`, validação e conversão para configurações puras do núcleo. |
| `Presentation` | `Core`, `Content`, Unity | Cenas, interface e reprodução de eventos; envia ações, sem calcular ou alterar o estado. |
| `Bootstrap` | `Core`, `Content`, `Presentation`, Unity | Cria a campanha pelo conteúdo e entrega cada batalha ativa ao apresentador; não contém regras. |
| `Tests/EditMode` | `Core` e/ou `Content` | Regras puras e contratos de mapeamento sem cenas. |
| CI | GitHub Actions, GameCI | Valida EditMode e o build WebGL em contêineres Linux. |

- Dependências seguem `Content -> Core` e `Presentation -> Content/Core`; referências circulares não são aceitas.
- A fronteira pública é `ação -> Core -> estado/eventos`: adaptadores convertem conteúdo na inicialização e a interface apenas apresenta a resposta.
- O bootstrap fornece cada batalha via `IBattle` ao apresentador da cena; `Presentation` não instancia o núcleo com atributos, campanha ou fonte aleatória próprios. Detalhes da ligação em `UI_CONTRACTS.md`.
- `MainMenu` é a entrada do build e carrega `Battle`; a cena de batalha concentra as variantes visuais dos encontros, enquanto definições e regras permanecem em `Content/Core`.
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
- Drops: após cada vitória que não é a última, 2 itens de tipos distintos por sorteio ponderado; pesos Poção de Cura 3, Gema de Sangue 3, Runa do Guardião 3, Cristal da Ruína 2, Elixir da Vida 1, Defesa Divina 1 e exclusivos 2 cada (Pergaminho para o Guerreiro; Manto e Chifre para o Mago); efeitos na tabela do `DATA_MODEL.md` (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Pergaminho Misterioso: com HP ≤ 30%, cura 50% do HP máximo e gasta o turno (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Manto de Chamas: não volta a ser oferecido a quem já o tem (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Chifre da Irmandade: armar gasta o turno; o próximo golpe letal é interceptado por um golpe de 30 + ATK do Mago; se o inimigo cair, o HP do Mago sobe a 30% do máximo, senão fica em 1 (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Modo inimigo: o inimigo escolhido joga com seus atributos-base e enfrenta um oponente sorteado por tier (Goblin/Esqueleto, Cavaleiro/Lobisomem, Vampiro/Necromante), sem repetir o escolhido; antes do chefe escolhe 1 de 3 itens base; o chefe é o snapshot final de uma classe concluída, sorteada, sem variação nem fúria (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Fúria do herói no modo inimigo: com HP ≤ 30%, vira a única ação, gasta o turno e reduz o oponente a 30% do HP máximo, uma vez por encontro (decisão de João Lucas, 2026-10-04, a partir do protótipo).
- Vampiro e Necromante, como heróis do modo inimigo, atacam ignorando a DEF; os demais usam o ataque físico com crítico e penetração (decisão de João Lucas, 2026-10-04, a partir do protótipo).

## Restrições

- Não implementar exploração no escopo inicial.
- Não instalar dependências sem aprovação.
- Não colocar regras de combate em componentes de UI ou animação.
- Até a tarefa dedicada após a limpeza das branches, não ajustar tamanho, proporção ou perspectiva visual dos personagens. A tarefa futura definirá, de uma vez, a escala-base de cada personagem e a redução por profundidade do inimigo.

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
- O fluxo Git, os gates de integração e a validação local seguem `AGENTS.md`; Jeferson integra a `main` somente depois de revisar base, escopo e checks do PR.
- O estado visual é acompanhado localmente pelo Unity Editor, em Play Mode, nas janelas Game, Scene e Inspector.

## CI

- Toda PR executa o check `policy`, sem secrets, para validar destino `main`, base atualizada, ausência de empilhamento e declaração de tarefa única.
- Os runners hospedados `ubuntu-latest` executam os contêineres GameCI para testes EditMode e build WebGL.
- O runner macOS e a sessão do Unity Hub não são dependências da CI; ficam disponíveis apenas para desenvolvimento local.
- Cada job restaura e salva somente `Library`, com chave separada por alvo, sistema e fontes/configuração do Unity; a primeira execução continua fria.
- Toda PR interna executa EditMode, PlayMode e build WebGL, inclusive após um rebase excepcional. PRs de forks executam somente `policy`, sem GameCI, cache ou secrets, e precisam ser reproduzidas numa branch interna criada da `main` antes do merge.
- A `main` só recebe código já validado pela PR atualizada; novas pushes na mesma PR cancelam a execução anterior.

## Segurança do repositório público

- Segredos ficam somente em GitHub Actions Secrets; nunca em arquivos, logs, exemplos ou histórico Git.
- A licença Personal e as credenciais da conta Unity ficam exclusivamente nos secrets `UNITY_LICENSE`, `UNITY_EMAIL` e `UNITY_PASSWORD`; nunca em arquivo versionado, log ou artefato.
- O runner atende somente este repositório e código de colaboradores confiáveis; PRs de forks não executam CI nele.
- A cache não inclui arquivos de credencial, artefatos ou secrets: apenas o diretório transitório `Library` do Unity.
- Código vindo de fork só entra após revisão e validação completa numa branch interna confiável.
- Antes do merge, revisar o diff e a saída da CI para detectar chaves, tokens ou dados pessoais.
