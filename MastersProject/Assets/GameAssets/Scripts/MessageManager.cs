using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[Serializable]
public struct DataObj
{
    public int id;
    public int day;
    public bool response;
    public string primary;
    public string secondary;
    public string tertiary;
    public int parent;
    public int allow;
    public int deny;
}

[Serializable]
public struct DataCollection
{ 
    public string[] day_names;
    public DataObj[] messages;
    public DataObj[] spam;
    public DataObj[] responses;
    public DataObj[] changelog;
}

public class MessageManager : MonoBehaviour
{
    // message variables
    public TextAsset dataFile;
    private string fileString;
    private DataCollection dataColl;

    private Dictionary<int, List<DataObj>> messageDayArray;
    private Dictionary<int, DataObj> messageArray;
    private Dictionary<int, DataObj> spamArray;
    public GameObject messageSpawns;
    private Transform[] messageSpawnArray;

    public GameObject changelogObj;
    private TMP_Text changelogText;

    // managing variables
    private int currentDay;
    private int messageCount;
    private static System.Random rng = new System.Random();
    public GameObject messagePrefab;
    public GameObject buttonObj;

    void Awake()
    {
        dataColl = JsonUtility.FromJson<DataCollection>(dataFile.ToString());

        messageSpawnArray = messageSpawns.GetComponentsInChildren<Transform>();
        changelogText = changelogObj.GetComponentInChildren<TMP_Text>();
        changelogText.text = "";

        currentDay = -1;
        BuildMessageArrays();
        Debug.Log("Messages Built");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Spawning Messages");
        SpawnMessages();    
        Debug.Log("Messages Spawned");
        buttonObj.SetActive(false);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BuildMessageArrays() 
    {
        messageDayArray = new Dictionary<int, List<DataObj>>();
        messageArray = new Dictionary<int, DataObj>();
        spamArray = new Dictionary<int, DataObj>();

        for (int i = 0; i < dataColl.day_names.Length; i++)
        {
            messageDayArray.Add(i, new List<DataObj>());
        }

        foreach (var message in dataColl.messages)
        {
            messageArray.Add(message.id, message);
            if (!message.response) messageDayArray[message.day].Add(message);            
        }

        foreach (var spam in dataColl.spam)
        {
            spamArray.Add(spam.id, spam);
        }

        Debug.Log(messageDayArray[0].Count);
    }

    public DataObj GetParentData(int parentID)
    {
        if (parentID > 0) return messageArray[parentID];
        else return spamArray[parentID];
    }

    void SpawnMessages()
    {
        Debug.Log("Spawn Messages");
        ++currentDay;

        if (currentDay == dataColl.day_names.Length) 
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }

        List<DataObj> tempSpamArr = spamArray.Values.OrderBy(a => rng.Next()).ToList();

        int spamTotal = rng.Next(1,3);
        int spamOffset = 0;
        for (int i = 0; i < spamTotal; i++)
        {
            DataObj tempSpamObj;
            do
            {
                tempSpamObj = tempSpamArr[spamOffset];
                spamOffset++;
            } while ((spamOffset < tempSpamArr.Count) && tempSpamObj.response);
            if (spamOffset == tempSpamArr.Count) break;

            messageDayArray[currentDay].Add(tempSpamObj);
        }

        List<Transform> tempPos = messageSpawnArray.OrderBy(a => rng.Next()).ToList();

        GameObject newMessage;
        foreach (var message in messageDayArray[currentDay])
        {
            newMessage = Instantiate(messagePrefab.gameObject, tempPos[messageCount].position, Quaternion.Euler(90, 0, 0), transform) as GameObject;
            newMessage.GetComponent<MessageInteractable>().SetMessageData(message);

            newMessage = null;
            ++messageCount;
        }

        int changelogOffset = 0;
        string newEntry = "";
        changelogText.text = "";
        while ((changelogOffset < dataColl.changelog.Length) && (dataColl.changelog[changelogOffset].day <= currentDay))
        {
            newEntry = string.Format("\n<size=120%><b>{0}</b>\n<size=100%>{1}\n<size=80%><i>{2}</i>\n ", 
                dataColl.day_names[dataColl.changelog[changelogOffset].day], dataColl.changelog[changelogOffset].primary, dataColl.changelog[changelogOffset].secondary);
            changelogText.text = newEntry + changelogText.text;
            ++changelogOffset;
        }
    }

    public void AddMessageToArray(bool isAllow, DataObj oldMessage)
    {
        DataObj response;
        if (isAllow && oldMessage.allow != 0) 
        {
            response = oldMessage.id > 0 ? messageArray[oldMessage.allow] : spamArray[oldMessage.allow];
            Debug.Log(String.Format("Old ID: {0}, new ID: {1}", oldMessage.id, oldMessage.allow));
        }
        else if (!isAllow && oldMessage.deny != 0) 
        {
            response = oldMessage.id > 0 ? messageArray[oldMessage.deny] : spamArray[oldMessage.deny];
            Debug.Log(String.Format("Old ID: {0}, new ID: {1}", oldMessage.id, oldMessage.allow));
        }
        else return;

        if (response.parent != oldMessage.id) response.parent = oldMessage.id;
        if (response.secondary == null) response.secondary = "re: " + oldMessage.secondary;     

        int newDay = response.day + (response.response ? currentDay : 0);
        if (newDay < dataColl.day_names.Length) messageDayArray[newDay].Add(response);
    }

    public void DestroyMessage()
    {
        --messageCount;
        if (messageCount == 0) buttonObj.SetActive(true);
    }

    public void NextDay()
    {
        Debug.Log("New Day");
        SpawnMessages();
    }
}
