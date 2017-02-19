using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
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

    private int mNumberCollisions;

    private Rigidbody mBody;

    private float mHealth;

    private int mScore;

    private float mNextUpdateScore;

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

        if (mNumberCollisions > 0 && !IsDead())
        {
            mHealth -= ((1 / Defense) * 10f);
        }

        if (mHealth < InitialHealth && mHealth > 0.5 * InitialHealth)
        {
            print("You have been hit!");
        }
        else if (mHealth <= 0.5 * InitialHealth && mHealth > 0.2 * InitialHealth)
        {
            print("Health is getting low.");
        }
        else if (mHealth <= 0.2 * InitialHealth && mHealth > 0)
        {
            print("Health is dangerously low!!");
        }
        else if (mHealth <= 0)
        {
            print("You are dead.");
            //gameObject.SetActive(false);
            //gameObject.GetComponent(Player).enabled = false;
            //Player player = gameObject.GetComponent(typeof(Player)) as Player;
            //player.enabled = false;
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

    internal void ResetPlayer()
    {
        mHealth = InitialHealth;
        mScore = 0;
        mNumberCollisions = 0;
    }

}
