using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using KS.UnityToolbag;
using UnityEngine.UI;

using KS.Diagnostics;

// or
// using System.Diagnostics;
// using Process = KS.Diagnostics.Process;
// using ProcessStartInfo = KS.Diagnostics.ProcessStartInfo;
// using Debug = UnityEngine.Debug;

public class ProcessTest : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] Text text2;

    IEnumerator Start()
    {
        var unityProcesses = Process.GetProcessesByName("Unity");
        
        text2.text += "Random Unity process id: " + unityProcesses[0].Id + Environment.NewLine;
        text2.text += "Has StartTime: " + unityProcesses[0].StartTime + Environment.NewLine;
        
        foreach(var process in unityProcesses)
            process.Dispose();
        
        yield return new WaitForSeconds(0.5f);

        var processes = Process.GetProcesses();
        // var processes = System.Diagnostics.Process.GetProcesses();

        StringBuilder sb = new StringBuilder();

        foreach(var process in processes)
        {
            try
            {
                sb.AppendLine(process.ProcessName + ": " + process.Id);
            }
            catch
            {
                //skip finished/unavailable processes
            }
        }

        foreach(var process in processes)
            process.Dispose();

        text2.text += sb.ToString();

        yield return new WaitForSeconds(1);

        var mainFolder = Application.dataPath;
        var exePath = Path.Combine(mainFolder, "Kamyker",
            "Process-for-IL2CPP", "Example", "NativeLibraryConsoleTest.ex");

        // this example works only in editor, for building application .exe could be placed for example in StreamingAssets folder https://docs.unity3d.com/Manual/StreamingAssets.html

        // var mainFolder = Application.streamingAssetsPath;
        // var exePath = Path.Combine(mainFolder,"NativeLibraryConsoleTest.ex");

        var proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = "",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exePath),
            },

            //enables rising Exited event
            EnableRaisingEvents = true
        };
        proc.OutputDataReceived += (s, d) =>
            // Dispatcher is used to run on unity main thread
            Dispatcher.Invoke(() => text.text += Environment.NewLine + d.Data);

        proc.Exited += (s, d) => Dispatcher.Invoke(() => StartCoroutine(OnExit()));
        
        // make sure path is correct otherwise app may crash
        if(!File.Exists(exePath))
            throw new Exception("File missing: " + exePath);
        
        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            proc.Start();
            proc.BeginOutputReadLine();
            
            // test kill
            // proc.Kill();
        }
        else
        {
            UnityEngine.Debug.LogError("Sample contains exe process only for Windows.");
        }

        // yield return new WaitForSeconds(3f);
        // proc.Dispose();
        // text.text += Environment.NewLine + "Exited";

        IEnumerator OnExit()
        {
            // wait a bit to make sure all messages were read
            yield return new WaitForSeconds(0.1f);
            text.text += Environment.NewLine + "Exited";
            text.text += Environment.NewLine + "ExitCode: " + proc.ExitCode;
            text.text += Environment.NewLine + "ExitTime: " + proc.ExitTime;
            proc.CancelOutputRead();
            proc.Dispose();
            proc = null;
            yield return new WaitForSeconds(0.1f);
        }
    }
}