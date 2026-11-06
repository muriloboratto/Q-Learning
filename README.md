# Prática de Inteligência Artificial — Q-Learning no Unity

**Disciplina:** Inteligência Artificial  
**Ambiente:** Unity (2D)  
**Tema:** Aprendizado por Reforço (*Reinforcement Learning*)

## 1. Objetivo

Implementar e observar o funcionamento do algoritmo **Q-Learning tabular** em um cenário 2D do Unity. Um **inimigo (agente)** aprende, por tentativa e erro, a alcançar um **herói (objetivo)** que permanece fixo.

Ao final da prática, o estudante deverá compreender a diferença entre **treinamento** e **inferência**, identificar os principais **hiperparâmetros** e interpretar as métricas do aprendizado.

## 2. Conceitos fundamentais

| Conceito | Definição | Na prática |
|---|---|---|
| Agente | Entidade que aprende e toma decisões. | Inimigo. |
| Ambiente | Espaço onde o agente interage. | Grid 2D do Unity. |
| Estado (*state*) | Situação observada pelo agente. | Célula atual do inimigo. |
| Ação (*action*) | Decisão disponível ao agente. | Mover para cima, baixo, esquerda ou direita. |
| Recompensa (*reward*) | Retorno que orienta o aprendizado. | Recompensas e penalidades por movimento. |
| Episódio (*episode*) | Uma tentativa de alcançar o objetivo. | Termina ao alcançar o herói ou atingir o limite de passos. |
| Passo (*step*) | Uma ação realizada no episódio. | Um movimento do inimigo. |
| Época (*epoch*) | Passagem por um conjunto de dados. | Não é utilizada explicitamente neste Q-Learning tabular. |
| Q-Table | Tabela de valores para pares estado–ação. | Armazena os valores aprendidos pelo inimigo. |

## 3. Arquivos e componentes

A prática utiliza:

- `QLearning.cs`: implementa o treinamento, a inferência e a interface de acompanhamento.
- `GridManager`: componente do projeto responsável pelas coordenadas e pela verificação das células transitáveis.
- `Enemy`: objeto do inimigo que se movimenta.
- `Hero`: objeto que representa o objetivo, mantido fixo.

> **Nota:** O script depende de um `GridManager` compatível, com métodos como `WorldToGrid`, `GridToWorld` e `IsWalkable`. Ele não requer o pacote Unity ML-Agents.

## 4. Configuração inicial

1. Abra o projeto no Unity e prepare a cena com o Grid, o inimigo e o herói.
2. Adicione o componente `QLearning` a um GameObject.
3. No **Inspector**, associe as referências **Grid**, **Zombie** (o campo do script que aponta para o inimigo) e **Hero**.
4. Certifique-se de que o inimigo e o herói estejam posicionados em células transitáveis.
5. Confira os hiperparâmetros no Inspector antes de executar a cena.

| Hiperparâmetro | Valor inicial | Função |
|---|---:|---|
| `episodes` | 5000 | Quantidade de episódios de treinamento. |
| `alpha` (α) | 0,15 | Taxa de aprendizado. |
| `gamma` (γ) | 0,95 | Importância das recompensas futuras. |
| `epsilon` (ε) | 0,25 | Probabilidade de escolher uma ação aleatória no treinamento. |
| `maxStepsPerEpisode` | 200 | Limite de passos por episódio. |

**Atenção:** Campos públicos do Unity podem ter valores salvos no Inspector diferentes dos valores iniciais declarados no C#. Confirme o valor efetivo de `episodes` antes do treinamento.

## 5. Como o Q-Learning funciona

A cada passo, o inimigo escolhe uma ação, observa o novo estado e recebe uma recompensa. Em seguida, atualiza o valor correspondente na Q-Table:

\[
Q(s,a) \leftarrow Q(s,a) + \alpha\left[r + \gamma\max_{a'}Q(s',a') - Q(s,a)\right]
\]

Onde `s` é o estado atual, `a` é a ação, `r` é a recompensa e `s'` é o próximo estado. No estado terminal, o valor futuro é considerado zero.

No script da prática, a recompensa é:

| Situação | Recompensa |
|---|---:|
| Alcançar o herói | +100 |
| Tentar mover para uma célula inválida (permanecer no mesmo estado) | −10 |
| Aproximar-se do herói | +0,9 |
| Mover-se sem se aproximar do herói | −1,1 |

A proximidade é calculada pela **distância Manhattan** entre as células.

## 6. Executando a prática

### Etapa A — Treinamento

1. Execute a cena com **Play**.
2. Clique em **TREINAR**.
3. Observe o episódio atual, os passos, o tempo, a recompensa média e a taxa de sucesso.
4. Aguarde o fim dos episódios ou clique em **PARAR TREINAMENTO E INICIAR INFERÊNCIA**.

O modo `visualTraining` permite acompanhar os movimentos do inimigo. Com esse modo desativado, o script processa um episódio por frame, tornando o treinamento normalmente mais rápido.

### Etapa B — Inferência

1. Após o treinamento, clique em **INFERÊNCIA** (ou utilize o botão de interrupção e inferência).
2. Observe o caminho escolhido pelo inimigo para alcançar o herói.
3. Registre a quantidade de movimentos e se o objetivo foi alcançado.
4. Use **REINICIAR APRENDIZADO** para limpar a Q-Table antes de um novo experimento.

Durante a inferência, o script utiliza a melhor ação conhecida na Q-Table, sem a exploração aleatória controlada por `epsilon`.

## 7. Métricas de avaliação

| Métrica | O que representa | Interpretação |
|---|---|---|
| `currentEpisode` | Episódio atual. | Progresso do treinamento. |
| `completedEpisodes` | Episódios concluídos. | Quantidade efetivamente processada. |
| `successes` | Episódios em que o herói foi alcançado. | Quanto maior, melhor, nas mesmas condições. |
| `successRate` | Percentual de sucessos no treinamento. | Quanto maior, melhor. |
| `averageReward` | Recompensa acumulada média por episódio. | Comparar apenas com a mesma função de recompensa. |
| `trainingSeconds` | Tempo de treinamento. | Custo de execução. |
| `currentStep` | Passo atual; na inferência, quantidade de movimentos executados. | Menos movimentos é desejável quando o objetivo é alcançado. |

> A taxa de sucesso exibida pelo script é calculada **durante o treinamento**. Para avaliar a política aprendida, observe também a inferência e, idealmente, repita os testes.

## 8. Experimento: ajustando hiperparâmetros

Mantenha o cenário e a quantidade de episódios constantes. Varie **um hiperparâmetro por vez**.

| Experimento | `alpha` | `gamma` | `epsilon` | `episodes` |
|---|---:|---:|---:|---:|
| A | 0,05 | 0,95 | 0,25 | 5000 |
| B | 0,15 | 0,95 | 0,25 | 5000 |
| C | 0,30 | 0,95 | 0,25 | 5000 |
| D | 0,50 | 0,95 | 0,25 | 5000 |

Registre os resultados:

| Experimento | Taxa de sucesso (%) | Recompensa média | Tempo (s) | Movimentos na inferência | Alcançou o herói? |
|---|---:|---:|---:|---:|---|
| A | — | — | — | — | — |
| B | — | — | — | — | — |
| C | — | — | — | — | — |
| D | — | — | — | — | — |

**Importante:** repita cada configuração algumas vezes, pois o treinamento contém escolhas aleatórias. Reinicie o aprendizado antes de cada execução. Os valores da tabela são sugestões de experimento, não resultados medidos.

## 9. Questões para discussão

1. Qual é a diferença entre episódio e passo?
2. Por que uma época não é necessária nesta implementação tabular?
3. O que acontece quando aumentamos `epsilon`?
4. Como `alpha` e `gamma` influenciam as atualizações da Q-Table?
5. Uma alta taxa de sucesso no treinamento garante um bom resultado na inferência? Justifique.
6. Qual configuração produziu o melhor resultado no seu experimento? Quais métricas sustentam sua conclusão?

## 10. Resultado esperado

Ao final, o estudante deverá ser capaz de explicar como o inimigo aprende a selecionar ações a partir de recompensas, distinguir treinamento de inferência e analisar criticamente o efeito dos hiperparâmetros sobre o comportamento do agente.

---