using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public OwnCard ownCard;

    void Start()
    {
        ownCard.cardName = "card_0";
        ownCard.cardTime = 2f;
    }
}
