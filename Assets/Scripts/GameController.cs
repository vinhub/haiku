﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
    public float showMessageDelay = 10f; // start emitter animation after this delay, seconds

    int currentVistaIndex = 0;
    VistaData[] vistas;

    GameObject emitterObject; // source of the effect (like bubbles or fireworks)
    Vector3 initialPosition, finalPosition;
    const float emitterAnimTimeTotal = 3; // total length of source animation in seconds
    float emitterAnimTimeCur = 0;
    enum GameState { started, showingMessage, emitterMoving, completed };
    GameState gameState = GameState.started;

    void Start()
    {
        TextAsset jsonTextFile = Resources.Load<TextAsset>("vistas");
        VistaDatas vistaDatas = JsonUtility.FromJson<VistaDatas>(jsonTextFile.text);
        vistas = vistaDatas.vistaDatas;

        emitterObject = GameObject.FindWithTag("Effect");
        initialPosition = emitterObject.transform.localPosition;
        finalPosition = new Vector3(0, -1, initialPosition.z);
    }

    void LateUpdate()
    {
        switch (gameState)
        {
            case GameState.started:
                if (Time.realtimeSinceStartup >= showMessageDelay)
                {
                    GameObject uiObj = GameObject.FindGameObjectWithTag("UI");
                    GameObject messagePnl = uiObj.FindObject("MessagePnl");
                    messagePnl.SetActive(true);
                    gameState = GameState.showingMessage;
                }
                break;

            case GameState.showingMessage:
                break;

            case GameState.emitterMoving:
                // move the emitter to make it visible
                emitterObject.transform.Translate((finalPosition.x - initialPosition.x) * Time.deltaTime / emitterAnimTimeTotal,
                    (finalPosition.y - initialPosition.y) * Time.deltaTime / emitterAnimTimeTotal,
                    0, Camera.main.transform);

                emitterAnimTimeCur += Time.deltaTime;
                if (emitterAnimTimeCur >= emitterAnimTimeTotal)
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

        gameState = GameState.emitterMoving;
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
