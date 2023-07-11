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
    private List<DataObj> junkList;
    private List<DataObj> loveList;
    private List<DataObj> activeList;
    private List<string> dropdownMessageList;
    private List<string> dropdownJunkList;
    private List<string> dropdownLoveList;
    private List<string> dropdownActiveList;
    private int chapterActive;
    private int messageActive;

    public TMP_Text textSwitch;
    public TMP_InputField  inputPrimary;
    public TMP_InputField  inputSecondary;
    public TMP_InputField  inputTertiary;
    public TMP_InputField  inputDay;
    public TMP_InputField  inputChapter;
    public TMP_Dropdown dropdownParent;
    public TMP_Dropdown dropdownAllow;
    public TMP_Dropdown dropdownDeny;
    public TMP_Dropdown dropdownSelect;
    public Toggle toggleResponse;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 5;

        dataColl = JsonUtility.FromJson<DataCollection>(dataFile.ToString());

        messageList = new List<DataObj>(){new DataObj(){primary = "empty"}};
        junkList = new List<DataObj>(){new DataObj(){primary = "empty"}};
        loveList = new List<DataObj>(){new DataObj(){primary = "empty"}};
        dropdownMessageList = new List<string>(){"empty"};
        dropdownJunkList = new List<string>(){"empty"};
        dropdownLoveList = new List<string>(){"empty"};

        foreach (var message in dataColl.messages)
        {
            messageList.Add(message);
            dropdownMessageList.Add(message.secondary.Length > 30 ? message.secondary.Substring(0, 30) : message.secondary);
        }
        foreach (var message in dataColl.spam.junk)
        {
            junkList.Add(message);
            dropdownJunkList.Add(message.secondary.Length > 30 ? message.secondary.Substring(0, 30) : message.secondary);
        }
        foreach (var message in dataColl.spam.love)
        {
            loveList.Add(message);
            dropdownLoveList.Add(message.secondary.Length > 30 ? message.secondary.Substring(0, 30) : message.secondary);
        }

        activeList = messageList;
        dropdownActiveList = dropdownMessageList;
        messageActive = 0;
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

    public void SwitchMessage(bool positive)
    {
        Submit();
        dropdownSelect.value = Math.Clamp(dropdownSelect.value + (positive ? 1 : -1), 1, dropdownActiveList.Count);
        FillValues();
    }

    public void FlipActive()
    {
        messageActive++;
        switch (messageActive)
        {
            default:
                messageActive = 0;
                goto case 0;
            case 0:
                activeList = messageList;
                dropdownActiveList = dropdownMessageList;
                textSwitch.text = "Messages";
                break;
            case 1:
                activeList = junkList;
                dropdownActiveList = dropdownJunkList;
                textSwitch.text = "Junk";
                break;
            case 2:
                activeList = loveList;
                dropdownActiveList = dropdownLoveList;
                textSwitch.text = "Love";
                break;
        }
        
        FillDropdowns(1);
    }

    public void FillValues()
    {
        inputDay.text = activeList[dropdownSelect.value].day.ToString();
        inputChapter.text = activeList[dropdownSelect.value].chapter.ToString();
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
        if (dropdownActiveList[dropdownParent.value] == "empty") return;
        inputChapter.text = activeList[dropdownParent.value].chapter.ToString();
        //inputSecondary.text = "Re: " + activeList[dropdownParent.value].secondary;
        //inputTertiary.text = activeList[dropdownParent.value].tertiary;
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
        dropdownActiveList[dropdownSelect.value] = inputSecondary.text.Length > 30 ? inputSecondary.text.Substring(0, 30) : inputSecondary.text;
        activeList[dropdownSelect.value] = new DataObj() {
            id = dropdownSelect.value + (messageActive * 100), day = Convert.ToInt32(inputDay.text), chapter = Convert.ToInt32(inputChapter.text), response = toggleResponse.isOn,
            primary = inputPrimary.text, secondary = inputSecondary.text, tertiary = inputTertiary.text,
            parent = dropdownParent.value + (messageActive * 100), allow = dropdownAllow.value + (messageActive * 100), deny = dropdownDeny.value + (messageActive * 100)
        };

        FillDropdowns(dropdownSelect.value);
    }

    public void Save()
    {        
        DataObj[] tempArr = new DataObj[messageList.Count - 1];
        messageList.CopyTo(1,tempArr, 0, messageList.Count - 1);
        dataColl.messages = tempArr;
        tempArr = new DataObj[junkList.Count - 1];
        junkList.CopyTo(1,tempArr, 0, junkList.Count - 1);
        dataColl.spam.junk = tempArr;
        tempArr = new DataObj[loveList.Count - 1];
        loveList.CopyTo(1,tempArr, 0, loveList.Count - 1);
        dataColl.spam.love = tempArr;
        File.WriteAllText("Assets/GameAssets/Data/data_new.json", JsonUtility.ToJson(dataColl));
    }
}
