using System.Collections;
using UnityEngine;

/// <summary>
/// Q-Learning tabular para o GridManager existente (14x9 por padrao).
/// O estado representa a celula do inimigo; o heroi permanece fixo.
/// Nao requer ML-Agents nem GridVisualizer.
/// </summary>
public class QLearning : MonoBehaviour
{
    [Header("Scene References")]
    public GridManager grid;
    public Transform zombie; // enemy
    public Transform hero;   // hero

    [Header("Q-Learning")]
    [Min(1)] public int episodes = 5000;
    [Range(0.001f, 1f)] public float alpha = 0.15f;
    [Range(0f, 1f)] public float gamma = 0.95f;
    [Range(0f, 1f)] public float epsilon = 0.25f;
    [Min(1)] public int maxStepsPerEpisode = 200;

    [Header("Visualization")]
    public bool visualTraining = true;
    [Min(0f)] public float trainingDelay = 0.05f;
    [Min(0f)] public float inferenceDelay = 0.25f;
    [Min(1)] public int maxInferenceSteps = 200;
    public bool showOnGUI = true;

    [Header("Live Statistics (read only)")]
    public int currentEpisode;
    public int currentStep;
    public int successes;
    public float episodeReward;
    public float averageReward;
    public float successRate;
    public float trainingSeconds;
    public int completedEpisodes;

    private float[,] q;
    private int startState;
    private int goalState;
    private int currentState;
    private bool isTraining;
    private bool isInferring;
    private bool isTrained;
    private bool initialized;
    private Coroutine running;
    private string status = "Aguardando treinamento";
    private float trainingStartTime;
    private bool hasLearnedSteps;

    // 0: cima, 1: baixo, 2: esquerda, 3: direita
    private static readonly int[] Dx = { 0, 0, -1, 1 };
    private static readonly int[] Dy = { 1, -1, 0, 0 };

    private void Start()
    {
        if (grid == null || zombie == null || hero == null)
        {
            Debug.LogError("QLearning: associe Grid, Enemy e Hero no Inspector.", this);
            enabled = false;
            return;
        }

        if (grid.width <= 0 || grid.height <= 0)
        {
            Debug.LogError("QLearning: dimensões inválidas no GridManager.", this);
            enabled = false;
            return;
        }

        Vector2Int startCell = grid.WorldToGrid(zombie.position);
        Vector2Int goalCell = grid.WorldToGrid(hero.position);
        if (!grid.IsWalkable(startCell) || !grid.IsWalkable(goalCell))
        {
            Debug.LogError(
                "QLearning: Enemy e Hero devem estar em células transitáveis. " +
                $"Enemy={startCell}, Hero={goalCell}.", this);
            enabled = false;
            return;
        }

        startState = ToState(startCell);
        goalState = ToState(goalCell);
        q = new float[grid.width * grid.height, 4];
        initialized = true;
        ResetLearning();
    }

    private int ToState(Vector2Int cell)
    {
        return cell.y * grid.width + cell.x;
    }

    private Vector2Int ToCell(int state)
    {
        return new Vector2Int(state % grid.width, state / grid.width);
    }

    private int NextState(int state, int action)
    {
        Vector2Int cell = ToCell(state);
        Vector2Int nextCell = cell + new Vector2Int(Dx[action], Dy[action]);
        return grid.IsWalkable(nextCell) ? ToState(nextCell) : state;
    }

    private int Distance(int a, int b)
    {
        Vector2Int ca = ToCell(a);
        Vector2Int cb = ToCell(b);
        return Mathf.Abs(ca.x - cb.x) + Mathf.Abs(ca.y - cb.y);
    }

    private float Reward(int state, int next)
    {
        if (next == goalState) return 100f;
        if (next == state) return -10f;

        // Pequena penalidade por passo, com recompensa por aproximar-se.
        return -0.1f + (Distance(next, goalState) < Distance(state, goalState) ? 1f : -1f);
    }

    private int BestAction(int state)
    {
        // Desempate aleatorio para evitar preferencia fixa por "cima".
        int best = 0;
        float bestValue = q[state, 0];
        int ties = 1;
        for (int action = 1; action < 4; action++)
        {
            float value = q[state, action];
            if (value > bestValue + 0.00001f)
            {
                best = action;
                bestValue = value;
                ties = 1;
            }
            else if (Mathf.Abs(value - bestValue) <= 0.00001f)
            {
                ties++;
                if (Random.Range(0, ties) == 0) best = action;
            }
        }
        return best;
    }

    private void ShowState(int state)
    {
        currentState = state;
        Vector3 world = grid.GridToWorld(ToCell(state));
        world.z = zombie.position.z; // preserva a camada visual do sprite
        zombie.position = world;
    }

    public void StartTraining()
    {
        if (!initialized || isTraining || isInferring) return;
        isTrained = false;
        hasLearnedSteps = false;
        trainingSeconds = 0f;
        trainingStartTime = Time.realtimeSinceStartup;
        running = StartCoroutine(TrainCoroutine());
    }

    private IEnumerator TrainCoroutine()
    {
        isTraining = true;
        successes = 0;
        completedEpisodes = 0;
        currentEpisode = 0;
        currentStep = 0;
        averageReward = 0f;
        successRate = 0f;
        float totalReward = 0f;
        status = visualTraining ? "Treinando (visual)" : "Treinando (rápido)";

        for (int ep = 0; ep < episodes; ep++)
        {
            int state = startState;
            currentEpisode = ep + 1;
            currentStep = 0;
            episodeReward = 0f;
            if (visualTraining) ShowState(state);

            for (int step = 0; step < maxStepsPerEpisode; step++)
            {
                currentStep = step + 1;
                int action = Random.value < epsilon ? Random.Range(0, 4) : BestAction(state);
                int next = NextState(state, action);
                bool done = next == goalState;
                float reward = Reward(state, next);
                episodeReward += reward;

                float future = done ? 0f : q[next, BestAction(next)];
                q[state, action] += alpha * (reward + gamma * future - q[state, action]);
                hasLearnedSteps = true;
                state = next;

                if (visualTraining)
                {
                    ShowState(state);
                    if (trainingDelay > 0f)
                        yield return new WaitForSeconds(trainingDelay);
                    else
                        yield return null;
                }

                if (done)
                {
                    successes++;
                    break;
                }
            }

            totalReward += episodeReward;
            completedEpisodes++;
            averageReward = totalReward / completedEpisodes;
            successRate = 100f * successes / completedEpisodes;

            // No modo rápido, libera o frame ao fim de cada treinamento.
            if (!visualTraining) yield return null;
        }

        trainingSeconds = Time.realtimeSinceStartup - trainingStartTime;
        ShowState(startState);
        isTraining = false;
        isTrained = hasLearnedSteps;
        running = null;
        status = "Treinamento concluído. Execute a inferência.";
        Debug.Log($"Q-Learning: {successes}/{episodes} sucessos ({successRate:F1}%). " +
                  $"Recompensa média: {averageReward:F2}", this);
    }

    // Interrompe o treinamento sem apagar a Q-Table e inicia a inferencia.
    public void StopTrainingAndInfer()
    {
        if (!initialized || !isTraining || isInferring) return;

        trainingSeconds = Time.realtimeSinceStartup - trainingStartTime;
        if (running != null) StopCoroutine(running);
        running = null;
        isTraining = false;
        isTrained = hasLearnedSteps;
        status = "Treinamento interrompido após " +
                 trainingSeconds.ToString("F1") + " segundos";

        if (isTrained)
            StartInference();
        else
            Debug.LogWarning("Nenhuma atualização da Q-Table foi realizada.", this);
    }

    public void StartInference()
    {
        if (!initialized || isTraining || isInferring || !isTrained) return;
        running = StartCoroutine(InferenceCoroutine());
    }

    private IEnumerator InferenceCoroutine()
    {
        isInferring = true;
        status = "Inferência em andamento";
        ShowState(startState);
        currentStep = 0;

        for (int step = 0; step < maxInferenceSteps; step++)
        {
            if (currentState == goalState) break;
            int action = BestAction(currentState);
            int next = NextState(currentState, action);
            ShowState(next);
            currentStep = step + 1;
            yield return new WaitForSeconds(inferenceDelay);
        }

        status = currentState == goalState
            ? $"Heroi alcancado em {currentStep} movimentos!"
            : "Limite de inferência atingido: política não encontrou o Heroi.";
        Debug.Log(status, this);
        isInferring = false;
        running = null;
    }

    public void ResetLearning()
    {
        if (!initialized) 
          return;

        if (running != null) 
          StopCoroutine(running);
        
        running = null;
        isTraining = false;
        isInferring = false;
        isTrained = false;

        q = new float[grid.width * grid.height, 4];
        
        currentEpisode = 0;
        currentStep = 0;
        successes = 0;
        episodeReward = 0f;
        averageReward = 0f;
        successRate = 0f;
        completedEpisodes = 0;
        trainingSeconds = 0f;
        hasLearnedSteps = false;

        ShowState(startState);
        
        status = "Aprendizado Por Reforço";
    }

    private void Update()
    {
        if (isTraining)
            trainingSeconds = Time.realtimeSinceStartup - trainingStartTime;
    }

    private void OnGUI()
    {
        if (!initialized || !showOnGUI) return;

        // Menu ancorado no canto inferior esquerdo da janela Game.
        float margin = 10f;
        float menuWidth = Mathf.Max(1f, Mathf.Min(350f, Screen.width - 2f * margin));
        float menuHeight = Mathf.Max(1f, Mathf.Min(340f, Screen.height - 2f * margin));
        float menuY = Mathf.Max(0f, Screen.height - menuHeight - margin);

        GUILayout.BeginArea(
            new Rect(margin, menuY, menuWidth, menuHeight),
            GUI.skin.box
        );
        GUILayout.Label("Q-LEARNING");
        GUILayout.Label($"Estado: {status}");
        GUILayout.Label($"Tentativa de treinamento atual: {currentEpisode}/{episodes}");
        GUILayout.Label($"Tentativas de treinamento concluidos: {completedEpisodes}");
        GUILayout.Label($"Tempo de treinamento: {trainingSeconds:F1} s");
        GUILayout.Label($"Movimentos: {currentStep}");
        GUILayout.Label($"Recompensa do treinamento: {episodeReward:F2}");
        GUILayout.Label($"Recompensa media: {averageReward:F2}");
        GUILayout.Label($"Sucessos: {successes}");
        GUILayout.Label($"Taxa de sucesso: {successRate:F1}%");

        GUI.enabled = !isTraining && !isInferring;
        if (GUILayout.Button("TREINAR")) StartTraining();

        GUI.enabled = isTraining;
        if (GUILayout.Button("PARAR TREINAMENTO E INICIAR INFERÊNCIA"))
            StopTrainingAndInfer();

        GUI.enabled = isTrained && !isTraining && !isInferring;
        if (GUILayout.Button("INFERÊNCIA")) StartInference();

        GUI.enabled = true;
        if (GUILayout.Button("REINICIAR APRENDIZADO")) ResetLearning();
        GUILayout.EndArea();
    }
}
