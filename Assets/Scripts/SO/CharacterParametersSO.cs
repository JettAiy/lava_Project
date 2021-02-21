using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Character parameters base", menuName ="GAME/Character parameters")]
public class CharacterParametersSO : ScriptableObject
{
    public byte team = 1;
    public float movementSpeed = 10f;
    public float atackSpeed = 5f;

}
