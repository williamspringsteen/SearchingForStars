using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupTag : MonoBehaviour
{
    public enum Powerup { Nothing, RepellentPlayer, Bomb, Bullets, MassRepel, Shield }

    [SerializeField]
    private Powerup PowerupType;

    public Powerup Type { get { return PowerupType; } }
}
