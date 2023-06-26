using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine;

using TMPro;

public class EditorManager : MonoBehaviour
{
    public TextAsset dataFile;
    private string fileString;
    private DataCollection dataColl;
    private List<DataObj> messageList;
    private List<DataObj> spamList;
    private List<DataObj> activeList;
    private List<string> dropdownMessageList;
    private List<string> dropdownSpamList;
    private List<string> dropdownActiveList;
    private bool messageActive;

    public TMP_Text textSwitch;
    public TMP_InputField  inputPrimary;
    public TMP_InputField  inputSecondary;
    public TMP_InputField  inputTertiary;
    public TMP_InputField  inputDay;
    public TMP_Dropdown dropdownParent;
    public TMP_Dropdown dropdownAllow;
    public TMP_Dropdown dropdownDeny;
    public TMP_Dropdown dropdownSelect;
    public Toggle toggleResponse;

    // Start is called before the first frame update
    void Start()
    {
        dataColl = JsonUtility.FromJson<DataCollection>(dataFile.ToString());

        messageList = new List<DataObj>(){new DataObj(){primary = "empty"}};
        spamList = new List<DataObj>(){new DataObj(){primary = "empty"}};
        dropdownMessageList = new List<string>(){"empty"};
        dropdownSpamList = new List<string>(){"empty"};
        foreach (var message in dataColl.messages)
        {
            messageList.Add(message);
            dropdownMessageList.Add(message.primary.Length > 65 ? message.primary.Substring(0, 40) : message.primary);
        }
        foreach (var message in dataColl.spam)
        {
            spamList.Add(message);
            dropdownSpamList.Add(message.primary.Length > 65 ? message.primary.Substring(0, 40) : message.primary);
        }

        activeList = messageList;
        dropdownActiveList = dropdownMessageList;
        messageActive = true;
        textSwitch.text = "Messages";
        FillDropdowns(1);
    }

    void FillDropdowns(int value)
    {
        dropdownSelect.ClearOptions();
        dropdownSelect.AddOptions(dropdownActiveList);
        dropdownSelect.value = value;
        dropdownParent.ClearOptions();
        dropdownParent.AddOptions(dropdownActiveList);
        dropdownAllow.ClearOptions();
        dropdownAllow.AddOptions(dropdownActiveList);
        dropdownDeny.ClearOptions();
        dropdownDeny.AddOptions(dropdownActiveList);
        FillValues();
    }

    public void FlipActive()
    {
        messageActive = !messageActive;
        activeList = messageActive ? messageList : spamList;
        dropdownActiveList = messageActive ? dropdownMessageList : dropdownSpamList;
        textSwitch.text = messageActive ? "Messages" : "Spam";
        FillDropdowns(1);
    }

    public void FillValues()
    {
        inputDay.text = activeList[dropdownSelect.value].day.ToString();
        inputPrimary.text = activeList[dropdownSelect.value].primary;
        inputSecondary.text = activeList[dropdownSelect.value].secondary;
        inputTertiary.text = activeList[dropdownSelect.value].tertiary;
        dropdownParent.value = Math.Abs(activeList[dropdownSelect.value].parent);
        dropdownAllow.value = Math.Abs(activeList[dropdownSelect.value].allow);
        dropdownDeny.value = Math.Abs(activeList[dropdownSelect.value].deny);
        toggleResponse.isOn = activeList[dropdownSelect.value].response;
    }

    public void FillFromParent()
    {
        inputSecondary.text = activeList[dropdownParent.value].secondary;
        inputTertiary.text = activeList[dropdownParent.value].tertiary;
    }

    public void AddValue()
    {
        DataObj tempObj = new DataObj();
        activeList.Add(tempObj);
        dropdownActiveList.Add("");

        FillDropdowns(dropdownActiveList.Count - 1);
    }

    public void Submit()
    {
        if (dropdownSelect.value == 0) return;
        dropdownActiveList[dropdownSelect.value] = inputPrimary.text.Length > 40 ? inputPrimary.text.Substring(0, 40) : inputPrimary.text;
        activeList[dropdownSelect.value] = new DataObj() {
            id = dropdownSelect.value * (messageActive ? 1 : -1), day = Convert.ToInt32(inputDay.text), response = toggleResponse.isOn,
            primary = inputPrimary.text, secondary = inputSecondary.text, tertiary = inputTertiary.text,
            parent = dropdownParent.value * (messageActive ? 1 : -1), allow = dropdownAllow.value * (messageActive ? 1 : -1), deny = dropdownDeny.value * (messageActive ? 1 : -1)
        };

        FillDropdowns(dropdownSelect.value);
    }

    public void Save()
    {        
        DataObj[] tempArr = new DataObj[messageList.Count - 1];
        messageList.CopyTo(1,tempArr, 0, messageList.Count - 1);
        dataColl.messages = tempArr;
        tempArr = new DataObj[spamList.Count - 1];
        spamList.CopyTo(1,tempArr, 0, spamList.Count - 1);
        dataColl.spam = tempArr;
        File.WriteAllText("Assets/GameAssets/Data/data_new.json", JsonUtility.ToJson(dataColl));
    }
}
