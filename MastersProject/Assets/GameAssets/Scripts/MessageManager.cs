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
    public int chapter;
    public bool response;
    public string primary;
    public string secondary;
    public string tertiary;
    public int parent;
    public int allow;
    public int deny;
}

[Serializable]
public struct ChapterInfo
{
    public int chapter_count;
    public int[] day_length;
    public bool[] spam_count;
    public bool[] love_count;
}

[Serializable]
public struct SpamCollection
{
    public DataObj[] junk;
    public DataObj[] responses;
    public DataObj[] love;
}

[Serializable]
public struct DataCollection
{ 
    public ChapterInfo chapter_info;
    public DataObj[] messages;
    public SpamCollection spam;
    public DataObj[] responses;
}

public class MessageManager : MonoBehaviour
{
    // message variables
    public TextAsset dataFile;
    private string fileString;
    private DataCollection dataColl;

    private Dictionary<int, List<DataObj>>[] messageDayArray;
    private Dictionary<int, DataObj> messageArray;
    private List<DataObj> spamArray;
    private Dictionary<int, DataObj> responseArray;
    private List<DataObj> loveArray;
    public GameObject messageSpawns;
    private Transform[] messageSpawnArray;

    // managing variables
    private int currentChapter;
    private int currentDay;
    private int messageCount;
    private int spamCount;
    private int loveCount;
    private static System.Random rng = new System.Random();
    public GameObject messagePrefab;
    public GameObject buttonObj;

    void Awake()
    {
        dataColl = JsonUtility.FromJson<DataCollection>(dataFile.ToString());

        messageSpawnArray = messageSpawns.GetComponentsInChildren<Transform>();

        currentChapter = -1;
        currentDay = 0;
        spamCount = 0;
        loveCount = 0;
        BuildMessageArrays();
        Debug.Log("Messages Built");
    }

    // Start is called before the first frame update
    void Start()
    {
        StartChapter();
        buttonObj.SetActive(false);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BuildMessageArrays() 
    {
        messageDayArray = new Dictionary<int, List<DataObj>>[dataColl.chapter_info.chapter_count];
        messageArray = new Dictionary<int, DataObj>();
        spamArray = new List<DataObj>();
        responseArray = new Dictionary<int, DataObj>();
        loveArray = new List<DataObj>();


        for (int i = 0; i < dataColl.chapter_info.chapter_count; i++)
        {
            messageDayArray[i] = new Dictionary<int, List<DataObj>>();
            for (int j = 1; j <= dataColl.chapter_info.day_length[i]; j++)
            {
                messageDayArray[i].Add(j, new List<DataObj>());
            }
        }

        foreach (var message in dataColl.messages)
        {
            messageArray.Add(message.id, message);
            if (!message.response) messageDayArray[message.chapter][message.day].Add(message);            
        }

        foreach (var spam in dataColl.spam.junk)
        {
            spamArray.Add(spam);       
        }
        spamArray = spamArray.OrderBy(a => rng.Next()).ToList();
        
        foreach (var response in dataColl.spam.responses)
        {
            responseArray.Add(response.id, response);       
        }

        foreach (var love in dataColl.spam.love)
        {
            loveArray.Add(love);       
        }
        loveArray = loveArray.OrderBy(a => rng.Next()).ToList();

        Debug.Log(messageDayArray[0].Count);
    }

    public DataObj GetParentData(int parentID)
    {
        if (parentID > 0) return messageArray[parentID];
        else return spamArray[-parentID];
    }

    void StartChapter()
    {
        ++currentChapter;
        Debug.Log("Chapter: " + currentChapter);
        currentDay = 0;

        if (currentChapter == dataColl.chapter_info.chapter_count) 
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        SpawnMessages();
    }

    void SpawnMessages()
    {
        ++currentDay;
        Debug.Log(String.Format("Day: {0}, Valid: {1}\nArray Length: {2}, JSON Length: {3}", currentDay, (messageDayArray[currentChapter].ContainsKey(currentDay)), messageDayArray[currentChapter].Count, dataColl.chapter_info.day_length[currentChapter]));

        if (currentDay > messageDayArray[currentChapter].Count)
        {
            StartChapter();
            return;
        }

        if (currentDay <= dataColl.chapter_info.day_length[currentChapter])
            {
                if (dataColl.chapter_info.spam_count[currentChapter])
                {
                    DataObj tempSpam = spamArray[spamCount];
                    tempSpam.id = -spamCount;
                    ++spamCount;
                    messageDayArray[currentChapter][currentDay].Add(tempSpam);
                }
                if (dataColl.chapter_info.love_count[currentChapter])
                {
                    messageDayArray[currentChapter][currentDay].Add(loveArray[loveCount++]);
                }
            }

        List<Transform> tempPos = messageSpawnArray.OrderBy(a => rng.Next()).ToList();

        GameObject newMessage;
        foreach (var message in messageDayArray[currentChapter][currentDay])
        {
            newMessage = Instantiate(messagePrefab.gameObject, tempPos[messageCount].position, Quaternion.Euler(90, 0, 0), transform) as GameObject;
            newMessage.GetComponent<MessageInteractable>().SetMessageData(message);

            newMessage = null;
            ++messageCount;
        }

        Debug.Log(String.Format("Messages Expected: {0}, Actual: {1}", messageDayArray[currentChapter][currentDay].Count, messageCount));
    }

    public void AddMessageToArray(bool isAllow, DataObj oldMessage)
    {
        DataObj response;
        if (isAllow && oldMessage.allow != 0) 
        {
            response = (oldMessage.id > 0) ? messageArray[oldMessage.allow] : responseArray[oldMessage.allow];
            //Debug.Log(String.Format("Old ID: {0}, new ID: {1}", oldMessage.id, oldMessage.allow));
        }
        else if (!isAllow && oldMessage.deny != 0) 
        {
            response = (oldMessage.id > 0) ? messageArray[oldMessage.deny] : responseArray[oldMessage.deny];
            //Debug.Log(String.Format("Old ID: {0}, new ID: {1}", oldMessage.id, oldMessage.allow));
        }
        else return;

        if (response.parent == 0) response.parent = oldMessage.id;
        if (response.secondary == null) response.secondary = "re: " + oldMessage.secondary;     
        if (response.tertiary == null) response.tertiary = oldMessage.tertiary;     
        
        int newDay;
        int newChapter = currentChapter;
        if (oldMessage.id > 0) 
        {
            newDay = response.day + (response.response ? currentDay : 0);
            while (newDay > messageDayArray[currentChapter].Count)
            {
                messageDayArray[currentChapter].Add(messageDayArray[currentChapter].Count + 1, new List<DataObj>());
            }
        }
        else 
        {
            newDay = response.day + currentDay;
            if (newDay > dataColl.chapter_info.day_length[currentChapter]) 
            {
                newDay = newDay - dataColl.chapter_info.day_length[currentChapter];
                newChapter++;
            }
        }
        messageDayArray[newChapter][newDay].Add(response);
    }

    public void DestroyMessage()
    {
        --messageCount;
        if (messageCount == 0) buttonObj.SetActive(true);
    }

    public void NextDay()
    {
        SpawnMessages();
    }
}
