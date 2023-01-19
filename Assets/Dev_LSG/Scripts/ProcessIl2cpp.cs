using System;
using System.Collections;
using KS.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Management;
using KS.UnityToolbag;

public class ProcessIl2cpp : MonoBehaviour
{
    private string tetrapodPath, gangformPath, tunnelPath;
    private string _runPath;
    private string _fileName;
    
    private void Awake()
    {
        tetrapodPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Tetrapod.txt"));
        gangformPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Gangform.txt"));
        tunnelPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Tunnel.txt"));
    }
    
    [ContextMenu("Tetrapod")]
    public void Tetrapod_exe()
    {
        StartProcess(tetrapodPath);
    }
    
    public void Gangform_exe()
    {
        StartProcess(gangformPath);
    }
    
    public void Tunnel_exe()
    {
        StartProcess(tunnelPath);
    }
    
    public void StartProcess(string path)
    {
        print(path);
        StopXR();
        //Application.OpenURL(path);
        _runPath = path;
        _fileName = Path.GetFileNameWithoutExtension(_runPath);
        StartCoroutine(StartProcess());
    }
    
    IEnumerator StartProcess()
    {
        var unityProcesses = Process.GetProcessesByName(_fileName);
        
        //text2.text += "Random Unity process id: " + unityProcesses[0].Id + Environment.NewLine;
        //text2.text += "Has StartTime: " + unityProcesses[0].StartTime + Environment.NewLine;
        
        foreach(var process in unityProcesses)
            process.Dispose();
        
        yield return new WaitForSeconds(0.1f);

        var processes = Process.GetProcesses();
        // var processes = System.Diagnostics.Process.GetProcesses();

        foreach(var process in processes)
        {
            try
            {
                //sb.AppendLine(process.ProcessName + ": " + process.Id);
            }
            catch
            {
                //skip finished/unavailable processes
            }
        }

        foreach(var process in processes)
            process.Dispose();

        yield return new WaitForSeconds(0.1f);

        // this example works only in editor, for building application .exe could be placed for example in StreamingAssets folder https://docs.unity3d.com/Manual/StreamingAssets.html

        // var mainFolder = Application.streamingAssetsPath;
        // var exePath = Path.Combine(mainFolder,"NativeLibraryConsoleTest.ex");

        var proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = _runPath,
                Arguments = "",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_runPath),
            },

            //enables rising Exited event
            EnableRaisingEvents = true
        };
        //proc.OutputDataReceived += (s, d) =>
            // Dispatcher is used to run on unity main thread
            //Dispatcher.Invoke(() => print("Loading..."));

        proc.Exited += (s, d) => Dispatcher.Invoke(() => StartCoroutine(OnExit()));
        
        // make sure path is correct otherwise app may crash
        if(!File.Exists(_runPath))
            throw new Exception("File missing: " + _runPath);
        
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
            print(Environment.NewLine + "Exited");
            print(Environment.NewLine + "ExitCode: " + proc.ExitCode);
            print(Environment.NewLine + "ExitTime: " + proc.ExitTime);
            proc.CancelOutputRead();
            proc.Dispose();
            proc = null;
            yield return new WaitForSeconds(0.1f);
            Invoke(nameof(Reconnect), 1f);
        }
    }
    
    [ContextMenu("Reconnect")]
    public void Reconnect()
    {
        StartCoroutine(StartXRCoroutine());
    }

    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
    
    string ReadTxt(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        string value = "";

        if (fileInfo.Exists)
        {
            StreamReader reader = new StreamReader(filePath);
            value = reader.ReadToEnd();
            reader.Close();           
        }

        else
            value = "파일이 없습니다.";

        return value;
    }
}
