using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: Will probably have to change ParticleSystem to be some kind of ring around player, maybe a different asset

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ParticleSystem))]
public class Player : MonoBehaviour
{
    /* The speed of this player. */
    [SerializeField]
    private float Speed;

    /* The starting health of this player. */
    [SerializeField]
    private float InitialHealth;

    /* Player gets one point every 'TimeBetweenScoreIncrease' seconds. */
    [SerializeField]
    private float TimeBetweenScoreIncrease = 2.0f;

    /* When a 'Mass Repel' powerup is used (as soon as the powerup is flown 
     * over), every enemy within 'MassRepelDistance' of the player will briefly
     * be repelled with a large repelling force. */
    [SerializeField]
    private float MassRepelDistance = 40.0f;
    
    /* When a bomb is used, every enemy withing 'BombRadius' will be 
     * destroyed. */
    [SerializeField]
    private float BombRadius = 30.0f;

    /* Time that the repellent and mass repel powerups will repel for. */
    private float mRepellentPowerupTime = 10.0f;
    private float mMassRepelPowerupTime = 0.4f;

    /* Number of enemies currently colliding with the player, thus causing the 
     * player to lose health. */
    private int mNumberCollisions;

    /* The RigidBody component for this player. */
    private Rigidbody mBody;

    /* The current health of the player. */
    private float mHealth;

    /* The current score of the player. */
    private int mScore;

    /* Repeatedly set to equal 'TimeBetweenScoreIncrease', and decreased every 
     * frame in Update(), and then when this reaches zero, the score will be 
     * updated. */
    private float mNextUpdateScore;

    /* Similar to mNextUpdateScore. */
    private float mRepellentPlayerTimeLeft;

    /* Similar to mNextUpdateScore. */
    private float mMassRepelTimeLeft;

    /* Minimum time a player must wait between using mass repel powerups. */
    private float mMassRepelCooldown = 2.4f;

    /* A list of the enemies that have been made repellent to the player by the
     * repellent player powerup, that need to be made to attract again when the
     * powerup expires. */
    private List<MagnetizedByPlayer> mRepellingToAttracting;

    /* Similar to mRepellingToAttracting, but for the mass repel powerup. */
    private List<MagnetizedByPlayer> mMassRepelEnemies;

    /* True if the player has the shield powerup. */
    private bool mShieldUp;

    /* Similar to mNextUpdateScore. */
    private float mShieldImmunityTimeLeft;

    /* After the player is hit while they have a shield, they are immune for 
     * this many seconds. */
    private float mShieldDownImmunityTime = 2.0f; 

    /* The player's shield. mShield.enabled is toggled depending on whether 
     * the shield should currently be active. */
    private ParticleSystem.EmissionModule mShield;

    /* Number of bombs the player currently has. */
    private int mBombs;

    /* Minimum time a player must wait between using obtaining pickups, such as
     * bombs. (Currently, bombs are the only powerups considered a pickup) */
    private float mPickupCooldown = 4.0f;

    /* Similar to mNextUpdateScore. */
    private float mPickupCooldownTimeLeft;    

    /* True if the user has just pressed the button to use a bomb, and is then 
     * set to false after the bomb has finished being used.  */
    private bool mUseBomb = false;

    /* Minimum time a player must wait between using bombs. */
    private float mUseBombCooldownTime = 1.5f;

    /* Similar to mNextUpdateScore. */
    private float mUseBombCooldown;

    /* These are for displaying the damage bar. */
    public float xEdgeBuffer = 2;
    public float yEdgeBuffer = 1;
    public float BarProgress;
    public Vector2 BarPos;
    public int BarWidth = 60;
    public int BarHeight = 20;
    public Vector2 BarSize;
    public Texture2D BarEmptyTex;
    public Texture2D BarFullTex;

    /* GUI element styles for the different text displayed by OnGUI(). */
    GUIStyle damageStyle = new GUIStyle();
    GUIStyle barStyle = new GUIStyle();
    GUIStyle scoreStyle = new GUIStyle();
    GUIStyle bombStyle = new GUIStyle();
    GUIStyle deadStyle = new GUIStyle();

    void Awake()
    {
        mBody = GetComponent<Rigidbody>();
        mShield = GetComponent<ParticleSystem>().emission;
        mRepellingToAttracting = new List<MagnetizedByPlayer>();
        mMassRepelEnemies = new List<MagnetizedByPlayer>();

        /* Set most of the fields of the player to default values. */
        ResetPlayer();

        BarPos = new Vector2(Screen.width / 6, yEdgeBuffer);
        BarSize = new Vector2(BarWidth, BarHeight);
        BarEmptyTex = new Texture2D(BarWidth, BarHeight);
        BarFullTex = new Texture2D(BarWidth, BarHeight);
        Color32 green = new Color32(0, 255, 0, 255);
        Color32 red = new Color32(255, 0, 0, 255);
        FillTexture(BarEmptyTex, green);
        FillTexture(BarFullTex, red);

        damageStyle.alignment = TextAnchor.MiddleCenter;
        damageStyle.fontSize = 18;
        damageStyle.normal.textColor = Color.white;

        barStyle.border.left = 0;
        barStyle.border.right = 0;
        barStyle.border.bottom = 0;
        barStyle.border.top = 0;

        scoreStyle.fontSize = 18;
        scoreStyle.normal.textColor = Color.white;
        scoreStyle.alignment = TextAnchor.UpperCenter;

        bombStyle.fontSize = 18;
        bombStyle.normal.textColor = Color.white;
        bombStyle.alignment = TextAnchor.UpperRight;

        deadStyle.alignment = TextAnchor.MiddleCenter;
        deadStyle.fontSize = 30;
        deadStyle.normal.textColor = Color.white;
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

        if (mUseBombCooldown > 0.0f)
        {
            mUseBombCooldown -= Time.deltaTime;
        }

        /* Press this key to fire a bomb. */
        if (Input.GetKey(KeyCode.Q))
        {
            if (mUseBombCooldown <= 0.0f && mBombs > 0)
            {
                mUseBomb = true;
                mBombs--;
                mUseBombCooldown = mUseBombCooldownTime;
            }
        }

        if (mNumberCollisions > 0 && !IsDead() && mShieldImmunityTimeLeft <= 0.0f && !mShieldUp)
        {
            mHealth -= 10f;

            //This updates the damage bar to reflect the current health.
            BarProgress = mHealth * (1 / InitialHealth);
        }

        /* Following code deals with setting or reverting the magnetism of the 
         * enemies, if the player has a repelling powerup. It also deals with 
         * decrementing the time that each of these powerups has left. */
        bool repellentPlayer = IsRepellentPlayer();
        bool massRepelling = IsMassRepelling();

        if (repellentPlayer || massRepelling)
        {
            float timeLeft = repellentPlayer ? mRepellentPlayerTimeLeft : mMassRepelTimeLeft;

            float newTimeLeft = timeLeft - Time.deltaTime;

            if (HasJustGotRepelPowerup())
            {
                MagnetizedByPlayer[] individuals = FindObjectsOfType<MagnetizedByPlayer>();

                if (individuals != null)
                {
                    for (int count = 0; count < individuals.Length; ++count)
                    {
                        MagnetizedByPlayer individual = individuals[count];

                        /* The RepellentPlayer powerup will only repel things 
                         * that were previously attracting to the player. It 
                         * will also only repel it by the same force that it 
                         * was previously attracting with. */
                        bool affected = repellentPlayer ? individual.ForceType == MagnetizedByPlayer.Type.Attract : true;

                        if ((individual != null) && !individual.CompareTag("Collectible") && affected)
                        {
                            if (repellentPlayer)
                            {
                                individual.MakeRepelling();
                                mRepellingToAttracting.Add(individual);
                            }
                            else if (massRepelling)
                            {
                                Vector3 difference = individual.transform.position - transform.position;
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
            }
            else if (newTimeLeft < 0.0f)
            {
                List<MagnetizedByPlayer> repellingEnemies = repellentPlayer ? mRepellingToAttracting : mMassRepelEnemies;

                for (int count = 0; count < repellingEnemies.Count; ++count)
                {
                    MagnetizedByPlayer enemy = repellingEnemies[count];

                    if (enemy != null)
                    {
                        enemy.RevertMagnetizeType();
                        if (massRepelling)
                        {
                            enemy.RevertMassRepelForce();
                            enemy.RevertMassRepelDistance();
                        }
                    }
                }

                repellingEnemies.Clear();
            }

            if (repellentPlayer)
            {
                mRepellentPlayerTimeLeft = newTimeLeft;
            }
            else
            {
                mMassRepelTimeLeft = newTimeLeft;
            }
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
                        if (mPickupCooldownTimeLeft <= 0.0f)
                        {
                            mBombs++;
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
                            mRepellentPlayerTimeLeft = mRepellentPowerupTime;
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
        GUI.BeginGroup(new Rect(xEdgeBuffer, yEdgeBuffer, BarPos.x - (2 * xEdgeBuffer), BarHeight - (2*yEdgeBuffer)));
            GUI.Label(new Rect(0, 0, BarPos.x - (2 * xEdgeBuffer), BarHeight - (2 * yEdgeBuffer)), "Damage: ", damageStyle);
        GUI.EndGroup();

        //Empty health bar
        GUI.BeginGroup(new Rect(BarPos.x, BarPos.y, BarWidth, BarHeight));
            GUI.Box(new Rect(0, 0, BarWidth, BarHeight), BarFullTex, barStyle);
            //Health bar filled in by BarProgress amount
            GUI.BeginGroup(new Rect(BarWidth - BarWidth * BarProgress, 0, BarWidth * BarProgress, BarHeight));
                GUI.Box(new Rect(0, 0, BarWidth, BarHeight), BarEmptyTex, barStyle);
            GUI.EndGroup();
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(xEdgeBuffer, yEdgeBuffer, Screen.width - (2 * xEdgeBuffer), Screen.height - (2 * yEdgeBuffer)));
            GUI.Label(new Rect(0, 0, Screen.width - (2 * xEdgeBuffer), Screen.height - (2 * yEdgeBuffer)), "Bombs: " + mBombs.ToString(), bombStyle);
            GUI.Label(new Rect(0, 0, Screen.width - (2 * xEdgeBuffer), Screen.height - (2 * yEdgeBuffer)), "Score: " + mScore.ToString(), scoreStyle);
        GUI.EndGroup();

        if (IsDead())
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Ship failure.", deadStyle);
        }

        //Maybe_TODO: if (mRepellentPlayerTimeLeft > 0.0f) { display timer below player, based on PowerupTime and current mRepellentPlayerTime } and similar for mass repel
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

    internal bool HasJustGotRepellentPowerup()
    {
        return (mRepellentPlayerTimeLeft == mRepellentPowerupTime);
    }

    private bool HasJustGotMassRepelPowerup()
    {
        return (mMassRepelTimeLeft == mMassRepelPowerupTime);
    }

    private bool IsRepellentPlayer()
    {
        return mRepellentPlayerTimeLeft > 0.0f;
    }

    private bool IsMassRepelling()
    {
        return mMassRepelTimeLeft > 0.0f;
    }

    private bool HasJustGotRepelPowerup()
    {
        return (HasJustGotRepellentPowerup() || HasJustGotMassRepelPowerup());
    }

    internal float GetRepellentPowerupTime()
    {
        return mRepellentPowerupTime;
    }

    internal bool UsingBomb()
    {
        return mUseBomb;
    }

    /* Called when a bomb has been successfully used, and the appropriate 
     * enemies have been destroyed. */
    internal void UsedBomb()
    {
        /* Regardless of the bomb radius, all enemies touching the player 
         * should be destroyed. */
        mNumberCollisions = 0;

        mUseBomb = false;
    }

    internal float GetBombRadius()
    {
        return BombRadius;
    }

    /* Set most of the fields, and things like position, of the player to 
     * default starting values. */
    internal void ResetPlayer()
    {
        //TODO: For two players - This may be tricker to change when there are multiple players, like each player will have to store what number player they are and so can work out where they spawn
        transform.position = new Vector3(0.0f, 0.5f, 0.0f); 
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
        mPickupCooldownTimeLeft = 0.0f;
        mUseBomb = false;
        mUseBombCooldown = 0.0f;
    }
}

