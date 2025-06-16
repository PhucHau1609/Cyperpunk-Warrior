using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsController : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    [Header("UI Panels")]
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject finishPanel;

    private List<Sprite> spritePairs = new List<Sprite>();
    private List<Card> activeCards = new List<Card>();

    Card firstSelected;
    Card secondSelected;

    int matchCounts;

    private void Start()
    {
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);
    }

    public void StartGame()
    {
        gamePanel.SetActive(true);
        finishPanel.SetActive(false);
        ResetGame();
    }

    void ResetGame()
    {
        matchCounts = 0;
        firstSelected = null;
        secondSelected = null;

        // Xóa thẻ cũ nếu có
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        activeCards.Clear();
        PrepareSprites();
        CreateCards();
    }

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();
        for(int i = 0; i < sprites.Length; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }

        ShuffleSprites(spritePairs);
    }

    void CreateCards()
    {
        for(int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.controller = this;
            //card.originalPosition = card.transform.localPosition;
            activeCards.Add(card);
        }
    }

    public void SetSelected(Card card)
    {
        if (card.isSelected == false)
        {
            card.Show();

            if (firstSelected == null)
            {
                firstSelected = card;
                return;
            }

            if (secondSelected == null)
            {
                secondSelected = card;
                StartCoroutine(CheckMatching(firstSelected, secondSelected));
                firstSelected = null;
                secondSelected = null;
            }
        }
    }

    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(.3f);
        if(a.iconSprite == b.iconSprite)
        {
            matchCounts++;

            a.Match();
            b.Match();

            // Xoá sau khi đã lật trùng
            yield return new WaitForSeconds(0.2f);
            a.FadeOut();
            b.FadeOut();
            activeCards.Remove(a);
            activeCards.Remove(b);
            ShuffleRemainingCards();

            if (activeCards.Count == 0)
            {
                yield return new WaitForSeconds(0.5f);
                gamePanel.SetActive(false);
                finishPanel.SetActive(true);
            }
        }
        else
        {
            a.Hide();
            b.Hide();
        }
    }

    //void ShuffleCardPositions()
    //{
    //    List<Card> remaining = new List<Card>();
    //    foreach (Card card in activeCards)
    //    {
    //        if (!card.isMatched)
    //            remaining.Add(card);
    //    }

    //    // Lấy vị trí hiện tại
    //    List<Vector3> positions = new List<Vector3>();
    //    foreach (Card card in remaining)
    //    {
    //        positions.Add(card.transform.localPosition);
    //    }

    //    // Shuffle vị trí
    //    for (int i = positions.Count - 1; i > 0; i--)
    //    {
    //        int j = Random.Range(0, i + 1);
    //        (positions[i], positions[j]) = (positions[j], positions[i]);
    //    }

    //    // Tween tới vị trí mới
    //    for (int i = 0; i < remaining.Count; i++)
    //    {
    //        remaining[i].transform.DOLocalMove(positions[i], 0.4f).SetEase(Ease.InOutBack);
    //    }
    //}

    void ShuffleRemainingCards()
    {
        // Lấy các thẻ chưa match
        List<Card> remaining = new List<Card>();
        foreach (Card card in activeCards)
        {
            if (!card.isMatched)
                remaining.Add(card);
        }

        // Lấy sprite hiện tại của chúng
        List<Sprite> remainingSprites = new List<Sprite>();
        foreach (Card card in remaining)
        {
            remainingSprites.Add(card.iconSprite);
        }

        // Xáo trộn sprite
        for (int i = remainingSprites.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = remainingSprites[i];
            remainingSprites[i] = remainingSprites[j];
            remainingSprites[j] = temp;
        }

        // Gán sprite mới cho thẻ + hiệu ứng
        for (int i = 0; i < remaining.Count; i++)
        {
            remaining[i].SetIconSprite(remainingSprites[i]);

            // Option: hiệu ứng lắc nhẹ
            remaining[i].transform.DOShakeRotation(0.3f, 10f, 10, 90f);
        }
    }

    void ShuffleSprites(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
