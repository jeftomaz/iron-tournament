# Iron Tournament

## Objetivo

RPG 2D de combate por turnos em Unity, com apresentação inspirada em RPGs 16-bit. A primeira etapa não inclui exploração.

## Referência

- `../ironturn/`: especificação funcional externa em Java; opcional para consulta e não versionada neste repositório. Suas regras serão revisadas, não portadas literalmente.
- `Assets/`: sprites direcionais estáticos para personagens e conteúdo do Unity.

## Stack

- Unity `6000.5.7f1`
- C#
- Projeto 2D pixel-perfect
- GameCI aprovado exclusivamente para a pipeline do GitHub Actions
- Build Web para execução no navegador

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

## Premissas confirmadas

- Combates frontais e sequenciais, inicialmente `1 x 1`.
- A campanha segue a ordem fixa: Goblin, Esqueleto, Cavaleiro, Lobisomem, Vampiro, Necromante e Rei Demônio.
- Atributos-base dos inimigos são fixos para preservar a curva de dificuldade.
- Variação de dano, crítico, penetração e drops permanecem probabilísticos.
- Regras de combate independentes da interface e das animações.
- Configurações em `ScriptableObject`; estado da partida em objetos de runtime.
- O núcleo deve ser testável sem carregar cenas.
- O protótipo Java é referência de design, não fonte de verdade para comportamentos defeituosos.

## Decisões confirmadas

- Reversão restaura integralmente o estado do encontro, exceto a sequência aleatória.
- Arquitetura idiomática para Unity, sem preservar os padrões GoF apenas por equivalência acadêmica.
- Primeiro marco: Guerreiro contra Goblin, com fluxo completo e testes.
- A entrega navegável usa o build Web do Unity; a hospedagem será definida antes da publicação.

## Restrições

- Não implementar exploração no escopo inicial.
- Não instalar dependências sem aprovação.
- Não colocar regras de combate em componentes de UI ou animação.

## Colaboração

- Jeferson Tomaz: arquitetura, infraestrutura, CI, integração e revisão da `main`.
- João Lucas: núcleo de combate e testes unitários independentes de cenas.
- João Pedro: cenas, interface, animações e integração dos assets.
- Cada mudança usa branch própria e Pull Request; ninguém envia diretamente para a `main`.
- A suíte completa roda localmente antes de cada commit; todo PR roda CI com testes essenciais, validações e build.
- O estado visual é acompanhado localmente pelo Unity Editor, em Play Mode, nas janelas Game, Scene e Inspector.

## Segurança do repositório público

- Segredos ficam somente em GitHub Actions Secrets; nunca em arquivos, logs, exemplos ou histórico Git.
- Credenciais da Unity usadas pelo GameCI devem ter o menor escopo possível.
- PRs de forks não recebem segredos nem executam etapas que dependam deles.
- Antes do merge, revisar o diff e a saída da CI para detectar chaves, tokens ou dados pessoais.
