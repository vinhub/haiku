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
    int iVistaCur = 0;
    VistaData[] vistas;

    void Start()
    {
        TextAsset jsonTextFile = Resources.Load<TextAsset>("vistas");
        VistaDatas vistaDatas = JsonUtility.FromJson<VistaDatas>(jsonTextFile.text);
        vistas = vistaDatas.vistaDatas;
    }

    public void NextVista()
    {
        iVistaCur = (iVistaCur + 1) % vistas.Count();
        if (RenderSettings.skybox.name != vistas[iVistaCur].fileName)
        {
            loadVista(iVistaCur);
        }
    }

    public void PrevVista()
    {
        iVistaCur = ((iVistaCur - 1) >= 0) ? (iVistaCur - 1) : (vistas.Count() - 1);

        if (RenderSettings.skybox.name != vistas[iVistaCur].fileName)
        {
            loadVista(iVistaCur);
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
