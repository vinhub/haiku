using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct VistaData
{
    public string fileName;
}

[Serializable]
public struct VistaDatas
{
    public VistaData[] vistaDatas;
}

public class GameController : MonoBehaviour
{
    public float delayForShowingMessage = 10f; // delay in seconds before we will consider showing the message
    public int rotationForShowingMessage = 40; // amount of rotation before we will consider showing the message

    int currentVistaIndex = 0;
    VistaData[] vistas;

    GameObject emitterObject; // source of the effect (like bubbles or fireworks)
    Quaternion initialCameraRotation;
    Vector3 initialPosition, finalPosition;

    enum GameState { started, lookedForSource, showingMessage, hidingMessage, emitterMoving, completed };
    GameState gameState = GameState.started;

    const float emitterAnimTime = 2; // total length of source animation in seconds
    float emitterAnimTimeCur = 0;

    CanvasGroup messagePnlCG;
    float messagePnlFadeTimeCur = 0; // message panel fade in/out time

    int curRotationMax = 0;

    void Start()
    {
        TextAsset jsonTextFile = Resources.Load<TextAsset>("vistas");
        VistaDatas vistaDatas = JsonUtility.FromJson<VistaDatas>(jsonTextFile.text);
        vistas = vistaDatas.vistaDatas;

        initialCameraRotation = new Quaternion(Camera.main.transform.rotation.x, Camera.main.transform.rotation.y, Camera.main.transform.rotation.z, Camera.main.transform.rotation.w);

        emitterObject = GameObject.FindWithTag("Effect");
        initialPosition = emitterObject.transform.localPosition;
        finalPosition = new Vector3(0, -1, initialPosition.z);

        messagePnlCG = GameObject.FindGameObjectWithTag("Message").GetComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        switch (gameState)
        {
            case GameState.started:
                if (Quaternion.Angle(Camera.main.transform.rotation, initialCameraRotation) > curRotationMax)
                    gameState = GameState.lookedForSource;
                break;

            case GameState.lookedForSource:
                if (Time.realtimeSinceStartup > delayForShowingMessage)
                {
                    messagePnlFadeTimeCur = 0;
                    gameState = GameState.showingMessage;
                }

                break;

            case GameState.showingMessage:
                if (messagePnlCG.alpha < 1)
                {
                    messagePnlFadeTimeCur += Time.deltaTime;
                    messagePnlCG.alpha = Mathf.Lerp(messagePnlCG.alpha, 1, messagePnlFadeTimeCur);
                }
                break;

            case GameState.hidingMessage:
                if (messagePnlCG.alpha > 0)
                {
                    messagePnlFadeTimeCur += Time.deltaTime;
                    messagePnlCG.alpha = Mathf.Lerp(messagePnlCG.alpha, 0, messagePnlFadeTimeCur);
                }
                else
                    gameState = GameState.emitterMoving;
                break;

            case GameState.emitterMoving:
                // move the emitter to make it visible
                emitterObject.transform.Translate((finalPosition.x - initialPosition.x) * Time.deltaTime / emitterAnimTime,
                    (finalPosition.y - initialPosition.y) * Time.deltaTime / emitterAnimTime,
                    0, Camera.main.transform);

                emitterAnimTimeCur += Time.deltaTime;
                if (emitterAnimTimeCur >= emitterAnimTime)
                    gameState = GameState.completed;
                break;

            case GameState.completed:
                break;
        }
    }

    public void HideMessage()
    {
        GameObject uiObj = GameObject.FindGameObjectWithTag("UI");
        GameObject messagePnl = uiObj.FindObject("MessagePnl");
        messagePnl.SetActive(false);

        messagePnlFadeTimeCur = 0;
        gameState = GameState.hidingMessage;
    }

    public void NextVista()
    {
        currentVistaIndex = (currentVistaIndex + 1) % vistas.Count();
        if (RenderSettings.skybox.name != vistas[currentVistaIndex].fileName)
        {
            loadVista(currentVistaIndex);
        }
    }

    public void PrevVista()
    {
        currentVistaIndex = ((currentVistaIndex - 1) >= 0) ? (currentVistaIndex - 1) : (vistas.Count() - 1);

        if (RenderSettings.skybox.name != vistas[currentVistaIndex].fileName)
        {
            loadVista(currentVistaIndex);
        }
    }

    private void loadVista(int iVista)
    {
        StartCoroutine(loadImage(iVista));
    }

    private IEnumerator loadImage(int iVista)
    {
        Uri uri = new Uri(Application.streamingAssetsPath + "/Skyboxes/" + vistas[iVista].fileName);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // TODO: Not working
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Material matSkybox = new Material(Shader.Find("Skybox/Cubemap"));
                matSkybox.SetTexture("_Tex", texture);
                RenderSettings.skybox = matSkybox;
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}
