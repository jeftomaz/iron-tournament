# IronTurn — Plano de Construção

## Ordem de Desenvolvimento

A regra é simples: **cada fase só depende do que já foi feito antes.**

---

### ✅ Fase 1 — Camada `model/`
Fundação do jogo.

- [x] `Character.java` — classe base abstrata (hp, atk, def, takeDamage)
- [x] `Hero.java` — personagem jogável; suporte a rage, guardian, scroll, flameCloak, inventário
- [x] `Enemy.java` — inimigo com variação de stats e triggerSpecial (fúria ao atingir limiar de HP)
- [x] `HeroClass.java` — enum com stats canônicos por classe (equippedAtk, etc.)
- [x] `HeroSnapshot.java` — record imutável para transferir stats entre modos de jogo

---

### ✅ Fase 2 — Padrão `strategy/`
Define *como* cada classe ataca.

- [x] `AttackStrategy.java` — interface
- [x] `WarriorAttack.java` — golpe físico com chance de crítico e penetração de armadura
- [x] `MageAttack.java` — magia que ignora defesa

---

### ✅ Fase 3 — Padrão `observer/`
Define *quem é notificado* quando algo acontece na batalha.

- [x] `BattleObserver.java` — interface
- [x] `BattleEvent.java` — objeto imutável de evento; tipos: ATTACK, GUARD, BURN_DAMAGE
- [x] `BattleLogger.java` — registra histórico completo (todos os tipos)
- [x] `StatusDisplay.java` — exibe barras de HP; ignora BURN_DAMAGE (fix pendente — adicionar early return)

---

### ✅ Fase 4 — Padrão `decorator/`
Adiciona atributos via equipamentos sem herança.

- [x] `CharacterDecorator.java` — base abstrata com `getWrapped()`
- [x] `SwordDecorator.java` — +10 ATK
- [x] `ShieldDecorator.java` — +8 DEF
- [x] `AmuletDecorator.java` — +15 ATK, +30 HP máximo
- [x] `FlameCloakDecorator.java` — burn de 5 HP/ação ao inimigo (drop exclusivo do Mago)

---

### ✅ Fase 5 — Padrão `command/`
Encapsula ações do turno como objetos (com undo).

- [x] `TurnCommand.java` — interface com `execute()` e `undo()`
- [x] `AttackCommand.java` — ataque + notifica observers; suporte a undo
- [x] `GuardCommand.java` — guerreiro levanta escudo; dobra DEF + dano reflexivo; `revert()` restaura DEF
- [x] `CommandHistory.java` — pilha LIFO para undo

---

### ✅ Fase 6 — Sistema de itens `model/item/`
Drops pós-batalha aplicados ao herói.

- [x] `Item.java` — interface (`apply`, `getName`, `getDescription`)
- [x] `HopeScroll.java` — cura +40% maxHp (Guerreiro, uso único, ≤30% HP); descartável
- [x] `BrotherhoodHorn.java` — ativa guardianArmed no Mago: intercepção fatal pelo Guerreiro
- [x] `FlameCloakItem.java` — sinaliza `flameCloakPending`; engine troca para `FlameCloakDecorator`
- [x] `DropTable.java` — `roll(HeroClass)` e `rollBase(n)` para drops contextuais e do modo inimigo

---

### ✅ Fase 7 — `battle/UI.java`
Camada de apresentação — independente de lógica de jogo.

- [x] Frames coloridos via `FramedOutputStream` (Decorator sobre `OutputStream`)
- [x] `frameOpen` / `frameClose` — redireciona `System.out` automaticamente
- [x] `buildBar` — barra de HP com ANSI colorido
- [x] Helpers: `section`, `separator`, `turnDivider`, `titleScreen`, `pause`

---

### 🔧 Fase 8 — `battle/BattleEngine.java`
Orquestra o loop de turnos. Depende de tudo acima.

- [x] Loop principal: jogador age → inimigo age → repete
- [x] Menu dinâmico por classe (Guerreiro: defender, scroll; Mago: reverter, guardian)
- [x] Seleção de herói no início (Guerreiro ou Mago)
- [x] Sequência de 7 inimigos em ordem
- [x] Drops pós-inimigo com aplicação de equipamentos via Decorator
- [x] Modo inimigo (`playAsEnemy`) — roster selecionável, inimigos em tier, boss = herói do jogador
- [x] Fúria no modo inimigo (rage ao atingir ≤30% HP, consome turno)
- [x] Drop pré-boss no modo inimigo (3 itens via `rollBase`)
- [x] Mecânica guardian (BrotherhoodHorn): intercepção fatal, cura ou sobrevivência com 1 HP
- [ ] **Bug** — `applyBurn()` chamado fora do frame em `playerTurn()` (fix: mover antes de `frameClose`)
- [ ] **Refatoração** — extrair `BattleSetup` e `BattleUI` (engine está grande demais)
- [ ] Mensagem de fim de jogo diferenciada para modo inimigo

---

### 🕐 Fase 9 — Novos padrões GoF
Introduzidos organicamente, não forçados.

- [ ] **Builder** — `HeroBuilder` substituindo o `switch` em `createHero()`
- [ ] **State** — `PlayerTurnState`, `EnemyTurnState`, `DropPhase` para organizar o loop

---

### 🕑 Fase 10 — `Main.java`
Ponto de entrada. Orquestra os dois modos de jogo.

- [x] Modo herói: instancia `BattleEngine` e chama `start()`
- [x] Modo inimigo: passa `HeroSnapshot` para segundo `BattleEngine`
- [x] Loop de rematch / troca de modo

---

### 🕒 Fase 11 — Documentação

- [ ] `README.md` — atualizar com FlameCloakDecorator, BrotherhoodHorn, modo inimigo, UML
- [ ] `LICENSE` (MIT)

---

## Compilação (referência rápida)

```bash
# A partir da raiz do projeto
find src -name "*.java" > sources.txt
javac -d out @sources.txt
java -cp out ironturn.Main
```

---

## O que está fora do escopo
- Persistência (save/load)
- Mapa ou movimentação
- Inventário com progressão de nível
- Frameworks externos