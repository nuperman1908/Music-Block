using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelInfo
{
    public int id;
    public string name;
    public int difficulty;
    public string music;
    public float startTime;
    public List<Item> items = new List<Item>();
}
