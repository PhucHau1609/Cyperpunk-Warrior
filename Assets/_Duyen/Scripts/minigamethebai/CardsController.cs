using DG.Tweening;
using NUnit.Framework;
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
