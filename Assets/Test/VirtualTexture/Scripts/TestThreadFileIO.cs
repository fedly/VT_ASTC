using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;

public class TestThreadFileIO : MonoBehaviour
{
    public string FileDir = "D:/";
    public int FileCount = 8;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        int bytesCount = 0;
        for (int i = 0; i < FileCount; i++)
        {
            var bytes = File.ReadAllBytes(Path.Combine(FileDir, i.ToString()));
            bytesCount += bytes.Length;
        }
        stopWatch.Stop();
        UnityEngine.Debug.Log($"{stopWatch.ElapsedMilliseconds}, {bytesCount}");
        yield return new WaitForSeconds(3);
        stopWatch.Reset();
        stopWatch.Restart();
        int count = 0;
        bytesCount = 0;
        for (int i = 0; i < FileCount; i++)
        {
            _ = File.ReadAllBytesAsync(Path.Combine(FileDir, i.ToString())).ContinueWith(t =>
            {
                Interlocked.Add(ref bytesCount, t.Result.Length);
                if (Interlocked.Increment(ref count) >= FileCount)
                {
                    stopWatch.Stop();
                    UnityEngine.Debug.Log($"{stopWatch.ElapsedMilliseconds}, {bytesCount}");
                }
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
