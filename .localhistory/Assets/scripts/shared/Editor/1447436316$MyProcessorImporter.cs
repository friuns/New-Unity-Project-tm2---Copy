using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using gui = UnityEngine.GUILayout;

public class MyProcessorImporterWindow : EditorWindow
{


    [MenuItem("Window/History", false, 0)]
    static void rtoolsclick()
    {
        GetWindow<MyProcessorImporterWindow>();
    }
    protected virtual void OnGUI()
    {
        var g = Selection.activeObject;
        if (g)
        {
            var p = AssetDatabase.GetAssetPath(g);
            if(string.IsNullOrEmpty(p))return;
            string searchPattern = Path.GetFileName(p + "*");
            List<FileInfo> infos = new List<FileInfo>();
            FileInfo fileInfo = new FileInfo(p);
            infos.Add(fileInfo);
            var path = "History/" + Path.GetDirectoryName(p);
            if (!Directory.Exists(path)) return;
            foreach (var a in Directory.GetFiles(path, searchPattern))
                infos.Add(new FileInfo(a));

            infos = infos.OrderBy(a => a.LastWriteTime).ToList();
            foreach (var a in infos)
            {
                GUI.enabled = a != fileInfo;
                gui.BeginHorizontal();
                if (gui.Button(File.GetLastWriteTime(a.FullName).ToString()))
                {
                    File.Delete(p + " - copy");
                    MyProcessorImporter.FileMove(p, p + " - copy");
                    MyProcessorImporter.FileMove(a.FullName, p);
                    MyProcessorImporter.FileMove(p + " - copy", MyProcessorImporter.PostFix(p));
                    Repaint();
                }
                if (gui.Button("Compare"))
                    Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe", string.Format("/diff \"{0}\" \"{1}\"", fileInfo.FullName, a.FullName));
                gui.EndHorizontal();
            }
            AssetDatabase.Refresh();
            GUI.enabled = true;
        }
    }
    public void OnSelectionChange()
    {
        Repaint();
    }
    
}

public class PrfabBackup { }
public class MyProcessorImporter : UnityEditor.AssetModificationProcessor
{
    static void OnWillSaveAssets(string[] importedAssets)
    {
        foreach (var str in importedAssets)
        {
            var lower = Path.GetExtension(str).ToLower();
            if (lower == ".prefab" || lower == ".unity")
            {
                string destFileName = PostFix(str);
                if (!File.Exists(destFileName))
                {
                    Directory.CreateDirectory("History/" + Path.GetDirectoryName(str));
                    FileMove(str, destFileName);                    
                }
            }
        }
    }
    public static void FileMove(string Str, string DestFileName)
    {
        var d = File.GetLastWriteTime(Str);
        File.Move(Str, DestFileName);
        File.SetLastWriteTime(DestFileName, d);
    }
    public  static string PostFix(string str)
    {
        return "History/" + str + "¤" + (new FileInfo(str).LastWriteTime.ToFileTime() + UnityEngine.Random.Range(0, 9999));
    }
}