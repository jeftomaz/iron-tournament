# Roadmap

Responsáveis: `JT` Jeferson Tomaz, `JL` João Lucas, `JP` João Pedro. Cada item deve ser entregue em PR próprio; itens dependentes começam após o contrato anterior entrar na `main`.

## Fase 0 — Base do projeto

- `done` `JT` Inicializar Unity, assemblies `Core/Content/Presentation`, testes EditMode e pipeline WebGL.
- `done` `JP` Criar cenários estáticos de batalha para heróis e inimigos; a integração no Unity permanece pendente.
- `done` `JT` Proteger a `main` com PR obrigatória e checks de testes EditMode e build WebGL em contêiner GameCI.
- `done` `JT` Definir tipos de `Content`, validadores e adaptação para os modelos puros do `Core`.
- `doing` `JT` Manter o mapa de arquitetura, contratos e decisões estruturais para a integração e a entrega final.
- `done` `JL` Implementar contratos fundamentais: estados, fases, ações, eventos, resultados e fonte aleatória injetável.
- `done` `JP` Preparar cena-base, importação pixel-perfect, prefab compartilhado dos combatentes e composições `360x640` e `1280x720`.

## Fase 1 — Vertical Guerreiro x Goblin

- `done` `JL` Implementar ataque físico, defesa temporária, reflexão, crítico de 7%, penetração de 10%, resposta do Goblin, vitória e derrota; cobrir regras e extremos em EditMode.
- `doing` `JP` Completar apresentação Guerreiro/Goblin: componentes, ligação ao `IBattle`, assets de conteúdo e cena no build preparados; pendente validação do fluxo com bootstrap.
- `todo` `JT` Compor configurações Guerreiro/Goblin, ligar cena ao núcleo e validar o fluxo na CI e no build WebGL.

## Fase 2 — Mago e reversão

- `todo` `JL` Implementar ataque que ignora defesa, carga compartilhada por encontro, snapshots completos de turno/batalha e continuidade da aleatoriedade; testar ataque A, resposta B, reversão para antes de A e nova escolha do Mago.
- `todo` `JL` Cobrir por testes a restauração de HP, atributos, efeitos, consumíveis, fase, flags e histórico, além da proibição após morte.
- `todo` `JP` Apresentar as duas reversões, cancelar eventos visuais desfeitos, redesenhar o estado restaurado e devolver o controle sem reproduzir a resposta inimiga.
- `todo` `JT` Integrar os checkpoints à `Presentation` e revisar isolamento entre estado restaurável e fonte aleatória.

## Fase 3 — Campanha, inimigos e itens

- `todo` `JL` Implementar a sequência fixa dos sete inimigos, variação de ±15% nos atributos por encontro, variação de dano, fúria de Vampiro/Necromante/Rei Demônio e transição entre encontros.
- `todo` `JL` Implementar drops ponderados, escolha entre dois tipos distintos, melhorias permanentes e itens exclusivos: Pergaminho, Manto de Chamas e Chifre da Irmandade.
- `todo` `JP` Configurar assets dos sete inimigos e produzir telas/animações de transição, fúria, saque, itens, vitória e derrota nas duas orientações.
- `todo` `JT` Criar e validar os assets `ScriptableObject` canônicos, garantindo Mago equipado com 30 ATK e faixas válidas para a variação dos inimigos.

## Fase 4 — Progresso e modo inimigo

- `todo` `JL` Implementar `ProgressData`, conclusão por classe, desbloqueio após Guerreiro e Mago, snapshot final com melhorias e regras do modo inimigo por tiers.
- `todo` `JT` Implementar save local versionado, validação de faixas, escrita atômica compatível com WebGL e testes de corrupção/migração.
- `todo` `JP` Implementar seleção de classe, indicadores de progresso, desbloqueio, seleção de inimigo, drop pré-boss e apresentação do herói final.
- `todo` `JT` Integrar campanha, persistência e modo inimigo sem expor save ou alteração de estado pela UI.

## Fase 5 — Fechamento

- `todo` `JP` Completar arte, animações, áudio e testes PlayMode; anexar evidência visual mobile/desktop aos PRs.
- `todo` `JL` Executar testes de regressão e ajustar balanceamento apenas por valores canônicos aprovados.
- `todo` `JT` Publicar e validar o build Web nos navegadores-alvo, revisar segurança, desempenho, CI e integração final.
