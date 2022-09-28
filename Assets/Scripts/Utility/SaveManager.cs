using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public GameObject levelDataPrefab;
    public GameObject honeyPrefab;


    public void Save()
    {
        ResourceManager resources = ResourceManager.Instance;
        GameManager manager = GameManager.Instance;
        PlayerData data = new PlayerData
        {
            totalHoney = resources.totalHoney,
            maxStuffing = resources.maxStuffing,
            maxHoney = resources.maxHoney
        };
        //Capture data from each level
        foreach (GameManager.Level level in manager.levels)
        {
            //get reference to honeys on each level
            List<HoneyPickup.Honey> tempHoneyList = new List<HoneyPickup.Honey>();
            foreach (HoneyPickup.Honey honey in level.honeys)
            {
                tempHoneyList.Add(honey);
            }
            data.levels.Add(new GameManager.Level(level.name, level.data, tempHoneyList, level.isComplete, level.completionTime, level.allotedTime));

        }
        string levelJSON = JsonUtility.ToJson(data);
        File.WriteAllText(Application.dataPath + "/SaveData/playerData.txt", levelJSON);
    }

    public void LoadData()
    {
        string path = Application.dataPath + "/SaveData/playerData.txt";
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(data);
            ResourceManager.Instance.maxHoney = playerData.maxHoney;
            ResourceManager.Instance.maxStuffing = playerData.maxStuffing;
            ResourceManager.Instance.totalHoney = playerData.totalHoney;
            foreach (GameManager.Level level in playerData.levels)
            {
                //for each level visited create a level data object
                GameObject levelDataReference = Instantiate(levelDataPrefab);
                //Capture name of the level
                LevelData levelData = levelDataReference.GetComponent<LevelData>();
                levelData.level = level.name;
                levelDataReference.name = level.name + " Data";
                //Capure Completion time
                levelData.completionTime = level.completionTime;
                //Capture if level was completed
                levelData.isComplete = level.isComplete;
                //How long player has to complete the level
                levelData.allotedTime = level.allotedTime;
                //get reference to honeys on each level
                List<HoneyPickup.Honey> tempHoneyList = new List<HoneyPickup.Honey>();
                foreach (HoneyPickup.Honey honey in level.honeys)
                {
                    //for each honey in the level create an instance
                    Vector3 honeyPosition = honey.position;
                    GameObject currentHoney = Instantiate(honeyPrefab, honeyPosition, Quaternion.identity);
                    currentHoney.transform.parent = levelDataReference.transform;
                    //Check to see if honey has already been collected
                    if (honey.pickedUpPrior && levelData.isComplete)
                    {
                        currentHoney.GetComponent<HoneyPickup>().pickedUpPrior = true;
                        tempHoneyList.Add(new HoneyPickup.Honey(true, currentHoney.transform, true));
                    }
                    else
                    {
                        tempHoneyList.Add(new HoneyPickup.Honey(false, currentHoney.transform, false));
                    }


                }
                GameManager.Instance.levels.Add(new GameManager.Level(level.name, levelDataReference, tempHoneyList, levelData.isComplete, levelData.completionTime, levelData.allotedTime));

            }
        }
        else
        {
            Debug.Log("Path does not exist.");
        }
    }
}
