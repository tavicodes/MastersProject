// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// [Serializable]
// public class DataObj
// {
//     public int day;
//     public string text;
//     public string subject;
//     public Allow allow;
//     public Deny deny;
// }

// [Serializable]
// public class Allow
// {
//     public DataObj message;
// }

// [Serializable]
// public class Deny
// {
//     public DataObj message;
// }

// [Serializable]
// public class ChangelogEntry
// {
//     public int day;
//     public string text;
// }

// [Serializable]
// public class DataCollection
// { 
//     public int dayCount;
//     public DataObj[] messages;
//     public ChangelogEntry[] changelog;
// }

// public class MessageManager : MonoBehaviour
// {
//     // message variables
//     public TextAsset dataFile;
//     private string fileString;
//     private DataCollection dataColl;

//     private Dictionary<int, List<DataObj>> messageArray;
//     public GameObject messageSpawns;
//     private Transform[] messageSpawnArray;

//     // managing variables
//     private int currentDay;
//     private int messageCount;
//     public GameObject messagePrefab;
//     private List<GameObject> messageObjArray;

//     public GameObject buttonObj;

//     void Awake()
//     {
//         // dataColl = new DataCollection() {dayCount = 3, messages = new Message[1] 
//         // {new Message() {day = 1, text = "hi!", subject = "tester", 
//         // allow = new Allow() {message = new Message() {day = 2, text = "woo", subject = "re:tester", allow = empty, deny = null}},
//         // deny = new Deny() {message = new Message() {day = 2, text = "noo", subject = "re:tester", allow = null, deny = null}}}}};

//         // string JSONData = JsonUtility.ToJson(dataColl);
//         // Debug.Log(JSONData);

//         dataColl = JsonUtility.FromJson<DataCollection>(dataFile.ToString());

//         Debug.Log(dataColl.messages[0].allow.message.text);
//         Debug.Log(dataColl.messages[0].allow.message.allow.message.text);

//         messageSpawnArray = messageSpawns.GetComponentsInChildren<Transform>();

//         currentDay = 0;
//         BuildMessageArray();
//         Debug.Log(messageArray[0][0].allow.message.allow.message.text);
//     }

//     // Start is called before the first frame update
//     void Start()
//     {
//         //SpawnMessages();    
//         buttonObj.SetActive(false);    
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }

//     void BuildMessageArray() 
//     {
//         messageArray = new Dictionary<int, List<DataObj>>();
//         for (int i = 0; i < dataColl.dayCount; i++)
//         {
//             messageArray.Add(i, new List<DataObj>());
//         }
//         foreach (var message in dataColl.messages)
//         {
//             messageArray[message.day - 1].Add(message);
//         }
//     }

//     public void AddMessageToArray(DataObj newMessage)
//     {
//         Debug.Log(messageArray[newMessage.day - 1].Count);

//         messageArray[newMessage.day - 1].Add(newMessage);

//         Debug.Log(newMessage.text);
//         Debug.Log(messageArray[newMessage.day - 1].Count);
//     }

//     void SpawnMessages()
//     {
//         System.Random r = new System.Random();
//         messageSpawnArray = messageSpawnArray.OrderBy(x => r.Next()).ToArray();
    
//         GameObject newMessage;
//         foreach (var message in messageArray[currentDay])
//         {
//             newMessage = Instantiate(messagePrefab.gameObject, messageSpawnArray[messageCount].position, Quaternion.Euler(90, 0, 0), transform) as GameObject;
//             newMessage.GetComponent<MessageInteractable>().SetMessageData(message);

//             newMessage = null;
//             ++messageCount;
//         }
//         ++currentDay;
//     }

//     public void DestroyMessage(DataObj oldMessage)
//     {
//         //messageArray[currentDay - 1].Remove(oldMessage);
//         --messageCount;
//         if (messageCount == 0) buttonObj.SetActive(true);
//     }

//     public void NextDay()
//     {
//         SpawnMessages();
//     }
// }
