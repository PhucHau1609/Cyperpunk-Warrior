using System.Collections.Generic;
using UnityEngine;

public class PlayerHand
{
    private List<Card> cards;
    private int maxSize;
    private bool visible;

    public int CardCount => cards.Count;

    public PlayerHand(int maxSize, bool visible)
    {
        this.maxSize = maxSize;
        this.visible = visible;
        cards = new List<Card>();
    }

    public void SetCards(List<Sprite> sprites)
    {
        cards.Clear();
        foreach (var sprite in sprites)
        {
            var go = new GameObject("Card");
            var card = go.AddComponent<Card>();
            card.Initialize(sprite, null);
            cards.Add(card);
        }
    }

    public void AddCard(Card card) => cards.Add(card);
    public void RemoveCard(Card card) => cards.Remove(card);
    public bool Contains(Card card) => cards.Contains(card);

    public Card GetRandomCard()
    {
        if (cards.Count == 0) return null;
        return cards[Random.Range(0, cards.Count)];
    }
}
