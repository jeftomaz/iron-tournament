# Contratos de UI

| Componente | Responsabilidade | Não deve fazer |
|---|---|---|
| `BattleView` | Exibir combatentes e cenário | Calcular ou aplicar dano |
| `BattleHud` | Exibir HP, estados e mensagens | Alterar o estado da batalha |
| `ActionMenu` | Mostrar ações disponíveis e enviar a escolha | Decidir disponibilidade de ações |
| `BattleEventPlayer` | Apresentar eventos em sequência | Criar resultados de combate |

## Referências de tela

| Plataforma | Aspect ratio | Área de referência | Prioridade |
|---|:---:|:---:|---|
| Navegador mobile | `9:16` | `360 × 640` | Inicial |
| Navegador desktop | `16:9` | `1280 × 720` | Posterior |

## Regras vinculantes

- A UI consome estado somente para exibição.
- `Presentation` pode referenciar `Core` e `Content`; o núcleo não referencia `UnityEngine` nem componentes de interface.
- Toda escolha passa pela validação do núcleo.
- Entrada fica bloqueada enquanto eventos do turno estão sendo apresentados.
- Ao concluir `Reverter Turno`, a apresentação descarta eventos visuais pendentes da rodada desfeita, redesenha o snapshot restaurado e reabre imediatamente o `ActionMenu` do Mago.
- `Reverter Batalha` segue o mesmo fluxo visual, usando o estado de entrada do encontro.
- Cada orientação usa composição própria; preservar a proporção com barras, nunca deformar a interface.
- Sprites usam filtro point e escala inteira sempre que possível.
- Textos fornecidos pelo jogador terão limite e tratamento de caracteres de controle/rich text.

## Validação visual

- PR de interface inclui captura ou vídeo curto do fluxo alterado.
- Revisão manual usa ambas as resoluções de referência e verifica legibilidade, navegação e estados extremos.
- Testes PlayMode cobrem transições e interações; a aprovação visual continua humana.
- Quando disponível, a pipeline publica uma build como artefato do PR para validação da integração.

## Abertura local

- Abrir a raiz do repositório no Unity Hub com a versão definida em `PROJECT.md`, carregar a cena alterada e iniciar Play Mode.
- Acompanhar o resultado na Game View; Scene e Inspector mostram hierarquia e estado de runtime.
- Em tarefas de UI, o agente abre o projeto e inicia Play Mode quando tiver acesso ao Unity Editor; caso contrário, informa o procedimento ao desenvolvedor.
