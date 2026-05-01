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
    public float endTime;
    public List<Item> items = new List<Item>();
}

[System.Serializable]
public class Item
{
    public int id;
    public float x;
    public float y;
    public float zRotate;
    public float alpha;
    public int groupId;
}