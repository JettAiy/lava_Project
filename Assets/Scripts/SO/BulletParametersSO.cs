using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet parameters base", menuName = "GAME/Bullet parameters")]
public class BulletParametersSO : ScriptableObject
{
    public string bulletName;
    public float impactForce = 300f;
    public Color color;

}
