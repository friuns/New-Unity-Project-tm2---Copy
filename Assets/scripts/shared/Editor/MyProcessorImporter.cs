#define pro
// if you want compare feature, uncomment this line
//#define compare
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using gui = UnityEngine.GUILayout;

public class LocalHistorySettings : MonoBehaviour
{
    public readonly static string[] FileExtensionsStatic = new string[] { ".unity", ".guiskin", ".prefab", ".fbx", ".controller", ".anim", ".jpg", ".shader", ".asset", ".ttf" };
    public string[] FileExtensions = FileExtensionsStatic;
}

public class LocalHistory : EditorWindow
{

    [MenuItem("Window/Local History")]
    static void rtoolsclick()
    {
        GetWindow<LocalHistory>();
    }
    private Vector2 scroll;
    protected virtual void OnGUI()
    {
        //FindObjectOfType<LocalHistorySettings>();
        scroll = gui.BeginScrollView(scroll);
        gui.Label(Draw());
        gui.EndScrollView();
    }
    private string Draw()
    {
        var p = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        var fileName = Path.GetFileName(p);
        if (string.IsNullOrEmpty(p)) return "Select file in project";
        gui.Label(fileName);
        if (!LocalHistoryImporter.formats.Contains(Path.GetExtension(fileName).ToLower())) return "Supported formats:" + string.Join(" ", LocalHistoryImporter.formats);
        string searchPattern = fileName + "*";
        List<FileInfo> infos = new List<FileInfo>();
        FileInfo fileInfo = new FileInfo(p);
        infos.Add(fileInfo);
        var path = "History/" + Path.GetDirectoryName(p);
        if (!Directory.Exists(path)) return "No File History found";

        foreach (var a in Directory.GetFiles(path, searchPattern))
            infos.Add(new FileInfo(a));

        infos = infos.OrderByDescending(a => a.LastWriteTime).ToList();
        foreach (FileInfo a in infos)
        {
            GUI.enabled = a != fileInfo;
#if compare
            gui.BeginHorizontal();
#endif
            if (gui.Button(File.GetLastWriteTime(a.FullName).ToString()))
            {
                File.Delete(p + " - copy");
                LocalHistoryImporter.FileMove(p, p + " - copy");
                LocalHistoryImporter.FileMove(a.FullName, p);
                LocalHistoryImporter.FileMove(p + " - copy", LocalHistoryImporter.PostFix(p));
                AssetDatabase.Refresh();
                Repaint();
            }
#if compare
            if (gui.Button("Compare")) 
                Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe", string.Format("/diff \"{0}\" \"{1}\"", fileInfo.FullName, a.FullName));
            gui.EndHorizontal();
#endif
        }
        GUI.enabled = true;
        return "";
    }
    public void OnSelectionChange()
    {
        Repaint();
    }

}

public class LocalHistoryImporter : UnityEditor.AssetModificationProcessor
{
    public static LocalHistorySettings settingsFile;
    public static string[] formats
    {
        get
        {
            if (!settingsFile) settingsFile = Resources.Load<LocalHistorySettings>("LocalHistorySettings");
            if (!settingsFile)
                return LocalHistorySettings.FileExtensionsStatic;
            return settingsFile.FileExtensions;
        }
    }
    public static AssetDeleteResult OnWillDeleteAsset(string str, RemoveAssetOptions opts)
    {
        if (opts == RemoveAssetOptions.MoveAssetToTrash)
        {
            Directory.CreateDirectory("History/" + Path.GetDirectoryName(str));
            var destFileName = "History/" + str;
            if (Directory.Exists(str))
                MoveDirectory(str, destFileName);
            else
                File.Copy(str, destFileName);
            if (File.Exists(str + ".meta"))
                File.Copy(str + ".meta", destFileName + ".meta");
        }
        return AssetDeleteResult.DidNotDelete;
    }


    static string[] OnWillSaveAssets(string[] importedAssets)
    {
        foreach (string str in importedAssets)
        {

            var lower = Path.GetExtension(str).ToLower();
            if (formats.Any(a => a == lower))
            {
                string destFileName = PostFix(str);
                if (!File.Exists(destFileName))
                {
                    Directory.CreateDirectory("History/" + Path.GetDirectoryName(str));
                    FileMove(str, destFileName, true);
                    //File.WriteAllText(str, "");
                }
            }
        }
        return importedAssets;
    }
    public static void FileMove(string Str, string DestFileName, bool copy = false)
    {
        var d = File.GetLastWriteTime(Str);
        if (copy)
            File.Copy(Str, DestFileName);
        else
            File.Move(Str, DestFileName);

        File.SetLastWriteTime(DestFileName, d);
    }
    public static string PostFix(string str)
    {
        return "History/" + str + "¤" + (new FileInfo(str).LastWriteTime.ToFileTime() + UnityEngine.Random.Range(0, 9999));
    }



    public static void MoveDirectory(string source, string target)
    {
        var stack = new Stack<Folders>();
        stack.Push(new Folders(source, target));

        while (stack.Count > 0)
        {
            var folders = stack.Pop();
            Directory.CreateDirectory(folders.Target);
            foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
            {
                string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                if (File.Exists(targetFile)) File.Delete(targetFile);
                File.Copy(file, targetFile);
            }

            foreach (var folder in Directory.GetDirectories(folders.Source))
            {
                stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
            }
        }
        //Directory.Delete(source, true);
    }
    public class Folders
    {
        public string Source { get; private set; }
        public string Target { get; private set; }

        public Folders(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }
}