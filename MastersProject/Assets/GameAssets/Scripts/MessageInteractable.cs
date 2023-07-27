using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;
using TMPro;

[RequireComponent( typeof( Interactable ) )]
public class MessageInteractable : MonoBehaviour
{
    private TMP_Text generalText;
    private TMP_Text sideTopText;
    private TMP_Text sideBotText;
    private TMP_Text bottomText;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 oldPosition;
    private Quaternion oldRotation;

    private DataObj messageData;
    private string messageText;
    private string subjectText;
    private string senderText;
    public int lineLength;

    public GameObject ghostPrefab;
    public GameObject ghostObj;

    private MessageManager parentScript;
    private MessageDestination destinationScript;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & ( ~Hand.AttachmentFlags.SnapOnAttach ) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
    private bool isGrabbed;

    private Interactable interactable;

    //-------------------------------------------------
    void Awake()
    {
        var textMeshs = GetComponentsInChildren<TMP_Text>();
        generalText = textMeshs[0];
        sideTopText = textMeshs[1];
        sideBotText = textMeshs[2];
        bottomText = textMeshs[3];

        generalText.text = "";
        sideTopText.text = "";
        sideBotText.text = "";
        bottomText.text = "";

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        isGrabbed = false;

        parentScript = GetComponentInParent<MessageManager>();
        interactable = this.GetComponent<Interactable>();
    }

    private string ResolveTextSize(string input)
    {
        string[] words = input.Split(" "[0]);
        string result = "";
        string line = "";
        
        foreach(string s in words){
            string temp = line + " " + s;
            
            if(temp.Length > lineLength)
            {
                result += line + "\n";
                line = s;
            }
            else 
            {
                line = temp;
            }
        }
        
        result += line;
        return result.Substring(1,result.Length-1);
    }

    void SpawnGhostChild(int newID)
    {
        int parentID = newID;
        int offset = 1;

        DataObj tempParent;
        GameObject tempObj;
        while (parentID != 0)
        {
            tempParent = parentScript.GetParentData(parentID);
            tempObj = Instantiate(ghostPrefab.gameObject, transform.position + new Vector3(-0.15f * offset, 0, -0.05f), Quaternion.Euler(90, 0, 0), ghostObj.transform) as GameObject;
            tempObj.GetComponent<MessageInteractable>().SetGhostData(tempParent);

            parentID = tempParent.parent;
            //Debug.Log(parentID);
            offset++;
        }

        //Debug.Log("Offset: " + offset.ToString());

        ghostObj.SetActive(false);
    }

    public void SetMessageData(DataObj newMessage)
    {
        messageData = newMessage;
        messageText = messageData.primary;
        subjectText = messageData.secondary;
        senderText = messageData.tertiary;

        generalText.text = subjectText;
        bottomText.text = senderText;

        if (messageData.parent != 0) SpawnGhostChild(messageData.parent);
    }
    
    public void SetGhostData(DataObj newMessage)
    {
        messageData = newMessage;
        messageText = messageData.primary;
        subjectText = messageData.secondary;
        senderText = messageData.tertiary;

        generalText.text = string.Format("<size=70%>{0}", subjectText);
        bottomText.text = senderText;
    }

    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin( Hand hand )
    {
        //generalText.text = "Hovering hand: " + hand.name;
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd( Hand hand )
    {
        //generalText.text = "No Hand Hovering";
    }


    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate( Hand hand )
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
        {
            // Save our position/rotation so that we can restore it when we detach
            if (ghostObj == null)
            {
                oldPosition = transform.localPosition;
                oldRotation = transform.localRotation;
            }
            else
            {
                oldPosition = transform.position;
                oldRotation = transform.rotation;
            }

            // Call this to continue receiving HandHoverUpdate messages,
            // and prevent the hand from hovering over anything else
            hand.HoverLock(interactable);

            // Attach this object to the hand
            hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
        }
        else if (isGrabEnding)
        {
            // Detach this object from the hand
            hand.DetachObject(gameObject);

            // Call this to undo HoverLock
            hand.HoverUnlock(interactable);

            // Restore position/rotation
            if (ghostObj == null)
            {
                transform.localPosition = oldPosition;
                transform.localRotation = oldRotation;
            }
            else
            {
                transform.position = oldPosition;
                transform.rotation = oldRotation;
            }
        }
    }


    //-------------------------------------------------
    // Called when this GameObject becomes attached to the hand
    //-------------------------------------------------
    private void OnAttachedToHand( Hand hand )
    {
        //if (ghostObj == null) Debug.Log("attached");
        generalText.text = string.Format("<size=70%>{0}", messageText);
        sideTopText.text = subjectText;
        sideBotText.text = senderText;
        bottomText.text = "";

        isGrabbed = true;
        if (ghostObj != null) ghostObj.SetActive(true);
        else 
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponentInChildren<BoxCollider>().isTrigger = false;
        }
    }


    //-------------------------------------------------
    // Called when this GameObject is detached from the hand
    //-------------------------------------------------
    private void OnDetachedFromHand( Hand hand )
    {
        if (destinationScript != null)
        {
            parentScript.AddMessageToArray(destinationScript.isAllow, messageData);
            parentScript.DestroyMessage();
            Destroy(gameObject);
        }
        else
        {
            sideTopText.text = "";
            sideBotText.text = "";
            bottomText.text = senderText;
            isGrabbed = false;
            if (ghostObj != null) 
            {
                generalText.text = subjectText;
                ghostObj.SetActive(false);
            }
            else 
            {
                generalText.text = string.Format("<size=70%>{0}", subjectText);
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponentInChildren<BoxCollider>().isTrigger = true;
            }
        }
    }


    //-------------------------------------------------
    // Called every Update() while this GameObject is attached to the hand
    //-------------------------------------------------
    private void HandAttachedUpdate( Hand hand )
    {
        //generalText.text = string.Format("Attached: {0} :: Time: {1:F2}", hand.name, (Time.time - attachTime));
    }

    private bool lastHovering = false;
    private void Update()
    {
        if (interactable.isHovering != lastHovering) //save on the .tostrings a bit
        {
            lastHovering = interactable.isHovering;
        }
    }


    //-------------------------------------------------
    // Called when this attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusAcquired( Hand hand )
    {
        Debug.Log("focused");
    }


    //-------------------------------------------------
    // Called when another attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusLost( Hand hand )
    {
        Debug.Log("unfocused");
    }

    void OnTriggerEnter(Collider other)
    {
        if (ghostObj == null) return;
        if (other.tag == "destination" && isGrabbed) destinationScript = other.GetComponentInParent<MessageDestination>();
        else if (other.tag == "warp") 
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (ghostObj == null) return;
        if (other.tag == "destination") destinationScript = null;
    }
}
