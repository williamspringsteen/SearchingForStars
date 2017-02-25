using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* RepellentPlayer will make player repel all enemies that were previosuly 
 * attracting, with the same force as before. It will also stop all new enemies
 * spawning while you are repelling. MassRepel will strongly repel away ALL 
 * enemies withing a certain radius, for a very short time. Bomb will give the 
 * player a bomb, which will destroy all enemies in a certain radius when used.
 * Shield gives the player a protective shield, that stops them getting hurt. 
 * Upon being hurt, the shield disappears, and the player has two seconds until
 * they start being hurt again. */
public class PowerupTag : MonoBehaviour
{
    public enum Powerup { Nothing, RepellentPlayer, Bomb, MassRepel, Shield }

    [SerializeField]
    private Powerup PowerupType;

    public Powerup Type { get { return PowerupType; } }
}
