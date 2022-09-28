using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    public int totalHoney;
    public float maxHoney;
    public float maxStuffing;
    public List<GameManager.Level> levels = new List<GameManager.Level>();
}
