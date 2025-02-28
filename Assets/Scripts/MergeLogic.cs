using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class MergeLogic
{
    public FruitPlate[,] FruitPlates;

    int columnsCount;
    int rowsCount;

    private int xOffset = 12;
    private int yOffset = 20;

    public int TriesForHasMatchGen = 100; //Чем меньше, чем чаше при генерации будут дубликаты
    public int TriesForMatchGen = 3; //Сколько раз можожет быть генерация с дубликатами

    public bool CanPlay { get; private set; } = false;

    public MergeLogic(int columnsCount, int rowsCount)
    {
        this.columnsCount = columnsCount;
        this.rowsCount = rowsCount;

        FruitPlates = new FruitPlate[columnsCount, rowsCount];
    }

    public void InitGame()
    {
        for (int j = 0; j < rowsCount; j++)
        {
            for (int i = 0; i < columnsCount; i++)
            {
                var fruitType = Extensions.RandomEnum<EFruitType>();
                var plateState = Extensions.RandomEnum<EPlateState>();

                var fruitPlate = new FruitPlate(
                    i, j, 
                    GamePanel.instance.InstantiateFruitPlate(),
                    new Vector2(i * 100 + i * xOffset, j * 100 + j * yOffset)
                    );
                fruitPlate.View.canvasGroup.alpha = 0f;
                FruitPlates[i, j] = fruitPlate;
            }
        }
    }

    public async UniTask GenGame(int seed)
    {
        Random.InitState(seed);
        Debug.Log($"Gen game with seed: {seed}");

        SpawnNewPlates(-1);
        await ReturnPosAllPlates();

        CanPlay = true;
    }

    bool TrySwapAndCheck(FruitPlate fruitPlateA, FruitPlate fruitPlateB)
    {
        SwapHash(fruitPlateA, fruitPlateB);
        bool hasMatch = HasMatch();
        SwapHash(fruitPlateA, fruitPlateB);
        return hasMatch;
    }

    void SwapHash(FruitPlate fruitPlateA, FruitPlate fruitPlateB)
    {
        int temp = fruitPlateA.FruitHash;
        fruitPlateA.FruitHash = fruitPlateB.FruitHash;
        fruitPlateB.FruitHash = temp;
    }

    bool HasMatch()
    {
        for (int r = 0; r < columnsCount; r++)
        {
            for (int c = 0; c < rowsCount - 2; c++)
            {
                if (FruitPlates[r, c].FruitHash < 0) continue;

                if (FruitPlates[r, c].FruitHash == FruitPlates[r, c + 1].FruitHash && FruitPlates[r, c].FruitHash == FruitPlates[r, c + 2].FruitHash)
                    return true;
            }
        }

        for (int c = 0; c < rowsCount; c++)
        {
            for (int r = 0; r < columnsCount - 2; r++)
            {
                if (FruitPlates[r, c].FruitHash < 0) continue;

                if (FruitPlates[r, c].FruitHash == FruitPlates[r + 1, c].FruitHash && FruitPlates[r, c].FruitHash == FruitPlates[r + 2, c].FruitHash)
                    return true;
            }
        }

        return false;
    }

    bool HasNullHash()
    {
        for (int r = 0; r < columnsCount; r++)
        {
            for (int c = 0; c < rowsCount; c++)
            {
                if (FruitPlates[r, c].FruitHash < 0) return true;
            }
        }

        return false;
    }

    async UniTask ReturnPosAllPlates()
    {
        List<UniTask> forAwait = new();

        foreach (var item in FruitPlates)
        {
            forAwait.Add(item.ReturnPos(.75f));
        }

        await forAwait;
    }

    void FallDownPlates()
    {
        int needFruitsForColumn;

        for (int i = 0; i < columnsCount; i++)
        {
            needFruitsForColumn = 0;

            for (int j = 0; j < rowsCount; j++)
            {
                if (FruitPlates[i, j].FruitHash < 0)
                {
                    needFruitsForColumn++;
                }
                else
                {
                    FruitPlates[i, j].SetNullHash();
                    FruitPlates[i, j].View.canvasGroup.alpha = 0f;

                    var downFruit = FruitPlates[i, j - needFruitsForColumn];
                    downFruit.SetFruits(FruitPlates[i, j].fruitType, FruitPlates[i, j].plateState);
                    downFruit.View.canvasGroup.alpha = 1f;
                    downFruit.View.rectTransform.anchoredPosition = FruitPlates[i, j].View.rectTransform.anchoredPosition;
                }
            }
        }
    }

    void SpawnNewPlates(int hasMatchTries)
    {
        int spawnTries = 0;
        int needFruitsForColumn;

        for (int i = 0; i < columnsCount; i++)
        {
            needFruitsForColumn = 0;

            for (int j = 0; j < rowsCount; j++)
            {
                if (FruitPlates[i, j].FruitHash < 0)
                {
                    needFruitsForColumn++;
                }
            }

            for (int y = rowsCount - needFruitsForColumn; y < rowsCount; y++)
            {
                var downFruit = FruitPlates[i, y];

                do
                {
                    var fruitType = Extensions.RandomEnum<EFruitType>();
                    var plateState = EPlateState.Base; //Extensions.RandomEnum<EPlateState>();
                    downFruit.SetFruits(fruitType, plateState);
                    if (hasMatchTries > 0)
                    {
                        hasMatchTries--;
                    }
                    spawnTries++;
                }
                while ((hasMatchTries > 0 || hasMatchTries == -1) && HasMatch() && spawnTries < 200);

                downFruit.View.canvasGroup.alpha = 1f;
                int j = y + needFruitsForColumn + 1;
                downFruit.View.rectTransform.anchoredPosition = new Vector2(i * 100 + i * xOffset, j * 100 + j * yOffset);
            }
        }

        Debug.Log($"SpawnNewPlates spawnTries: {spawnTries}");
    }

    async UniTask FindAndCollectMatches()
    {
        bool[,] matches = new bool[FruitPlates.GetLength(0), FruitPlates.GetLength(1)];
        FindMatches(matches);

        for (int j = 0; j < rowsCount; j++)
        {
            for (int i = 0; i < columnsCount; i++)
            {
                if (matches[i, j])
                {
                    await FruitPlates[i, j].Collect();
                }
            }
        }
    }

    public async UniTask<bool> TryMerge(FruitPlate fruitPlateA, FruitPlate fruitPlateB)
    {
        if (!CanPlay) return false;
        CanPlay = false;

        fruitPlateA.View.rectTransform.anchoredPosition = fruitPlateA.currentPos;
        fruitPlateB.View.rectTransform.anchoredPosition = fruitPlateB.currentPos;

        SwitchSprites(fruitPlateA, fruitPlateB);

        if (HasMatch())
        {
            var genTries = Random.Range(0, TriesForMatchGen);

            await FindAndCollectMatches();

            do
            {
                FallDownPlates();
                SpawnNewPlates(genTries > 0 ? TriesForHasMatchGen : -1);
                await ReturnPosAllPlates();
                await FindAndCollectMatches();

                genTries--;
            }
            while (HasNullHash());

            CanPlay = true;
            return true;
        }

        SwitchSprites(fruitPlateA, fruitPlateB);

        CanPlay = true;
        return false;
    }

    void FindMatches(bool[,] matches)
    {
        for (int r = 0; r < columnsCount; r++)
        {
            for (int c = 0; c < rowsCount - 2; c++)
            {
                if (FruitPlates[r, c].FruitHash < 0) continue;

                if (FruitPlates[r, c].FruitHash == FruitPlates[r, c + 1].FruitHash && FruitPlates[r, c].FruitHash == FruitPlates[r, c + 2].FruitHash)
                {
                    matches[r, c] = matches[r, c + 1] = matches[r, c + 2] = true;
                }
            }
        }

        for (int c = 0; c < rowsCount; c++)
        {
            for (int r = 0; r < columnsCount - 2; r++)
            {
                if (FruitPlates[r, c].FruitHash < 0) continue;

                if (FruitPlates[r, c].FruitHash == FruitPlates[r + 1, c].FruitHash && FruitPlates[r, c].FruitHash == FruitPlates[r + 2, c].FruitHash)
                {
                    matches[r, c] = matches[r + 1, c] = matches[r + 2, c] = true;
                }
            }
        }
    }

    void SwitchSprites(FruitPlate fruitPlateA, FruitPlate fruitPlateB)
    {
        var spritesA = (fruitPlateA.fruitType, fruitPlateA.plateState);
        fruitPlateA.SetFruits(fruitPlateB.fruitType, fruitPlateB.plateState);
        fruitPlateB.SetFruits(spritesA.fruitType, spritesA.plateState);
    }
}