using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance=null;
    public Sprite[] damageNumbers = new Sprite[10];
    public Sprite[] healthNumbers = new Sprite[10];

    public Deck playerDeck = new Deck();
    public Deck enemyDeck = new Deck();
    public Transform playerDeckpos;
    public Transform enemyDeckpos;

    public Player player = null;
    public Player enemy = null;

    public List<CardData> cards = new List<CardData>();

    public Hand playersHand=new Hand();
    public Hand enemysHand=new Hand();

    public GameObject cardPrefab = null;
    public Canvas canvas = null;

    public bool isPlayable = false;

    public GameObject effectFromLeftPrefab = null;
    public GameObject effectFromRightPrefab = null;

    public Sprite fireballImage=null;
    public Sprite iceballImage=null;
    public Sprite multFireballImage = null;
    public Sprite multiIceballImage = null;
    public Sprite fireAndIceballImage = null;
    public Sprite DestroyballImage=null;

    public bool playersTurn = true;

    public Image enemySkipImage=null;

    public Text turnText;
    public Text scoreText = null;

    public int playerScore = 0;
    public int playerKills = 0;

    public Sprite fireDemon = null;
    public Sprite iceDemon = null;

    public AudioSource playerDieAudio = null;
    public AudioSource enemyDieAudio = null;

    private void Awake()
    {
        instance = this;
        SetupEnemy();
        playerDeck.Create();
        enemyDeck.Create();
        UpdateScore();
        StartCoroutine(DealHands());
    }

    public void SkipTurn()
    {
        if (playersTurn && isPlayable)
            NextPlayersTurn();
    }
    internal IEnumerator DealHands()
    {
        yield return new WaitForSeconds(1);

        for(int t=0;t<3;t++)
        {
            playerDeck.DealCard(playersHand);
            enemyDeck.DealCard(enemysHand);

            yield return new WaitForSeconds(1);
        }
        isPlayable = true;
    }

    internal bool UseCard(Card card, Player usingOnPlayer, Hand fromHand)
    {   
        if(!CardValid(card,usingOnPlayer,fromHand))
            return false;

        isPlayable = false;

        CastCard(card, usingOnPlayer, fromHand);

        player.glowImage.gameObject.SetActive(false);
        enemy.glowImage.gameObject.SetActive(false);

        fromHand.RemoveCard(card);

        return false;
    }

    internal bool CardValid(Card cardBeingPlayed, Player usingOnPlayer, Hand fromHand)
    {
        bool valid = false;
        if (cardBeingPlayed == null)
            return false;

        if (fromHand.isPlayers)
        {
            if (cardBeingPlayed.cardData.cost <= player.mana)
            {
                if(usingOnPlayer.isPlayer&&cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if(!usingOnPlayer.isPlayer&&!cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
            }
        }

        else
        {
            if (cardBeingPlayed.cardData.cost <= enemy.mana)
            {
                if (!usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
            }
        }
            return valid;
    }

    internal void CastCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        if(card.cardData.isMirrorCard)
        {
            usingOnPlayer.SetMirror(true);
            NextPlayersTurn();
            isPlayable = true;
            usingOnPlayer.PlayMirrorSound();
        }
        else
        {
            if(card.cardData.isDefenseCard)
            {
                usingOnPlayer.health += card.cardData.damage;
                usingOnPlayer.PlayHealSound();

                if (usingOnPlayer.health > usingOnPlayer.maxHealth)
                    usingOnPlayer.health = usingOnPlayer.maxHealth;

                UpdateHealths();

                StartCoroutine(CastHealEffect(usingOnPlayer));
            }
            else
            {
                CastAttackEffect(card, usingOnPlayer);
            }

            if (fromHand.isPlayers)
                playerScore += card.cardData.damage;

            UpdateScore();
        }
        if (fromHand.isPlayers)
        {
            player.mana -= card.cardData.cost;
            player.UpdateManaBalls();
        }
        else
        {
            enemy.mana -= card.cardData.cost;
            enemy.UpdateManaBalls();
        }
    }

    private IEnumerator CastHealEffect(Player usingOnPlayer)
    {
        yield return new WaitForSeconds((float)0.5);
        NextPlayersTurn();
        isPlayable = true;
    }

    internal void CastAttackEffect(Card card, Player usingOnPlayer)
    {
        GameObject effectGO = null;
        if(usingOnPlayer.isPlayer)
        {
            effectGO = Instantiate(effectFromRightPrefab, canvas.gameObject.transform);
        }
        else
        {
            effectGO = Instantiate(effectFromLeftPrefab, canvas.gameObject.transform);
        }

        Effect effect = effectGO.GetComponent<Effect>();
        if(effect)
        {
            effect.targetPlayer = usingOnPlayer;
            effect.sourceCard = card;

            switch(card.cardData.damageType)
            {
                case CardData.DamageType.Destruct:
                    effect.effectImage.sprite = DestroyballImage;
                    effect.PlayDestructballSound();
                    break;
                case CardData.DamageType.Fire:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multFireballImage;
                    else
                        effect.effectImage.sprite = fireballImage;
                    effect.PlayFireballSound();
                    break;
                case CardData.DamageType.Ice:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiIceballImage;
                    else
                        effect.effectImage.sprite = iceballImage;
                    effect.PlayIceSound();
                    break;
                case CardData.DamageType.Both:
                    effect.effectImage.sprite = fireAndIceballImage;
                    effect.PlayFireballSound();
                    effect.PlayIceSound();
                    break;
            }
        }
    }
    
    internal void UpdateHealths()
    {
        player.UpdateHealth();
        enemy.UpdateHealth();

        if(player.health<=0)
        {
            StartCoroutine(GameOver());
            player.UpdateHealth();
        }
        if(enemy.health<=0)
        {
            playerKills++;
            playerScore += 100;
            UpdateScore();
            StartCoroutine(NewEnemy());
            enemy.UpdateHealth();
        }
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    private IEnumerator NewEnemy()
    {
        enemy.gameObject.SetActive(false);

        enemysHand.ClearHand();

        yield return new WaitForSeconds(0.75f);
        SetupEnemy();
        enemy.gameObject.SetActive(true);
        StartCoroutine(DealHands());
    }

    private void SetupEnemy()
    {
        enemy.mana = 0;
        enemy.health = 5;
        enemy.UpdateHealth();
        enemy.isFire = true;

        if(UnityEngine.Random.Range(0,2)==1)
            enemy.isFire = false;
        if (enemy.isFire)
            enemy.playerImage.sprite = fireDemon;
        else
            enemy.playerImage.sprite = iceDemon;
    }

    public void NextPlayersTurn()
    {
        playersTurn = !playersTurn;
        bool enemyIsDead = false;

        if (playersTurn)
        {
            if (player.mana < 5)
                player.mana++;
        }
        else
        {
            if (enemy.health > 0)
            {
                if (enemy.mana < 5)
                    enemy.mana++;
            }
            else
                enemyIsDead = true;
        }

        if (enemyIsDead)
        {
            playersTurn = !playersTurn;
            if (player.mana < 5)
                player.mana++;
        }
        else
        {
            SetTurnText();

            if (!playersTurn)
                MonstersTurn();
        }

        player.UpdateManaBalls();
        enemy.UpdateManaBalls();

    }

    internal void SetTurnText()
    {
        if(playersTurn)
        {   
            turnText.text= "Merlin's turn";
        }
        else
            turnText.text = "Enemy's turn";
    }

    private void MonstersTurn()
    {
        Card card = AIChooseCard();
        StartCoroutine(MonsterCastCard(card));
    }

    private Card AIChooseCard()
    {
        List<Card> available = new List<Card>();
        for(int i=0;i<3;i++)
        {
            if(CardValid(enemysHand.cards[i],enemy,enemysHand))
                    available.Add(enemysHand.cards[i]);
            else if (CardValid(enemysHand.cards[i], player, enemysHand))
                available.Add(enemysHand.cards[i]);
        }
        if(available.Count==0)
        {
            NextPlayersTurn();
            return null;
        }
        int choice = UnityEngine.Random.Range(0, available.Count);
        return available[choice];
    }

    private IEnumerator MonsterCastCard(Card card)
    {
        yield return new WaitForSeconds(0.5f);

        if(card)
        {
            TurnCard(card);

            yield return new WaitForSeconds(2f);

            if (card.cardData.isDefenseCard)
                UseCard(card, enemy, enemysHand);
            else
                UseCard(card, player, enemysHand);

            yield return new WaitForSeconds(1f);

            enemyDeck.DealCard(enemysHand);

            yield return new WaitForSeconds(1f);
        }
        else
        {
            enemySkipImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            enemySkipImage.gameObject.SetActive(false);
        }
    }

    internal void TurnCard(Card card)
    {
        Animator animator = card.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.SetTrigger("Flip");
        }
        else
            Debug.LogError("No Animator found.");
    }

    private void UpdateScore()
    {
        scoreText.text = "Demons killed: " + playerKills.ToString() + ". Score: " + playerScore.ToString();
    }

    internal void PlayPlayerDieSound()
    {
        playerDieAudio.Play();
    }

    internal void PlayEnemyDieSound()
    {
        enemyDieAudio.Play();
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }
}
