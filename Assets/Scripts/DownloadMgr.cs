using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadMgr
{
    private BlockingQueue2<string> blockingQueue = new BlockingQueue2<string>(5);

    public void AddTask(string url)
    {
        blockingQueue.Open();
        blockingQueue.Enqueue(url);
    }

    private void ExecuteTask()
    {
        blockingQueue.Close();

    }
}
