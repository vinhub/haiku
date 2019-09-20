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

//[Serializable]
//public struct VistaData
//{
//    public string fileName;
//}

//[Serializable]
//public struct VistaDatas
//{
//    public VistaData[] vistaDatas;
//}

public class GameController : MonoBehaviour
{
    public float delayForShowingMessage = 10f; // delay in seconds before we will consider showing the message
    public int numDragsForShowingMessage = 3;
    public int rotationForShowingMessage = 40; // amount of rotation before we will consider showing the message

    //int currentVistaIndex = 0;
    //VistaData[] vistas;

    GameObject effectContainer; // source of the effect (like bubbles or fireworks)
    Quaternion initialCameraRotation;
    Vector3 initialPosition, finalPosition;

    enum GameState { start, fadeInInitMessage, fadeOutInitMessage, lookingForSource, lookedForSource, fadeInMessage, fadeOutMessage, emitterMoving, completed, fadeInEndMessage };
    GameState gameState = GameState.start;
    private float startedLookingAt;
    const float emitterAnimTime = 1; // total length of source animation in seconds
    float emitterAnimTimeCur = 0;

    GameObject messagePnl, messageIconImage, messageTxt, dismissMessageBtn, dismissMessageTxt, brain;
    CanvasGroup messagePnlCG;
    float messagePnlFadeTimeCur = 0; // message panel fade in/out time

    float curRotationMax = 0;
    static int cDragsTotal = 0;

    void Start()
    {
        //TextAsset jsonTextFile = Resources.Load<TextAsset>("vistas");
        //VistaDatas vistaDatas = JsonUtility.FromJson<VistaDatas>(jsonTextFile.text);
        //vistas = vistaDatas.vistaDatas;

        initialCameraRotation = new Quaternion(Camera.main.transform.rotation.x, Camera.main.transform.rotation.y, Camera.main.transform.rotation.z, Camera.main.transform.rotation.w);

        effectContainer = GameObject.Find("EffectContainer");
        initialPosition = effectContainer.transform.localPosition;
        finalPosition = new Vector3(0, -1, initialPosition.z);

        messagePnl = GameObject.Find("MessagePnl");
        messagePnlCG = messagePnl.GetComponent<CanvasGroup>();
        messageTxt = GameObject.Find("MessageTxt");
        dismissMessageBtn = GameObject.Find("DismissMessageBtn");
        dismissMessageTxt = GameObject.Find("DismissMessageTxt");
        brain = GameObject.Find("Brain");
    }

    void LateUpdate()
    {
        switch (gameState)
        {
            case GameState.start:
                messageTxt.GetComponent<TMP_Text>().text = "Look around by clicking or tapping and draging."; // TODO: check input device to show approp message
                dismissMessageTxt.GetComponent<Text>().text = "Ok";
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
                    messageTxt.GetComponent<TMP_Text>().text = "Do not try and find where the bubbles are coming from. That's impossible.\r\n\r\nInstead, only try to realize the truth.";
                    dismissMessageTxt.GetComponent<Text>().text = "What truth?";
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
                    (finalPosition.x - initialPosition.x) * Time.deltaTime / emitterAnimTime,
                    (finalPosition.y - initialPosition.y) * Time.deltaTime / emitterAnimTime,
                    (finalPosition.z - initialPosition.z) * Time.deltaTime / emitterAnimTime,
                    Camera.main.transform);

                emitterAnimTimeCur += Time.deltaTime;
                if (emitterAnimTimeCur >= emitterAnimTime)
                    gameState = GameState.fadeInEndMessage;
                break;

            case GameState.fadeInEndMessage:
                messageTxt.GetComponent<TMP_Text>().text = "That it's all in your mind...";
                dismissMessageBtn.SetActive(false);
                messagePnlFadeTimeCur = 0;
                gameState = GameState.completed;
                break;

            case GameState.completed:
                if (FadeInOut(messagePnlCG, true))
                {
                    effectContainer.transform.Rotate(Vector3.up, 0.1f);
                }
                break;
        }
    }

    // fades in or out a canvas group, returns true when completed
    private bool FadeInOut(CanvasGroup cg, bool fadingIn)
    {
        int fadeTo = fadingIn ? 1 : 0;
        if (fadingIn ? (cg.alpha < fadeTo) : (cg.alpha > fadeTo))
        {
            messagePnlFadeTimeCur += Time.deltaTime;
            messagePnlCG.alpha = Mathf.Lerp(messagePnlCG.alpha, fadeTo, messagePnlFadeTimeCur);
            return false;
        }

        return true;
    }

    public void HideMessage()
    {
        messagePnlFadeTimeCur = 0;
        gameState = (gameState == GameState.fadeInInitMessage) ? GameState.fadeOutInitMessage : GameState.fadeOutMessage;
    }

    internal static void DragOver()
    {
        cDragsTotal++;
    }

    //public void NextVista()
    //{
    //    currentVistaIndex = (currentVistaIndex + 1) % vistas.Count();
    //    if (RenderSettings.skybox.name != vistas[currentVistaIndex].fileName)
    //    {
    //        loadVista(currentVistaIndex);
    //    }
    //}

    //public void PrevVista()
    //{
    //    currentVistaIndex = ((currentVistaIndex - 1) >= 0) ? (currentVistaIndex - 1) : (vistas.Count() - 1);

    //    if (RenderSettings.skybox.name != vistas[currentVistaIndex].fileName)
    //    {
    //        loadVista(currentVistaIndex);
    //    }
    //}

    //private void loadVista(int iVista)
    //{
    //    StartCoroutine(loadImage(iVista));
    //}

    //private IEnumerator loadImage(int iVista)
    //{
    //    Uri uri = new Uri(Application.streamingAssetsPath + "/Skyboxes/" + vistas[iVista].fileName);

    //    using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
    //    {
    //        yield return uwr.SendWebRequest();

    //        if (uwr.isNetworkError || uwr.isHttpError)
    //        {
    //            Debug.Log(uwr.error);
    //        }
    //        else
    //        {
    //            // TODO: Not working
    //            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
    //            Material matSkybox = new Material(Shader.Find("Skybox/Cubemap"));
    //            matSkybox.SetTexture("_Tex", texture);
    //            RenderSettings.skybox = matSkybox;
    //            DynamicGI.UpdateEnvironment();
    //        }
    //    }
    //}
}
