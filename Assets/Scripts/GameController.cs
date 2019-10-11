using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public float delayForShowingMessage = 10f; // delay in seconds before we will consider showing the message
    public int numDragsForShowingMessage = 3;
    public int rotationForShowingMessage = 40; // amount of rotation before we will consider showing the message

    GameObject effectContainer; // source of the effect (like bubbles or fireworks)
    Quaternion initialCameraRotation;
    Vector3 initialPosition, finalPosition;

    enum GameState { start, fadeInInitMessage, fadeOutInitMessage, lookingForSource, lookedForSource, fadeInMessage, fadeOutMessage, emitterMoving, completed, fadeInEndMessage };
    static GameState gameState = GameState.start;
    private float startedLookingAt;
    const float emitterAnimTimeTotal = 2; // total length of source animation in seconds
    float emitterAnimTimeCur = 0;

    GameObject messagePnl, dismissMessageBtn, dismissMessageTxt, brain;
    TMP_Text messageTMPText;
    RawImage messageIconImage;
    CanvasGroup messagePnlCG;
    float messageFadeTimeTotal = 3; // total time for fading in / out the message panel
    float messageFadeTimeCur = 0; // message panel fade in/out current time

    float curRotationMax = 0;
    static int cDragsTotal = 0;

    void Awake()
    {
        initialCameraRotation = new Quaternion(Camera.main.transform.rotation.x, Camera.main.transform.rotation.y, Camera.main.transform.rotation.z, Camera.main.transform.rotation.w);

        effectContainer = GameObject.Find("EffectContainer");
        initialPosition = effectContainer.transform.localPosition;
        finalPosition = new Vector3(0, -1, initialPosition.z);

        messagePnl = GameObject.Find("MessagePnl");
        messagePnlCG = messagePnl.GetComponent<CanvasGroup>();
        messageIconImage = GameObject.Find("MessageIcon").GetComponent<RawImage>();
        messageTMPText = GameObject.Find("MessageTxt").GetComponent<TMP_Text>();
        dismissMessageBtn = GameObject.Find("DismissMessageBtn");
        brain = GameObject.Find("Brain");

        dismissMessageBtn.SetActive(false);
    }

    void LateUpdate()
    {
        switch (gameState)
        {
            case GameState.start:
                messageIconImage.texture = (Texture2D)Resources.Load("AppIcon", typeof(Texture2D));
                messageTMPText.GetComponent<TMP_Text>().text = "Crisp fall evening,\r\nBubbles blowing in the wind.\r\nChildhood memories.\r\n\r\n(Want to know where they are coming from?\r\nYou can click or tap and drag to look around.)"; // TODO: check input device to show approp message
                gameState = GameState.fadeInInitMessage;
                break;

            case GameState.fadeInInitMessage:
                FadeInOut(messagePnlCG, true);
                break;

            case GameState.fadeOutInitMessage:
                if (FadeInOut(messagePnlCG, false))
                {
                    gameState = GameState.lookingForSource;
                    startedLookingAt = Time.realtimeSinceStartup;
                }
                break;

            case GameState.lookingForSource:
                float curRotation = Quaternion.Angle(Camera.main.transform.rotation, initialCameraRotation);
                if (curRotation > curRotationMax)
                    curRotationMax = curRotation;

                // if total number of drags and the max rotation so far have exceeded their target Or user has already viewed the vista for a long time, move to the next state
                if (((cDragsTotal >= numDragsForShowingMessage) && (curRotationMax >= rotationForShowingMessage)) ||
                    ((Time.realtimeSinceStartup - startedLookingAt) > delayForShowingMessage * 3))
                    gameState = GameState.lookedForSource;
                break;

            case GameState.lookedForSource:
                if ((Time.realtimeSinceStartup - startedLookingAt) > delayForShowingMessage)
                {
                    messageIconImage.texture = (Texture2D)Resources.Load("SpoonBoy", typeof(Texture2D));
                    messageTMPText.GetComponent<TMP_Text>().text = "Where they come from is\r\nUnknowable. Instead,\r\nJust realize the truth.";
                    dismissMessageBtn.SetActive(true);
                    gameState = GameState.fadeInMessage;
                }
                break;

            case GameState.fadeInMessage:
                FadeInOut(messagePnlCG, true);
                break;

            case GameState.fadeOutMessage:
                if (FadeInOut(messagePnlCG, false))
                    gameState = GameState.emitterMoving;
                break;

            case GameState.emitterMoving:
                // move the emitter to make it visible
                effectContainer.transform.Translate(
                    (finalPosition.x - initialPosition.x) * Time.deltaTime / emitterAnimTimeTotal,
                    (finalPosition.y - initialPosition.y) * Time.deltaTime / emitterAnimTimeTotal,
                    (finalPosition.z - initialPosition.z) * Time.deltaTime / emitterAnimTimeTotal,
                    Camera.main.transform);

                emitterAnimTimeCur += Time.deltaTime;
                if (emitterAnimTimeCur >= emitterAnimTimeTotal)
                    gameState = GameState.fadeInEndMessage;
                break;

            case GameState.fadeInEndMessage:
                messageIconImage.texture = (Texture2D)Resources.Load("AppIcon", typeof(Texture2D));
                messageTMPText.GetComponent<TMP_Text>().text = "That they come from within...";
                dismissMessageBtn.SetActive(false);
                messageFadeTimeCur = 0;
                gameState = GameState.completed;
                break;

            case GameState.completed:
                FadeInOut(messagePnlCG, true);
                effectContainer.transform.Rotate(Vector3.up, 0.1f);
                break;
        }
    }

    // fades in or out a canvas group, returns true when completed
    private bool FadeInOut(CanvasGroup cg, bool fadingIn)
    {
        int fadeTo = fadingIn ? 1 : 0;
        if (fadingIn ? (cg.alpha < fadeTo) : (cg.alpha > fadeTo))
        {
            messageFadeTimeCur += Time.deltaTime;
            messagePnlCG.alpha = Mathf.Lerp(messagePnlCG.alpha, fadeTo, messageFadeTimeCur / messageFadeTimeTotal);
            return false;
        }

        return true;
    }

    public void HideMessage()
    {
        messageFadeTimeCur = 0;
        gameState = GameState.fadeOutMessage;
    }

    internal static void DragOver()
    {
        if (gameState == GameState.fadeInInitMessage)
            gameState = GameState.fadeOutInitMessage;

        cDragsTotal++;
    }
}
