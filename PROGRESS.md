# Progresso

## 2026-10-02

- Criados via ImageGen os fundos dos demais inimigos e dos heróis, inspirados em Castlevania: Lords of Shadow; mantido o Goblin aprovado. O Mago normal e com fogo compartilham cenário.
- Conferidos visualmente os fundos; arquivos, dimensões e prompts registrados em `assets/backgrounds/metadata.json`.
- Pendentes: aprovação visual dos novos cenários e integração no Unity.

## 2026-10-01

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
