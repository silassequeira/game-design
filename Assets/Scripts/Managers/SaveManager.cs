// Save/load system (JSON/PlayerPrefs)[System.Serializable]
public class SaveData {
    public int currentLevel;
    public Vector3 lastCheckpoint;
    public int collectibles;
}

public void SaveGame() {
    SaveData data = new SaveData {
        currentLevel = GameManager.CurrentLevel,
        lastCheckpoint = Player.transform.position,
        collectibles = PlayerState.Collectibles
    };
    string json = JsonUtility.ToJson(data);
    System.IO.File.WriteAllText(savePath, json);
}


//Checkpoint System

//Use trigger colliders with Checkpoint tags
//Save game state when player enters trigger
//Visual indicator (flag/light) for active checkpoint