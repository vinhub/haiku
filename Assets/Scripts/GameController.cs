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
    public float sourceVisibleDelay = 10f;

    int currentVistaIndex = 0;
    VistaData[] vistas;
    GameObject sourceObject; // source of the magic effect (like bubbles or fireworks)
    bool isSourceVisible = false;

    void Start()
    {
        TextAsset jsonTextFile = Resources.Load<TextAsset>("vistas");
        VistaDatas vistaDatas = JsonUtility.FromJson<VistaDatas>(jsonTextFile.text);
        vistas = vistaDatas.vistaDatas;

        sourceObject = GameObject.FindWithTag("Effect");
    }

    void LateUpdate()
    {
        // if specified number of seconds are up, move the source object to center of screen
        if (!isSourceVisible && (Time.realtimeSinceStartup > sourceVisibleDelay))
        {
            Transform transform = sourceObject.transform;
            Vector3 localPosition = transform.localPosition;
            transform.Translate(-localPosition.x, -localPosition.y, localPosition.z, Camera.main.transform);
            isSourceVisible = true;
        }
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
