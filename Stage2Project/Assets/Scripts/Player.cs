using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: Will probably have to change ParticleSystem to be some kind of ring around player, maybe a different asset

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ParticleSystem))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private float Speed;

    [SerializeField]
    private float InitialHealth;

    [SerializeField]
    private float Defense;

    [SerializeField]
    private float TimeBetweenScoreIncrease;

    [SerializeField]
    private float MassRepelDistance = 40.0f;

    //Some may last 1 * PowerupTime, others 1.5 * PowerupTime, etc., so this time is relative
    //(Or may just keep it as 1 * PowerupTime, since a lot of the powerups don't decay)
    [SerializeField]
    private float PowerupTime = 10.0f;

    [SerializeField]
    private float BombRadius = 30.0f;

    private float mMassRepelPowerupTime;

    private int mNumberCollisions;

    private Rigidbody mBody;

    private float mHealth;

    private int mScore;

    private float mNextUpdateScore;

    private float mRepellentPlayerTimeLeft;

    private float mMassRepelTimeLeft;

    private float mMassRepelCooldown;

    private List<MagnetizedByPlayer> mRepellingToAttracting;

    private List<MagnetizedByPlayer> mMassRepelEnemies;

    private bool mShieldUp;

    private float mShieldImmunityTimeLeft;

    private float mShieldDownImmunityTime = 2.0f; //TODO: Change to 1? Maybe make serializable?

    private ParticleSystem.EmissionModule mShield;

    private int mBombs;

    private int mBullets;

    private float mPickupCooldown = 5.0f;

    private float mPickupCooldownTimeLeft;    

    private bool mUseBomb = false;

    //For health bar
    public float BarProgress;
    public Vector2 BarPos = new Vector2(200, 40);
    public int BarWidth = 60;
    public int BarHeight = 20;
    public Vector2 BarSize;
    public Texture2D BarEmptyTex;
    public Texture2D BarFullTex;

    void Awake()
    {
        mBody = GetComponent<Rigidbody>();

        mShield = GetComponent<ParticleSystem>().emission;

        mRepellingToAttracting = new List<MagnetizedByPlayer>();

        mMassRepelEnemies = new List<MagnetizedByPlayer>();

        mMassRepelPowerupTime = PowerupTime / 30.0f;
        //mMassRepelPowerupTime = 0.4f;

        ResetPlayer();

        BarSize = new Vector2(BarWidth, BarHeight);
        BarEmptyTex = new Texture2D(BarWidth, BarHeight);
        BarFullTex = new Texture2D(BarWidth, BarHeight);
        Color32 green = new Color32(0, 255, 0, 255);
        Color32 red = new Color32(255, 0, 0, 255);
        FillTexture(BarEmptyTex, green);
        FillTexture(BarFullTex, red);
        
    }

    void Update()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
        {
            direction = -Vector3.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            direction = Vector3.right;
        }

        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction += -Vector3.forward;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (mBombs > 0)
            {
                mUseBomb = true;
                mNumberCollisions = 0; //Regardless of the bomb radius, all enemies touching the player should be destroyed.
                mBombs--;
            }
        }

        if (mNumberCollisions > 0 && !IsDead() && mShieldImmunityTimeLeft <= 0.0f && !mShieldUp)
        {
            mHealth -= ((1 / Defense) * 10f);
        }

        //TODO: Maybe do something here, although it messes with the way the screens and buttons work 
        //Also, we want to make sure this 
        /*if (IsDead())
        {
            print("You are dead.");
            //gameObject.SetActive(false);
            //gameObject.GetComponent(Player).enabled = false;
            //Player player = gameObject.GetComponent(typeof(Player)) as Player;
            //player.enabled = false;
        }*/

        if (mRepellentPlayerTimeLeft > 0.0f)
        {
            float newTimeLeft = mRepellentPlayerTimeLeft - Time.deltaTime;

            if (HasJustGotRepelPowerup())
            {
                MagnetizedByPlayer[] individuals = FindObjectsOfType<MagnetizedByPlayer>();
                if (individuals != null)
                {
                    for (int count = 0; count < individuals.Length; ++count)
                    {
                        MagnetizedByPlayer individual = individuals[count];

                        if ((individual != null) && !individual.CompareTag("Collectible") && (individual.ForceType == MagnetizedByPlayer.Type.Attract))
                        {

                            individual.MakeRepelling();
                            mRepellingToAttracting.Add(individual);
                        }
                    }
                }
            }
            else if (newTimeLeft < 0.0f)
            {
                /* The repellent powerup is about to expire, so make all enemies that should be attracting, attract again. */
                for (int count = 0; count < mRepellingToAttracting.Count; ++count)
                {
                    if (mRepellingToAttracting[count] != null)
                    {
                        mRepellingToAttracting[count].RevertMagnetizeType();
                    }
                }
                mRepellingToAttracting.Clear();
            }

            mRepellentPlayerTimeLeft = newTimeLeft;
        }
        else if (mMassRepelTimeLeft > 0.0f)
        {
            float newTimeLeft = mMassRepelTimeLeft - Time.deltaTime;

            if (HasJustGotMassRepelPowerup())
            {
                MagnetizedByPlayer[] individuals = FindObjectsOfType<MagnetizedByPlayer>();

                if (individuals != null)
                {

                    for (int count = 0; count < individuals.Length; ++count)
                    {
                        MagnetizedByPlayer individual = individuals[count];

                        if ((individual != null) && !individual.CompareTag("Collectible"))
                        {
                            Vector3 difference = individuals[count].transform.position - transform.position;
                            if (difference.magnitude <= MassRepelDistance)
                            {
                                individual.MakeRepelling();
                                individual.SetMassRepelForce();
                                individual.SetMassRepelDistance();
                                mMassRepelEnemies.Add(individual);
                            }
                        }
                    }
                }
            }
            else if (newTimeLeft < 0.0f)
            {
                for (int count = 0; count < mMassRepelEnemies.Count; ++count)
                {
                    if (mMassRepelEnemies[count] != null)
                    {
                        mMassRepelEnemies[count].RevertMagnetizeType();
                        mMassRepelEnemies[count].RevertMassRepelForce();
                        mMassRepelEnemies[count].RevertMassRepelDistance();
                    }
                }
                mMassRepelEnemies.Clear();
            }

            mMassRepelTimeLeft = newTimeLeft;
        }

        mMassRepelCooldown -= Time.deltaTime;

        if (mShieldImmunityTimeLeft > 0.0f)
        {
            mShieldImmunityTimeLeft -= Time.deltaTime;
        }

        if (mPickupCooldownTimeLeft > 0.0f)
        {
            mPickupCooldownTimeLeft -= Time.deltaTime;
        }

        BarProgress = mHealth * (1 / InitialHealth);

        mNextUpdateScore -= Time.deltaTime;
        if (mNextUpdateScore <= 0.0f)
        {
            if (!IsDead())
            {
                mScore++;
            }
            mNextUpdateScore = TimeBetweenScoreIncrease;
        }

        if (!IsDead())
        {
            mBody.AddForce(direction * Speed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            mNumberCollisions++;
            if (mShieldUp)
            {
                mShieldUp = false;
                mShield.enabled = false;
                mShieldImmunityTimeLeft = mShieldDownImmunityTime;
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            mNumberCollisions--;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!IsDead())
        {
            if (col.gameObject.CompareTag("Collectible"))
            {
                Destroy(col.gameObject);
                mScore += 50;
            }
            else if (col.gameObject.CompareTag("Powerup"))
            {
                PowerupTag.Powerup powerupType = col.gameObject.GetComponent<PowerupTag>().Type;

                switch (powerupType)
                {
                    case PowerupTag.Powerup.Bomb:
                        //Increment bomb count 
                        //(Code to set off bomb still to be written somewhere)
                        if (mPickupCooldownTimeLeft <= 0.0f)
                        {
                            mBombs++;
                            mPickupCooldownTimeLeft = mPickupCooldown;
                        }
                        break;
                    case PowerupTag.Powerup.Bullets:
                        //
                        if (mPickupCooldownTimeLeft <= 0.0f)
                        {
                            mBullets++;
                            mPickupCooldownTimeLeft = mPickupCooldown;
                        }
                        break;
                    case PowerupTag.Powerup.MassRepel:
                        if (mMassRepelCooldown <= 0.0f && mRepellentPlayerTimeLeft <= 0.0f)
                        {
                            mMassRepelCooldown = mMassRepelPowerupTime * 6;
                            mMassRepelTimeLeft = mMassRepelPowerupTime;
                        }
                        break;
                    case PowerupTag.Powerup.RepellentPlayer:
                        if (mMassRepelTimeLeft <= 0.0f)
                        {
                            mRepellentPlayerTimeLeft = PowerupTime;
                        }
                        break;
                    case PowerupTag.Powerup.Shield:
                        mShieldUp = true;
                        mShield.enabled = true;
                        break;
                    default:
                        break;

                }
            }
        }
    }

    void OnGUI()
    {

        GUIStyle damageStyle = new GUIStyle();
        damageStyle.alignment = TextAnchor.MiddleCenter;
        damageStyle.fontSize = 18;
        damageStyle.normal.textColor = Color.white;

        GUI.BeginGroup(new Rect(0, BarPos.y, BarPos.x, BarHeight));
            GUI.Label(new Rect(0, 0, BarPos.x, BarHeight), "Damage: ", damageStyle);
        GUI.EndGroup();

        GUIStyle barStyle = new GUIStyle();
        barStyle.border.left = 0;
        barStyle.border.right = 0;
        barStyle.border.bottom = 0;
        barStyle.border.top = 0;

        //Empty health bar
        GUI.BeginGroup(new Rect(BarPos.x, BarPos.y, BarWidth, BarHeight));
            GUI.Box(new Rect(0, 0, BarWidth, BarHeight), BarFullTex, barStyle);

            //Health bar filled in by BarProgress amount
            GUI.BeginGroup(new Rect(BarWidth - BarWidth * BarProgress, 0, BarWidth * BarProgress, BarHeight));
                GUI.Box(new Rect(0, 0, BarWidth, BarHeight), BarEmptyTex, barStyle);

            GUI.EndGroup();
        GUI.EndGroup();

        GUIStyle scoreStyle = new GUIStyle();
        scoreStyle.fontSize = 18;
        scoreStyle.normal.textColor = Color.white;

        GUI.BeginGroup(new Rect(BarPos.x + 400, BarPos.y, BarWidth + 100, BarHeight));
            GUI.Label(new Rect(0, 0, BarWidth, BarHeight), "Score: " + mScore.ToString(), scoreStyle);
        GUI.EndGroup();

        //Maybe_TODO: if (mRepellentPlayerTimeLeft > 0.0f) { display timer below player, based on PowerupTime and current mRepellentPlayerTime }
    }

    void FillTexture(Texture2D texture, Color32 colour)
    {
        int numPixels = texture.width * texture.height;
        Color32[] newColours = new Color32[numPixels];
        for (int i = 0; i < numPixels; i++)
        {
            newColours[i] = colour;
        }
        texture.SetPixels32(newColours);
        texture.Apply(true);
    }

    internal bool IsDead()
    {
        return mHealth <= 0;
    }

    internal bool HasJustGotRepelPowerup()
    {
        return (mRepellentPlayerTimeLeft == PowerupTime);
    }

    private bool HasJustGotMassRepelPowerup()
    {
        return (mMassRepelTimeLeft == mMassRepelPowerupTime);
    }

    internal float GetPowerupTime()
    {
        return PowerupTime;
    }

    internal bool UsingBomb()
    {
        return mUseBomb;
    }

    internal void UsedBomb()
    {
        mUseBomb = false;
    }

    internal float GetBombRadius()
    {
        return BombRadius;
    }

    //Protection of this function is worrying, although the given code could access the mPlayer.transform already, I still feel uneasy that you can access the health, score and number of collisions.
    internal void ResetPlayer()
    {
        transform.position = new Vector3(0.0f, 0.5f, 0.0f); //This may be tricker to change when there are multiple players, like each player will have to store what number player they are and so can work out where they spawn
        mHealth = InitialHealth;
        mScore = 0;
        mNumberCollisions = 0;
        mRepellentPlayerTimeLeft = 0.0f;
        mRepellingToAttracting.Clear();
        mMassRepelEnemies.Clear();
        mBody.velocity = Vector3.zero;
        mMassRepelCooldown = 0.0f;
        mMassRepelTimeLeft = 0.0f;
        mShieldImmunityTimeLeft = 0.0f;
        mShieldUp = false;
        mShield.enabled = false;
        mBombs = 0;
        mBullets = 0;
        mPickupCooldownTimeLeft = 0.0f;
        mUseBomb = false;
    }
}

