using DG.Tweening;
using EasyUI.PickerWheelUI;
using System.Collections;
using UnityEngine;

public class EnhancedPickerWheel : PickerWheel
{
    [Header("Skill System")]
    [SerializeField] private SkillChallenge skillChallenge;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private float[] bonusMultipliers = { 1.33f, 1.66f, 2f }; // Bonus cho từng level

    [Header("Player Setting")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject panelMiniGameX;
    [SerializeField] private Animator playerAnimator;


    [Header("Reward System")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int bombCount = 4; // Số lượng bomb muốn spawn
    [SerializeField] private float spawnDelay = 0.1f; // Delay giữa các lần spawn (tùy chọn)
    [SerializeField] private Transform spawnLeft;
    [SerializeField] private Transform spawnRight;
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private Vector2 leftForce = new Vector2(-5f, 8f);
    [SerializeField] private Vector2 rightForce = new Vector2(5f, 8f);
    [SerializeField] private float coreMoveDistance = 2f;
    [SerializeField] private float coreMoveDuration = 1f;
    [SerializeField] private DG.Tweening.Ease coreMoveEase = DG.Tweening.Ease.OutBack;




    private float[] originalChances; // Lưu trữ chance ban đầu của từng wheel piece
    private int completedLevels = 0;
    private float currentBonusMultiplier = 1f;

    // 2. Thêm method để backup và restore chances
    private void BackupOriginalChances()
    {
        if (originalChances == null || originalChances.Length != wheelPieces.Length)
        {
            originalChances = new float[wheelPieces.Length];
        }

        for (int i = 0; i < wheelPieces.Length; i++)
        {
            originalChances[i] = wheelPieces[i].Chance;
        }
    }

    private void RestoreOriginalChances()
    {
        if (originalChances != null && originalChances.Length == wheelPieces.Length)
        {
            for (int i = 0; i < wheelPieces.Length; i++)
            {
                wheelPieces[i].Chance = originalChances[i];
            }
            RecalculateWeights();
        }
    }


    protected override void Start()
    {
        base.Start();

        BackupOriginalChances();


        // Setup events
        if (skillChallenge != null)
            skillChallenge.OnChallengeComplete += OnChallengeComplete;

        if (progressBar != null)
            progressBar.OnReachCheckpoint += OnReachCheckpoint;
    }
    // Thêm method override cho Spin để đăng ký event:

    // 4. Cập nhật method Spin để reset trước mỗi lần quay
    public override void Spin()
    {
        if (IsSpinning) return;

        // Reset về trạng thái ban đầu trước mỗi lần quay
        RestoreOriginalChances();

        playerMovement.canMove = false;

        // Đăng ký event trước khi spin
        OnSpinEnd(OnWheelSpinComplete);

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
    /*  public override void Spin()
      {
          if (IsSpinning) return;
          playerMovement.canMove = false;

          // Đăng ký event trước khi spin
          OnSpinEnd(OnWheelSpinComplete);

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
      }*/


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

    // Thêm method override cho OnSpinEnd:
    // Method xử lý khi bánh xe dừng lại:
    /*   private void OnWheelSpinComplete(WheelPiece piece)
       {
           // Xác định loại phần thưởng dựa trên index
           WheelPieceType pieceType = GetWheelPieceTypeByIndex(piece.Index);

           // Spawn phần thưởng tương ứng
           switch (pieceType)
           {
               case WheelPieceType.Artefact:
                   SpawnEnergyCore();
                   break;
               case WheelPieceType.Bomb:
                   SpawnBombs();
                   break;
           }

           // Cho phép player di chuyển lại
           if (playerMovement != null)
               playerMovement.canMove = true;
       }*/

    // 5. Cập nhật method OnWheelSpinComplete để cho phép quay lại
    private void OnWheelSpinComplete(WheelPiece piece)
    {
        // Xác định loại phần thưởng dựa trên index
        WheelPieceType pieceType = GetWheelPieceTypeByIndex(piece.Index);

        // Spawn phần thưởng tương ứng
        switch (pieceType)
        {
            case WheelPieceType.Artefact:
                SpawnEnergyCore();
                break;
            case WheelPieceType.Bomb:
                SpawnBombs();
                break;
        }

        // Cho phép player di chuyển lại
        if (playerMovement != null)
            playerMovement.canMove = true;

        // Reset về trạng thái ban đầu để sẵn sàng cho lần quay tiếp theo
        RestoreOriginalChances();
    }

    // 6. Thêm method public để reset thủ công (nếu cần)
    public void ResetToOriginalState()
    {
        RestoreOriginalChances();
        completedLevels = 0;
        currentBonusMultiplier = 1f;
    }

    private void SpawnBombs()
    {
        this.TurnOffMiniGame();
        playerAnimator.Play("Player_ANGRY_N");
        if (bombPrefab != null && spawnLeft != null && spawnRight != null)
        {
            for (int i = 0; i < bombCount; i++)
            {
                // Chọn vị trí spawn (trái hoặc phải)
                Transform spawnPosition = (i % 2 == 0) ? spawnLeft : spawnRight;
                Vector2 force = (i % 2 == 0) ? leftForce : rightForce;

                // Spawn bomb
                GameObject bomb = Instantiate(bombPrefab, spawnPosition.position, Quaternion.identity);
                Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // Thêm random variation để bomb không bay cùng quỹ đạo
                    Vector2 randomizedForce = new Vector2(
                        force.x + Random.Range(-1f, 1f),
                        force.y + Random.Range(-0.5f, 0.5f)
                    );
                    rb.AddForce(randomizedForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    // Hoặc nếu muốn spawn với delay:
    private void SpawnBombsWithDelay()
    {
        if (bombPrefab != null && spawnLeft != null && spawnRight != null)
        {
            StartCoroutine(SpawnBombsCoroutine());
        }
    }

    private IEnumerator SpawnBombsCoroutine()
    {
        for (int i = 0; i < bombCount; i++)
        {
            // Chọn vị trí spawn (trái hoặc phải)
            Transform spawnPosition = (i % 2 == 0) ? spawnLeft : spawnRight;
            Vector2 force = (i % 2 == 0) ? leftForce : rightForce;

            // Spawn bomb
            GameObject bomb = Instantiate(bombPrefab, spawnPosition.position, Quaternion.identity);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Thêm random variation để bomb không bay cùng quỹ đạo
                Vector2 randomizedForce = new Vector2(
                    force.x + Random.Range(-1f, 1f),
                    force.y + Random.Range(-0.5f, 0.5f)
                );
                rb.AddForce(randomizedForce, ForceMode2D.Impulse);
            }

            // Delay trước khi spawn bomb tiếp theo
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnergyCore()
    {
        this.TurnOffMiniGame();
        playerAnimator.Play("Player_HAPPY_N");
        if (spawnCenter != null)
        {
            ItemsDropCtrl coreItem = ItemsDropManager.Instance.DropItemObject(ItemCode.UpgradeItem_2, 1, spawnCenter.position);
            if (coreItem == null) return;

            // Di chuyển bằng DOTween
            Vector3 targetPos = coreItem.transform.position + Vector3.up * coreMoveDistance;
            coreItem.transform.DOMove(targetPos, coreMoveDuration)
                .SetEase(coreMoveEase)
                .SetUpdate(true); // vẫn hoạt động khi timescale = 0
        }


    }

    public void TurnOffMiniGame()
    {
        panelMiniGameX.SetActive(false);

        RestoreOriginalChances();

    }
}

// Thêm enum để phân loại wheel pieces
public enum WheelPieceType
{
    Artefact,
    Bomb,
    Other
}
