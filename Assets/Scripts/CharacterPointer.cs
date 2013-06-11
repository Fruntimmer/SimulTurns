using UnityEngine;
using System.Collections;

public class CharacterPointer : MonoBehaviour
{
    public Character character;
    void EnemySighted(Character enemy)
    {
        character.EnemySighted(enemy);   
    }
    void EnemyOutOfSight()
    {
        character.EnemyOutOfSight();
    }
}
