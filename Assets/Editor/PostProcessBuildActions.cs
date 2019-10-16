using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

public class PostBuildActions
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string targetPath)
    {
        // remove mobile warning
        string path = Path.Combine(targetPath, "Build/UnityLoader.js");
        string text = File.ReadAllText(path);
        text = text.Replace("UnityLoader.SystemInfo.mobile", "false");
        File.WriteAllText(path, text);

        // copy web.config
        string srcPath = Path.GetFullPath(Path.Combine(targetPath, "../Deploy/web.config"));
        string dstPath = Path.Combine(targetPath, "web.config");
        File.Copy(srcPath, dstPath, true);
    }
}
