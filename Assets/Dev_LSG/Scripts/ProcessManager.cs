using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Management;
using Debug = UnityEngine.Debug;

public class ProcessManager : MonoBehaviour
{
    private string tetrapodPath, gangformPath, tunnelPath;
    [SerializeField] private bool isRun = false;
    //private Process[] _pName;
    private string _runPath;
    private string _fileName;

    private void Awake()
    {
        tetrapodPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Tetrapod.txt"));
        gangformPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Gangform.txt"));
        tunnelPath = ReadTxt(Path.Combine(Application.streamingAssetsPath, "Tunnel.txt"));
        print(tunnelPath);
    }

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
        _runPath = path;
        _fileName = Path.GetFileNameWithoutExtension(_runPath);
        print(path);
        isRun = true;
        //Invoke(nameof(DelayCheck), 3f);
        StopXR();
        Process.Start(path);
    }

    public void DelayReconnect()
    {
        Reconnect();
    }
    private void Update()
    {
        if (isRun == true)
        {
            // Process.GetProcess(): 실행중인 프로세스 배열 반환
            foreach(Process process in Process.GetProcesses())
            {
                // _fileName 라는 이름을 가진 프로세스가 존재하면 true를 리턴한다.
                if (!process.HasExited)
                {
                    if(process.ProcessName.StartsWith(_fileName))
                    {
                        print(_fileName + " is running!");
                    }
                }
                else
                {
                    print(_fileName + " exited!");
                    isRun = false;
                    Invoke(nameof(DelayReconnect), 1f);
                }
            }
            // else
            // {
            //     Reconnect();
            //     isRun = false;
            // }
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
