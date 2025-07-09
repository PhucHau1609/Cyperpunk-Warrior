using EasyUI.PickerWheelUI;
using UnityEngine;

public class EnhancedPickerWheel : PickerWheel
{
    [Header("Skill System")]
    [SerializeField] private SkillChallenge skillChallenge;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private float[] bonusMultipliers = { 1.33f, 1.66f, 2f }; // Bonus cho từng level

    private int completedLevels = 0;
    private float currentBonusMultiplier = 1f;

    protected override void Start()
    {
        base.Start();

        // Setup events
        if (skillChallenge != null)
            skillChallenge.OnChallengeComplete += OnChallengeComplete;

        if (progressBar != null)
            progressBar.OnReachCheckpoint += OnReachCheckpoint;
    }

    public override void Spin()
    {
        if (IsSpinning) return;

        // Reset skill system
        completedLevels = 0;
        currentBonusMultiplier = 1f;

        // Bắt đầu progress bar thay vì spin ngay
        if (progressBar != null)
        {
            progressBar.StartProgress();
        }
        else
        {
            // Fallback to normal spin
            base.Spin();
        }
    }

    private void OnReachCheckpoint(int checkpointIndex)
    {
        // Bắt đầu skill challenge
        if (skillChallenge != null)
        {
            skillChallenge.StartChallenge(checkpointIndex);
        }
    }

    private void OnChallengeComplete(bool success)
    {
        if (success)
        {
            // Hoàn thành level - tăng bonus
            completedLevels++;
            if (completedLevels <= bonusMultipliers.Length)
            {
                currentBonusMultiplier = bonusMultipliers[completedLevels - 1];
            }

            // Tiếp tục progress bar
            if (progressBar != null)
            {
                progressBar.ContinueProgress();
            }
        }
        else
        {
            // Thất bại - kết thúc và spin với bonus hiện tại
            StartFinalSpin();
        }

        // Kiểm tra đã hoàn thành hết 3 level chưa
        if (completedLevels >= 3)
        {
            StartFinalSpin();
        }
    }

    private void StartFinalSpin()
    {
        // Áp dụng bonus multiplier vào chance
        ApplyBonusMultiplier();

        // Bắt đầu spin bánh xe
        base.Spin();
    }

    private void ApplyBonusMultiplier()
    {
        // Tăng chance của các item có giá trị cao
        for (int i = 0; i < wheelPieces.Length; i++)
        {
            // Giả sử item có Amount cao hơn sẽ được tăng chance
            if (wheelPieces[i].Amount > 0)
            {
                wheelPieces[i].Chance *= currentBonusMultiplier;
            }
        }

        // Tính lại weights
        RecalculateWeights();
    }

    private void RecalculateWeights()
    {
        accumulatedWeight = 0;
        nonZeroChancesIndices.Clear();

        for (int i = 0; i < wheelPieces.Length; i++)
        {
            WheelPiece piece = wheelPieces[i];

            accumulatedWeight += piece.Chance;
            piece._weight = accumulatedWeight;
            piece.Index = i;

            if (piece.Chance > 0)
                nonZeroChancesIndices.Add(i);
        }
    }

    private void OnDestroy()
    {
        if (skillChallenge != null)
            skillChallenge.OnChallengeComplete -= OnChallengeComplete;

        if (progressBar != null)
            progressBar.OnReachCheckpoint -= OnReachCheckpoint;
    }
}
