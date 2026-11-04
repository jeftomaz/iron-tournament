# ART STYLE GUIDE — RPG Dark Fantasy 16 Bits

**Versão:** 1.0<br>
**Status:** Guia inicial de referência visual<br>
**Escopo:** personagens, inimigos, NPCs, criaturas, cenários, objetos, interfaces ilustradas e efeitos visuais.

> Este guia foi desenvolvido a partir de oito sprites de referência fornecidos para o projeto. As cores abaixo são uma **paleta normalizada e proposta**, não uma amostragem pixel a pixel dos arquivos. Famílias adicionais contemplam ambientes e efeitos ainda não representados nas referências.

## 1. Identidade visual

- **Gênero:** fantasia medieval sombria (*dark fantasy*).
- **Linguagem:** pixel art inspirada na era de 16 bits, com silhuetas legíveis, volumes construídos por blocos de pixels e sombreamento deliberado.
- **Clima:** atmosfera sombria, materiais usados/envelhecidos, predominância de neutros frios e tons terrosos, contrastados com fontes pontuais de luz e magia.
- **Hierarquia visual:** personagens e pontos narrativos devem se destacar de cenários e elementos secundários.
- **Princípio:** consistência se obtém por famílias cromáticas, distribuição de valores, iluminação e tratamento de materiais — não pela proibição de toda cor intermediária.

## 2. Como ler a paleta

Cada família apresenta cinco referências:

| Tom | Função | Orientação |
|---|---|---|
| **T1 — Profundo** | Cavidades, regiões de maior oclusão | Não precisa ocupar toda a sombra. |
| **T2 — Sombra** | Planos voltados para longe da luz | Principal separação da base. |
| **T3 — Base** | Identidade cromática do material | Cor dominante da superfície. |
| **T4 — Luz** | Faces iluminadas | Ajuda a definir volumes. |
| **T5 — Brilho** | Reflexos e acentos | Usar seletivamente, sem preencher superfícies inteiras. |

**Importante:** a progressão de tons não é uma fórmula universal de luminosidade. Os valores de cada família foram escolhidos artisticamente; a seleção real depende da iluminação da cena. As cores podem ser interpoladas ou ajustadas para uma fonte de luz específica.

## 3. Paleta cromática oficial — 16 famílias / 80 referências

### A. Neutros e materiais

| ID | Família | T1 — Profundo | T2 — Sombra | T3 — Base | T4 — Luz | T5 — Brilho | Aplicações |
|---|---|---|---|---|---|---|---|
| 01 | Preto e sombras | `#08080C` | `#14131B` | `#24212B` | `#37313C` | `#514852` | Contornos, cavidades, escuridão |
| 02 | Aço frio | `#1D202B` | `#303440` | `#50555E` | `#777D84` | `#AFB5B7` | Armaduras, espadas, estruturas metálicas |
| 03 | Prata e aço claro | `#32343C` | `#575D66` | `#858B91` | `#BBC0C2` | `#E1E4DF` | Lâminas, reflexos, metais claros |
| 04 | Ossos e marfim | `#40363A` | `#70625D` | `#A3937E` | `#D4C5A7` | `#F3E9C8` | Ossos, dentes, chifres, pergaminhos |

### B. Orgânicos e terrosos

| ID | Família | T1 — Profundo | T2 — Sombra | T3 — Base | T4 — Luz | T5 — Brilho | Aplicações |
|---|---|---|---|---|---|---|---|
| 05 | Pele humana | `#3D292B` | `#70443B` | `#A5745A` | `#D39B76` | `#F0C49A` | Guerreiros, aldeões, crianças; adaptar a diferentes tons de pele |
| 06 | Couro e madeira | `#291D21` | `#48302C` | `#74503D` | `#A16F4C` | `#C79567` | Botas, cintos, armas, troncos |
| 07 | Terra e pedra | `#242329` | `#3D3838` | `#625A53` | `#8C8171` | `#B6A995` | Solo, ruínas, cavernas, caminhos |
| 08 | Verde musgo | `#20271E` | `#3C492C` | `#656F3D` | `#929D5E` | `#BDC18A` | Goblins, musgo, raízes e vegetação |
| 09 | Verde florestal | `#142221` | `#243C33` | `#3B5B48` | `#62846A` | `#94AB86` | Folhas, florestas e pântanos |

### C. Sobrenaturais e destaques

| ID | Família | T1 — Profundo | T2 — Sombra | T3 — Base | T4 — Luz | T5 — Brilho | Aplicações |
|---|---|---|---|---|---|---|---|
| 10 | Vermelho sombrio | `#28151E` | `#55242A` | `#8A3435` | `#BE4D46` | `#E77D60` | Demônios, tecidos, sangue e ferimentos estilizados |
| 11 | Fogo e brasas | `#442020` | `#86351F` | `#D94D1C` | `#F68A2B` | `#FFD17A` | Chamas, ataques ígneos, luz quente |
| 12 | Ouro envelhecido | `#392C25` | `#6D522F` | `#A27B40` | `#D3AD60` | `#F3DB98` | Relíquias, ornamentos, símbolos |
| 13 | Roxo arcano | `#21182E` | `#402D50` | `#6B4C79` | `#A078AB` | `#D2B0DF` | Necromancia, vestes e efeitos mágicos |
| 14 | Azul espectral | `#142630` | `#264655` | `#397386` | `#64A5AF` | `#B2DEE0` | Espíritos, magia fria, brilhos sobrenaturais |

### D. Extensões para cenários

| ID | Família | T1 — Profundo | T2 — Sombra | T3 — Base | T4 — Luz | T5 — Brilho | Aplicações |
|---|---|---|---|---|---|---|---|
| 15 | Água e umidade | `#121F25` | `#24353A` | `#39565B` | `#5A7D80` | `#91ADA9` | Poças, pântanos, rios, superfícies úmidas |
| 16 | Céu e névoa | `#1A1D2D` | `#30374B` | `#535D70` | `#818C9D` | `#B8BBC0` | Luar, atmosfera, céu e profundidade |

**Não tratar os 80 códigos como uma restrição absoluta.** São pontos de ancoragem para novas artes. Acrescentar uma cor é aceitável quando ela respeita a lógica de luz, valor, saturação e família, ou quando uma necessidade narrativa justifica um novo destaque.

## 4. Regras cromáticas globais

### 4.1 Escuridão e contraste

1. Preferir sombras coloridas e profundas a grandes massas de preto absoluto.
2. Reservar o menor valor de luminosidade para cavidades, separações estruturais e regiões de maior oclusão.
3. Garantir leitura de silhueta em escala real de jogo; não depender exclusivamente de detalhes internos.
4. Separar personagens do fundo por **diferença de valor, temperatura ou borda**, mesmo quando compartilham a mesma cor-base.
5. Usar os realces T5 com parcimônia. Nem toda superfície precisa chegar ao tom mais claro.

### 4.2 Saturação

1. Couro, pedra, madeira, tecido comum e vegetação: priorizar saturação baixa a moderada.
2. Fogo, feitiços, olhos sobrenaturais e marcas narrativas: permitir saturação mais intensa.
3. Um personagem importante pode conter mais cor de destaque que um NPC comum, sem necessidade de cores novas.
4. Reduzir a competição visual de elementos secundários, especialmente nos planos de fundo.

### 4.3 Proporção visual indicativa

Como ponto de partida para uma composição sombria, considerar aproximadamente **65% de tons neutros/escuros, 25% de cores de materiais e 10% de destaques**. Isso **não é uma regra matemática** nem se aplica indistintamente a cada personagem: um demônio em chamas pode ter uma proporção maior de vermelho e laranja.

### 4.4 Contornos e acabamento

- Usar contorno escuro seletivo para preservar separação visual, sem exigir contorno preto uniforme em toda forma.
- Manter agrupamentos coerentes de pixels; evitar ruído, pontos dispersos e microdetalhamento sem função.
- Evitar antialiasing suave, borrão, gradientes contínuos e texturas fotográficas.
- Evitar sombras geradas apenas por sobreposição aleatória de pixels (*pillow shading*).

## 5. Luz e interação entre famílias

A família cromática representa a **cor local** do material. A luz ambiente pode alterar sua aparência.

**Exemplo — aço próximo ao fogo:**

- Base: aço frio, família 02 (`#50555E`).
- Sombra: aço T1–T2, mais escuro e frio.
- Faces iluminadas pela chama: deslocamento controlado para laranja/ouro (famílias 11 e 12), em vez de apenas aumentar o brilho do cinza.
- Reflexos de metal: pequenos pixels T4–T5, concentrados em arestas e planos diretamente iluminados.

**Regras:**

1. Definir uma fonte principal de luz por cena, além de eventuais fontes secundárias.
2. Manter consistente a direção da luz nos personagens e no cenário.
3. Luz quente (tocha, fogo) aquece superfícies próximas; luz fria (lua, magia espectral) desloca as superfícies para azuis/cinzas.
4. Aplicar a cor emissiva de forma localizada, com queda de intensidade à distância.
5. Nem todo material reflete a luz da mesma maneira: metal reflete pontualmente; tecido e madeira têm transições mais amplas e discretas.
6. O brilho não deve eliminar a legibilidade dos pixels.

## 6. Materiais e personagens

| Elemento | Famílias prioritárias | Orientação |
|---|---|---|
| Guerreiros e cavaleiros | 01, 02, 03, 05, 06, 12 | Metais frios, couro escuro e reflexos controlados. |
| Humanos, aldeões e crianças | 01, 05, 06, 07, 04 | Silhuetas simples; tons de pele e vestimentas devem variar sem perder unidade estilística. |
| Goblins e criaturas da floresta | 01, 08, 09, 06, 04 | Verdes terrosos, detalhes de osso e couro. |
| Esqueletos e mortos-vivos | 01, 04, 07, 13, 14 | Ossos marfim envelhecidos, sombras frias e magia pontual. |
| Magos e necromantes | 01, 06, 13, 14, 11 | Tecidos de baixa saturação contrastando com efeitos brilhantes. |
| Demônios e chefes | 01, 10, 11, 12, 04 | Vermelhos profundos, chifres e áreas incandescentes; preservar leitura de grandes massas. |
| Armas e escudos | 02, 03, 06, 12 | Reflexos concentrados nos metais; bordas visíveis. |

### Variações de pele e criaturas

A família 05 é **uma referência, não uma lista exaustiva de tons de pele**. Humanos com diferentes tons de pele devem ter rampas próprias, construídas com a mesma lógica de sombra/base/luz. O mesmo vale para criaturas cuja identidade exija outra cor local. Não forçar toda pele ao mesmo conjunto de cinco HEX.

## 7. Cenários e profundidade

### 7.1 Primeiro plano

- Maior nitidez de borda e separação entre objetos.
- Contraste próximo ao dos personagens apenas nos elementos importantes.
- Sombras mais definidas e texturas moderadas.

### 7.2 Plano de combate

- Usar predominantemente famílias 07–09, 15 e 16, conforme o bioma.
- Preservar áreas visualmente limpas onde personagens e efeitos serão sobrepostos.
- Evitar mosaicos de alto contraste sob os pés dos combatentes.
- A sombra de contato e a luz incidente devem integrar o personagem ao chão.

### 7.3 Fundo distante

- Reduzir contraste e saturação de detalhes.
- Aproximar os tons da cor atmosférica da cena (16 e, quando apropriado, 14/15).
- Diminuir a frequência de bordas claras e texturas pequenas.
- Separar profundidade por grupos de valores, sem depender de desfoque.

### 7.4 Exemplos por ambiente

| Ambiente | Base | Destaque | Observação |
|---|---|---|---|
| Floresta noturna | 01, 09, 07, 16 | 08, 14 | Vegetação escura e profundidade fria. |
| Pântano | 01, 08, 09, 15 | 14, 11 | Água turva, raízes e reflexos pontuais. |
| Cemitério/ruínas | 01, 07, 04, 16 | 13, 14 | Pedras frias e sinais sobrenaturais. |
| Caverna | 01, 07, 06 | 11, 12 | Grande domínio de sombras, pontos quentes locais. |
| Castelo/masmorra | 01, 02, 07, 06 | 11, 12, 13 | Pedra e metal, iluminação arquitetônica. |
| Área infernal | 01, 07, 10, 11 | 12 | A luz das brasas deve tingir materiais vizinhos. |

## 8. Fogo, magia e outros efeitos

- **Fogo:** família 11 como núcleo cromático; áreas mais brilhantes pequenas e concentradas. Não usar somente laranja uniforme.
- **Magia necromântica:** família 13, opcionalmente combinada com 14 para diferenciação da energia.
- **Magia espectral:** família 14; brilho frio localizado.
- **Sangue:** família 10, preferencialmente evitando vermelhos extremamente saturados em superfícies comuns.
- **Ouro/efeitos sagrados ou relíquias:** família 12 com T5 seletivo.
- **Partículas:** baixa densidade e distribuição deliberada; não encobrir a forma principal.
- **Efeitos especiais temporários:** podem exceder os níveis de saturação comuns; manter o contraste narrativo sem alterar permanentemente a identidade cromática da cena.

## 9. Regras técnicas para pixel art

1. **Grade nativa absoluta:** um pixel lógico é a menor unidade de desenho do sprite.
2. Se um sprite original tiver 48 × 48 pixels, preservar **exatamente 48 × 48 posições lógicas** em cada pose ou frame correspondente.
3. Ampliação para exibição não autoriza meios-pixels, detalhes subpixel, novos contornos finos ou pixels intermediários.
4. Escalar pixel art por multiplicadores inteiros sempre que possível, com filtragem *nearest-neighbor*.
5. Manter proporções, características de silhueta e quantidade de detalhe comparáveis entre personagens de mesma categoria.
6. Evitar efeitos de pós-processamento que tornem as bordas borradas.
7. Uma mesma personagem pode usar ajustes de luz por cenário, mas não deve alterar arbitrariamente sua paleta local em cada pose.
8. Criar animações e poses com mudanças legíveis de grandes formas, não refinando o sprite com novos detalhes que não existiam em sua resolução original.

## 10. Fluxo para criar novos personagens e cenários

1. **Definir função e contexto:** personagem, inimigo, objeto ou ambiente; local, material e iluminação.
2. **Escolher famílias:** 2–4 predominantes por elemento, com adicionais para efeitos especiais.
3. **Planejar valores:** determinar as massas claras, médias e escuras antes de adicionar texturas.
4. **Fixar luz:** selecionar a direção, temperatura e intensidade das fontes.
5. **Criar silhueta:** validar leitura no tamanho final do jogo.
6. **Aplicar cor:** base T3, sombras T1–T2 e luz T4–T5 conforme material.
7. **Adicionar acentos:** usar destaque pontual onde houver importância narrativa ou material emissivo.
8. **Testar no cenário:** verificar contraste com o fundo e coerência da iluminação.
9. **Revisar grade lógica:** impedir detalhes ou antialiasing incompatíveis com a resolução nativa.
10. **Registrar desvios:** anotar famílias novas ou exceções justificadas para futura revisão deste guia.

## 11. Checklist de aprovação

- [ ] O resultado comunica fantasia medieval sombria?
- [ ] A silhueta é legível na escala real do jogo?
- [ ] As cores locais pertencem às famílias da paleta ou são extensões justificáveis?
- [ ] Sombras, base e reflexos possuem hierarquia clara?
- [ ] Fogo, magia e brilhos têm destaque sem dominar elementos não relevantes?
- [ ] A iluminação do personagem é coerente com a do cenário?
- [ ] O fundo não compete com o foco narrativo?
- [ ] Os materiais são reconhecíveis pela combinação de cor e textura?
- [ ] O desenho respeita a grade nativa de pixels?
- [ ] Não existem bordas borradas, gradientes suaves ou microdetalhes indevidos?

## 12. Prompt-base reutilizável

> Criar arte de RPG **dark fantasy** em pixel art inspirada nos 16 bits, consistente com a paleta cromática oficial do projeto. Usar famílias de sombras profundas, materiais envelhecidos, neutros frios e tons terrosos. Definir volumes por blocos nítidos de pixels, aproximadamente 3 a 5 níveis de luminosidade por material, com iluminação coerente com a cena. Reservar vermelho, laranja, dourado, roxo e azul espectral intensos a focos narrativos, fogo, magia e reflexos. Manter silhueta legível na escala nativa, sem gradientes suaves, borrão, antialiasing ou pixels intermediários. Preservar o tamanho da grade lógica original ao criar variações e poses.

**Para usar o prompt:** complementar com a descrição do personagem/cenário, enquadramento, dimensões lógicas, perspectiva, pose, transparência do fundo e as famílias prioritárias. Quando necessário, fornecer as linhas HEX pertinentes da tabela da seção 3 como referência explícita.

## 13. Política de evolução

- Esta é a **versão inicial**, sujeita a refinamento após testes lado a lado entre sprites e cenários.
- Uma família cromática nova deve ter função visual clara; evitar criar famílias redundantes para diferenças mínimas.
- As alterações na paleta devem preservar referências existentes sempre que possível, para não provocar inconsistências entre artes já aprovadas.
- Se uma cor nova aparecer recorrentemente e agregar identidade ao jogo, considerar incorporá-la formalmente à próxima versão.
- Recomenda-se manter uma imagem de *color swatches* e cenas-teste como material complementar a este Markdown.

---

**Resumo de direção:** padronizar famílias cromáticas, sombra, luz, legibilidade e uso de destaques. A paleta orienta as decisões artísticas sem limitar rigidamente todos os pixels a 80 códigos HEX.
