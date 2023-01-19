using System;
using System.Collections;
//using System.Diagnostics;
using KS.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Management;
using KS.UnityToolbag;

public class ProcessManager : MonoBehaviour
{
    private string tetrapodPath, gangformPath, tunnelPath;
    private Process tetrapodProc, gangformProc, tunnelProc;
    //private Process[] _pName;

    private void Awake()
    {
        tetrapodPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Tetrapod.txt"));
        gangformPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Gangform.txt"));
        tunnelPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Tunnel.txt"));
        
        SetProc(tetrapodProc, tetrapodPath);
        SetProc(gangformProc, gangformPath);
        SetProc(tunnelProc, tunnelPath);
    }

    void SetProc(Process proc, string path)
    {
        proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = "",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(path),
            },

            //enables rising Exited event
            EnableRaisingEvents = true
        };

        proc.OutputDataReceived += (s, d) =>
            // Dispatcher is used to run on unity main thread
            Dispatcher.Invoke(() => print(proc.StartInfo.FileName));

        proc.Exited += (s, d) => Dispatcher.Invoke(() => StartCoroutine(OnExit()));
    }
    [ContextMenu("Tetrapod")]
    public void Tetrapod_exe()
    {
        print(tetrapodProc.StartInfo.FileName);
        StartProcess(tetrapodProc);
    }
    
    public void Gangform_exe()
    {
        StartProcess(gangformProc);
    }
    
    public void Tunnel_exe()
    {
        StartProcess(tunnelProc);
    }
    
    public void StartProcess(Process process)
    {
        print(process.StartInfo.FileName);
        //Invoke(nameof(DelayCheck), 3f);
        StopXR();
        process.Start();
    }

    public void DelayReconnect()
    {
        Reconnect();
    }

    IEnumerator OnExit()
    {
        // wait a bit to make sure all messages were read
        yield return new WaitForSeconds(0.1f);
        foreach(Process process in Process.GetProcesses())
        {
            //print(" exited!");
        }
        Invoke(nameof(DelayReconnect), 1f);
        yield return new WaitForSeconds(0.1f);
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
    
    void WriteTxt(string filePath, string message)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        FileStream fileStream
            = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.WriteLine(message);
        writer.Close();
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
