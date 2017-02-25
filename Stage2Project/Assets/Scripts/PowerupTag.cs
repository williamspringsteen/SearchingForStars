using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* RepellentPlayer will make player repel all enemies that were previosuly attracting, with the same force as before. It will also stop all new enemies spawning while you are repelling.  */
public class PowerupTag : MonoBehaviour
{
    public enum Powerup { Nothing, RepellentPlayer, Bomb, MassRepel, Shield }

    [SerializeField]
    private Powerup PowerupType;

    public Powerup Type { get { return PowerupType; } }
}
