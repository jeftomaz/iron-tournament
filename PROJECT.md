# Iron Tournament

## Objetivo

RPG 2D de combate por turnos em Unity, com apresentação inspirada em RPGs 16-bit. A primeira etapa não inclui exploração.

## Referência

- `ironturn/`: especificação funcional em Java; suas regras serão revisadas, não portadas literalmente.
- `assets/`: sprites direcionais estáticos para personagens.

## Stack

- Unity `6000.5.7f1`
- C#
- Projeto 2D pixel-perfect
- Sem dependências externas aprovadas

## Premissas confirmadas

- Combates frontais e sequenciais, inicialmente `1 x 1`.
- A campanha segue a ordem fixa: Goblin, Esqueleto, Cavaleiro, Lobisomem, Vampiro, Necromante e Rei Demônio.
- Atributos-base dos inimigos são fixos para preservar a curva de dificuldade.
- Variação de dano, crítico, penetração e drops permanecem probabilísticos.
- Regras de combate independentes da interface e das animações.
- Configurações em `ScriptableObject`; estado da partida em objetos de runtime.
- O núcleo deve ser testável sem carregar cenas.
- O protótipo Java é referência de design, não fonte de verdade para comportamentos defeituosos.

## Propostas aguardando confirmação

- Reversão restaura o estado completo, exceto a sequência aleatória.
- Arquitetura idiomática para Unity, sem preservar os padrões GoF apenas por equivalência acadêmica.
- Primeiro marco: Guerreiro contra Goblin, com fluxo completo e testes.

## Restrições

- Não implementar exploração no escopo inicial.
- Não instalar dependências sem aprovação.
- Não colocar regras de combate em componentes de UI ou animação.
