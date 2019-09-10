using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    int iMaterialCur = 0;
    List<Material> materials = new List<Material>();

    void Start()
    {
        if (materials.Count() == 0)
        {
            IEnumerable<string> paths = AssetDatabase.FindAssets(null, new[] { "Assets/Materials/Skyboxes" }).Select(guid => AssetDatabase.GUIDToAssetPath(guid));

            foreach (string path in paths)
            {
                Material material = (Material)AssetDatabase.LoadMainAssetAtPath(path); // new Material(RenderSettings.skybox);
                materials.Add(material);
            }

            RenderSettings.skybox = materials[iMaterialCur];
            DynamicGI.UpdateEnvironment();
        }

    }

    public void NextVista()
    {
        iMaterialCur = (iMaterialCur + 1) % materials.Count();

        if (RenderSettings.skybox.name != materials[iMaterialCur].name)
        {
            RenderSettings.skybox = materials[iMaterialCur];
            DynamicGI.UpdateEnvironment();
        }
    }

    public void PrevVista()
    {
        iMaterialCur = ((iMaterialCur - 1) >= 0) ? (iMaterialCur - 1) : (materials.Count() - 1);

        if (RenderSettings.skybox.name != materials[iMaterialCur].name)
        {
            RenderSettings.skybox = materials[iMaterialCur];
            DynamicGI.UpdateEnvironment();
        }
    }
}
