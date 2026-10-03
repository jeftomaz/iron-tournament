# Progresso

## 2026-10-03

- Validados testes EditMode e build WebGL em contêineres GameCI hospedados; a proteção da `main` é a pendência de infraestrutura imediata.

## 2026-10-02

- Criados cenários estáticos de batalha para heróis e inimigos; arquivos e prompts estão em `assets/backgrounds/metadata.json` e aguardam integração no Unity.
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
