# Progresso

## 2026-10-01

- Definidas telas de referência: mobile `360 × 640` (`9:16`) como foco e desktop `1280 × 720` (`16:9`), com composição responsiva por orientação.
- Definida a entrega Web pelo navegador, sem multiplayer, autenticação ou backend no escopo atual; a CI agora gera artefato WebGL.
- Confirmadas as regras canônicas: o IronTurn é apenas referência, atributos-base permanecem fixos e a reversão restaura todo o encontro, exceto a sequência aleatória.
- Inicializado o projeto Unity 2D pixel-perfect com as assemblies `Core`, `Content`, `Presentation` e testes EditMode; o núcleo não referencia a engine.
- Adicionada pipeline GameCI de testes EditMode e build Linux; PRs de forks não recebem segredos. Falta cadastrar `UNITY_LICENSE` e configurar a proteção da `main` no GitHub.
- A validação local pelo editor permanece pendente: o ambiente atual não inicializou o serviço de licenças do Unity.
- Definidas responsabilidades da equipe, fluxo por Pull Request e Jeferson como integrador da `main`.
- Aprovado GameCI para a futura pipeline; segredos ficam restritos ao GitHub Actions e indisponíveis para PRs de forks.
- Definida revisão de UI por evidência visual no PR, teste manual, PlayMode e build de CI quando disponível.
- Registrado o procedimento de abertura e inspeção local da UI; agentes abrem o Unity Editor e Play Mode quando o ambiente permitir.
- Movido o protótipo Java para diretório externo e adicionado `ironturn/` ao `.gitignore`; ele não faz parte deste repositório.
- Preparada a base do repositório com instruções e assets; arquivos locais e gerados permanecem ignorados.
- Confirmadas sequência fixa, atributos-base fixos e aleatoriedade restrita a dano, crítico, penetração e drops.
- Definido histórico de reversão isolado por encontro, sem apagar HP, itens ou melhorias da campanha.
- Analisados o protótipo Java e os assets disponíveis; nenhum código do jogo foi alterado.
- Definidas premissas iniciais para Unity e propostas correções para estado, reversão, aleatoriedade, eventos e progresso.
- Pendentes: confirmar escopo da reversão, tratamento dos padrões GoF e primeiro marco jogável.
