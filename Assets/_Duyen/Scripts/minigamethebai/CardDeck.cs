//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CardDeck
//{
//    private List<Card> cards = new List<Card>();
//    private Sprite hiddenSprite;
//    private Card cardPrefab;

//    public CardDeck(Sprite[] cardSprites, Sprite hiddenSprite, GameConfig config, Card prefab = null)
//    {
//        this.hiddenSprite = hiddenSprite;
//        this.cardPrefab = prefab;
//    }

//    public IEnumerator CreateDeck(List<Sprite> sprites, Transform parent, Transform pile)
//    {
//        cards.Clear();
//        foreach (var sprite in sprites)
//        {
//            Card card;

//            if (cardPrefab != null)
//            {
//                // Dùng prefab có sẵn Image
//                card = Object.Instantiate(cardPrefab, parent);
//            }
//            else
//            {
//                // fallback (tạo GO trống)
//                var go = new GameObject("Card");
//                go.transform.SetParent(parent);
//                card = go.AddComponent<Card>();
//            }

//            card.Initialize(sprite, hiddenSprite);
//            cards.Add(card);

//            yield return null;
//        }
//    }

//    public bool Contains(Card card) => cards.Contains(card);
//    public void FlipCard(Card card)
//    {
//        if (card.IsFlipped)
//            card.Hide();
//        else
//            card.Show();
//    }
//    public bool IsCardFlipped(Card card) => card.IsFlipped;

//    public Card GetRandomUnflippedCard()
//    {
//        var candidates = cards.FindAll(c => !c.IsFlipped);
//        if (candidates.Count == 0) return null;
//        return candidates[Random.Range(0, candidates.Count)];
//    }

//    public void ResetFlippedCards()
//    {
//        foreach (var card in cards)
//        {
//            if (card.IsFlipped)
//                card.Hide();
//        }
//    }
//}
