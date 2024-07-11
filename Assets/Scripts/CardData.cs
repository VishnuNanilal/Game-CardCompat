using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Card", menuName ="CardGame/Card")]
public class CardData: ScriptableObject
{
    public enum DamageType
    {
        Fire,
        Ice,
        Destruct,
        Both
    }

    public string cardTitle;
    public string description;
    public int cost;
    public int damage;

    public Sprite cardImage;
    public Sprite cardFrame;
    public int numberInDeck;
    public DamageType damageType;
    public bool isMulti = false;
    public bool isDefenseCard = false;
    public bool isMirrorCard = false;
    public bool isDestructCard = false;
}
