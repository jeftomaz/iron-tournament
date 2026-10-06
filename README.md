# Iron Tournament

Protótipo de RPG 2D por turnos feito em Unity. A demonstração acompanha um herói em uma campanha linear de sete batalhas, do Goblin ao Rei Demônio.

## Demonstração

1. Na tela inicial, consulte `Regras`, `Objetivo` e `Desafios` se desejar apresentá-los.
2. Clique em `Iniciar campanha` para abrir a cena de batalha.
3. Clique em `Iniciar combate`.
4. Em cada turno, escolha uma ação disponível, como `Atacar` ou `Defender`.
5. Após cada vitória intermediária, escolha um dos dois saques oferecidos para seguir ao próximo encontro.
6. A campanha termina ao derrotar o Rei Demônio; uma derrota encerra a tentativa atual.

Ordem da campanha: Goblin, Esqueleto, Cavaleiro, Lobisomem, Vampiro, Necromante e Rei Demônio.

## Mecânicas apresentadas

- Combate por turnos 1 x 1 com vida, ataque, defesa, crítico e penetração.
- `Defender` aumenta temporariamente a defesa e reflete dano no inimigo.
- Atributos dos inimigos variam uma vez por encontro.
- Vampiro, Necromante e Rei Demônio entram em fúria quando estão com pouca vida.
- Saques entre batalhas concedem melhorias ou recursos ao herói durante a campanha.
- Interface com sprites, cenários e HUD específicos para cada inimigo.

## Executar no Unity

Pré-requisitos:

- Unity Hub com Unity `6000.5.7f1`.
- Uma licença Unity ativa no computador.

Passos:

1. Clone o repositório e abra sua raiz no Unity Hub com a versão indicada.
2. Aguarde a importação dos assets.
3. Abra `Assets/Scenes/MainMenu.unity`.
4. Inicie o Play Mode.
5. Siga o fluxo de demonstração acima.

`MainMenu` é a primeira cena habilitada no build; por isso, uma build WebGL também inicia por ela.

## Build WebGL

No Unity, selecione **File > Build Settings**, escolha **WebGL**, use **Switch Platform** se necessário e clique em **Build**. Hospede a pasta gerada em um servidor HTTP; não abra o arquivo HTML diretamente no navegador.

Em uma PR, a integração contínua também gera um artefato WebGL chamado `webgl`.

## Validação local

Instale o hook versionado uma vez após clonar:

```bash
scripts/install-git-hooks.sh
```

Execute a validação completa:

```bash
scripts/verify.sh
```

Ela roda testes EditMode, testes PlayMode e uma build WebGL descartável. A licença do Unity precisa estar inicializada antes do comando.

## Estrutura

- `Assets/IronTournament/Runtime/Core`: regras puras de combate e campanha.
- `Assets/IronTournament/Runtime/Content`: definições de conteúdo e mapeamento para o núcleo.
- `Assets/IronTournament/Runtime/Presentation`: interface, HUD e eventos visuais.
- `Assets/IronTournament/Runtime/Bootstrap`: ligação entre cenas e campanha.
- `Assets/Scenes/MainMenu.unity`: entrada da demonstração.
- `Assets/Scenes/Battle.unity`: batalha e progressão da campanha.

Os documentos `PROJECT.md`, `ROADMAP.md`, `DATA_MODEL.md` e `UI_CONTRACTS.md` registram os contratos e o escopo técnico do projeto.

## Limites do protótipo

Esta entrega não inclui salvamento persistente, seleção de classe pela interface, modo inimigo, áudio ou publicação web. O foco é demonstrar o fluxo completo de combate e campanha.
