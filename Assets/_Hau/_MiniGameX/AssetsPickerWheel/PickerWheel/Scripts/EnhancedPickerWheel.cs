using EasyUI.PickerWheelUI;
using UnityEngine;

public class EnhancedPickerWheel : PickerWheel
{
    [Header("Skill System")]
    [SerializeField] private SkillChallenge skillChallenge;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private float[] bonusMultipliers = { 1.33f, 1.66f, 2f }; // Bonus cho từng level

    [Header("Player Setting")]
    [SerializeField] private PlayerMovement playerMovement;


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
       // playerMovement.canMove = false;
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
        }
        // KHÔNG còn else để fail ngay - luôn tiếp tục

        // Luôn tiếp tục progress bar
        if (progressBar != null)
        {
            progressBar.ContinueProgress();
        }

        // Kiểm tra đã hoàn thành hết 3 level chưa
        if (progressBar.IsCompleted())
        {
            // Lấy số level thực sự hoàn thành
            if (skillChallenge != null)
            {
                completedLevels = skillChallenge.GetCompletedLevels();
            }

            StartFinalSpin();
        }
    }


    /*  private void OnChallengeComplete(bool success)
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
      }*/

    private void StartFinalSpin()
    {
        // Áp dụng bonus multiplier vào chance
        ApplyBonusMultiplier();

        // Bắt đầu spin bánh xe
        base.Spin();
    }

    private void ApplyBonusMultiplier()
    {
        // Lấy số level thành công và thất bại
        int successCount = 0;
        int failCount = 0;

        if (skillChallenge != null)
        {
            bool[] results = skillChallenge.GetLevelResults();
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i])
                    successCount++;
                else
                    failCount++;
            }
        }

        // Tính bonus cho từng loại
        float artefactBonus = 1f;
        float bombBonus = 1f;

        // Tăng bonus artefact dựa trên số level thành công
        for (int i = 0; i < successCount && i < bonusMultipliers.Length; i++)
        {
            artefactBonus += (bonusMultipliers[i] - 1f);
        }

        // Tăng bonus bomb dựa trên số level thất bại
        for (int i = 0; i < failCount && i < bonusMultipliers.Length; i++)
        {
            bombBonus += (bonusMultipliers[i] - 1f);
        }

        // Áp dụng bonus vào từng wheel piece dựa trên index
        for (int i = 0; i < wheelPieces.Length; i++)
        {
            WheelPieceType pieceType = GetWheelPieceTypeByIndex(i);

            switch (pieceType)
            {
                case WheelPieceType.Artefact:
                    wheelPieces[i].Chance *= artefactBonus;
                    break;
                case WheelPieceType.Bomb:
                    wheelPieces[i].Chance *= bombBonus;
                    break;
            }
        }

        RecalculateWeights();

        // Debug để kiểm tra
        Debug.Log($"Success: {successCount}, Fail: {failCount}");
        Debug.Log($"Artefact Bonus: {artefactBonus:F2}, Bomb Bonus: {bombBonus:F2}");
    }

    /*   private void ApplyBonusMultiplier()
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
   */
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

    // Thêm method để xác định loại wheel piece dựa trên index
    private WheelPieceType GetWheelPieceTypeByIndex(int index)
    {
        // Theo setup của bạn: index chẵn (0,2,4,6,8) là artefact
        // index lẻ (1,3,5,7,9) là bomb
        if (index % 2 == 0)
            return WheelPieceType.Artefact;
        else
            return WheelPieceType.Bomb;
    }
}

// Thêm enum để phân loại wheel pieces
public enum WheelPieceType
{
    Artefact,
    Bomb,
    Other
}
