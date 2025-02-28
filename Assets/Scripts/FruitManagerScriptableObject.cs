using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "FruitGame/FruitManagerScriptableObject", order = 1)]
public class FruitManagerScriptableObject : ScriptableObject
{
    [SerializeField]
    public List<FruitPlateCfg> fruitPlateCfgs;

    private Dictionary<(EFruitType, EPlateState), Sprite> fruitPlateSprites = new();

    public void Init()
    {
        foreach (var item in fruitPlateCfgs)
        {
            fruitPlateSprites.Add((item.fruitType, item.plateState), item.value);
        }
    }

    public Sprite GetSprite(EFruitType fruitType, EPlateState plateState)
    {
        return fruitPlateSprites[(fruitType, plateState)];
    }
}

[Serializable]
public class FruitPlateCfg
{
    public EFruitType fruitType;
    public EPlateState plateState;

    public Sprite value;
}