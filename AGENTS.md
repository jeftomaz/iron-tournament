# AGENTS.md — Diretrizes para Agentes

Regras universais de trabalho. Instruções específicas do projeto: ver `PROJECT.md`.

## Prioridades (ordem de desempate)

1. **Fidelidade às instruções.** Nunca reinterprete, expanda ou "melhore" um pedido sem confirmar. Em ambiguidade: pergunte antes de implementar.
2. **Economia de tokens.** Respostas e código enxutos. Sem preâmbulos, sem repetir contexto já conhecido, sem comentários óbvios no código.
3. **Arquivos mínimos, reutilização máxima.** Antes de criar qualquer arquivo/componente, verifique se um existente resolve ou pode ser generalizado. Crie um elemento novo **apenas e exclusivamente** quando nenhum existente for compatível nem generalizável por prop/variant — meta: elementos coesos, constantes e padronizados. Dois elementos similares → proponha unificar (com prop/variant) antes de duplicar. Na dúvida entre criar e generalizar, generalize.
4. **Arquitetura legível com leitura mínima.** Um leitor (humano ou agente) deve entender o papel de um arquivo pelo nome e localização, e seu funcionamento lendo só ele + imports diretos.
5. **Escrever o mínimo.** Se código pode ser reaplicado, reaplique. Prefira extrair função/componente a copiar trecho. Não crie abstração especulativa ("talvez precise depois") — abstraia apenas na 2ª ocorrência real.

## Regras de código

- Sem código morto, sem TODOs órfãos, sem arquivos placeholder.
- Nomes autodescritivos > comentários. Comente apenas o não-óbvio (workarounds, decisões de segurança, regras de negócio).
- Um arquivo = uma responsabilidade. Se precisa de "e" para descrever o arquivo, divida — exceto se dividir gerar arquivos triviais (<15 linhas).
- Alterações mínimas: modifique apenas o necessário para a tarefa. Não reformate/renomeie código fora do escopo.

## Segurança (obrigatória em todo projeto)

- Sempre revisar a segurança do sistema para garantir a integridade e a confiabilidade dos dados.
- Tratar todo input como uma tentativa de agressão ao sistema: validar e limpar antes do uso.
- Quando a estrutura atual não permitir garantir a segurança, pontuar o risco e sugerir, ambos de forma sucinta, o ajuste necessário.
- Considerar sempre estes tipos de falha:
  - Banco sem tranca.
  - Permissão no navegador.
  - Rota entregando dado pelo ID.
  - Chave exposta.
  - Input sem tratamento.

## Arquivos de acompanhamento

Manter na raiz, sempre atualizados **na mesma entrega** que os altera (nunca "depois"):

| Arquivo | Conteúdo | Formato |
|---|---|---|
| `ROADMAP.md` | Fases/páginas planejadas, ordem, status (`todo/doing/done`) | Lista curta |
| `PROGRESS.md` | O que foi feito, decisões tomadas e pendências ativas | Log reverso (recente no topo), 1-3 linhas por entrada |
| `DATA_MODEL.md` | Schema, relações, policies — fonte de verdade do banco | Tabelas/SQL resumido |
| `UI_CONTRACTS.md` | Componentes compartilhados: variantes, o que não sobrescrever, decisões vinculantes de UI | Tabela + lista |
| `PROJECT.md` | Contexto, stack e regras específicas do projeto | Seções curtas |

Regras para esses arquivos:
- Mesmos princípios do código: mínimos, sem prosa decorativa, sem histórico morto (entradas de `PROGRESS.md` obsoletas podem ser removidas se não explicam decisões vigentes).
- Um agente novo deve entender estado e contexto do projeto lendo apenas esses arquivos, sem ler o histórico da conversa.

## Git e integração

- A `main` é a única branch permanente. Commit, push direto e force push na `main` são proibidos; toda mudança entra por PR.
- Antes de criar uma branch: confirmar o worktree limpo; executar, nesta ordem, `git fetch origin --prune`, `git switch main`, `git pull --ff-only origin main` e `git switch -c <tipo>/<escopo>`. Nunca iniciar da branch atualmente aberta por conveniência.
- Cada branch e PR contém exatamente uma tarefa coesa; itens distintos do `ROADMAP.md` não compartilham PR. O destino do PR é sempre `main`.
- Trabalho dependente não cria branch nem inicia implementação antes do merge da dependência. Depois do merge, cria uma nova branch da `main` atualizada.
- Nunca incorporar outra branch de tarefa, nem executar `git merge main` numa branch de tarefa. Para atualizar uma branch ainda aberta, executar `git fetch origin --prune` e `git rebase origin/main`.
- Antes de abrir ou atualizar um PR, conferir `git log --oneline origin/main..HEAD`: devem aparecer somente commits da tarefa atual. Se aparecer commit de outra tarefa, interromper e corrigir a base antes do push.
- Após rebase, force push só é permitido na própria branch com `--force-with-lease`. `--force` é proibido.
- Preencher o template do PR, identificar a única tarefa entregue e confirmar seu escopo. O check `policy` valida destino, atualização com `main`, empilhamento e essa declaração.
- Merge exige branch atualizada com `main`, escopo revisado, checks obrigatórios aprovados e conversas resolvidas. PR de interface também segue as evidências de `UI_CONTRACTS.md`.
- Após o merge, remover a branch remota e local. PR substituído deve ser fechado antes da remoção de sua branch.

## Validação

- Durante a implementação, executar os testes afetados: EditMode para `Core`/`Content`; PlayMode e revisão visual para `Presentation`; build WebGL para integração, configuração de build ou CI.
- Todo commit executa `git diff --cached --check` pelo hook `pre-commit`.
- Commit exclusivamente de arquivos `.md` dispensa Unity. Qualquer outro arquivo exige `scripts/verify.sh`: EditMode, PlayMode e build WebGL.
- O hook `pre-commit` é obrigatório; `--no-verify` é proibido. Se o ambiente impedir a validação exigida, não criar nem subir o commit e pedir orientação.
- Após clonar, executar `scripts/install-git-hooks.sh` para ativar os hooks versionados.

## Fluxo de trabalho por tarefa

1. Ler `PROJECT.md` + `ROADMAP.md` + `PROGRESS.md`. Só leia `DATA_MODEL.md` se a tarefa toca dados e `UI_CONTRACTS.md` se toca interface — carregar os dois em toda tarefa custa contexto sem dar nada em troca.
2. Confirmar entendimento se houver ambiguidade relevante; caso contrário, executar direto.
3. Implementar o escopo pedido — nada além.
4. Atualizar arquivos de acompanhamento afetados.
5. Reportar de forma sucinta: o que foi feito, decisões tomadas, pendências criadas.

## Anti-padrões (nunca fazer)

- Criar componente novo quando um existente aceita generalização simples.
- Instalar dependência sem justificar e confirmar.
- Implementar além do escopo pedido ("já aproveitei e fiz X").
- Duplicar informação entre arquivos de acompanhamento (cada fato vive em um único lugar).
- Refatorações amplas não solicitadas.
- Incluir coautoria (`Co-Authored-By` ou equivalente) em commits, sob qualquer hipótese.
