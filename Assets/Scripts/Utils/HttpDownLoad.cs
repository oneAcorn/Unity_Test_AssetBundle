using UnityEngine;
using System.Collections;
using System.Threading;
using System.IO;
using System.Net;
using System;

/// <summary>
/// ͨ��http������Դ
/// </summary>
public class HttpDownLoad
{
	//���ؽ���
	public float progress { get; private set; }
	//�漰���߳�Ҫע��,Unity�رյ�ʱ�����̲߳���رգ�����Ҫ��һ����ʶ
	private bool isStop;
	//���̸߳������أ�������������̣߳�Unity����Ῠ��
	private Thread thread;
	//��ʾ�����Ƿ����
	public bool isDone { get; private set; }
	const int ReadWriteTimeOut = 2 * 1000;//��ʱ�ȴ�ʱ��
	const int TimeOutWait = 5 * 1000;//��ʱ�ȴ�ʱ��


	/// <summary>
	/// ���ط���(�ϵ�����)
	/// </summary>
	/// <param name="url">URL���ص�ַ</param>
	/// <param name="savePath">Save path����·��</param>
	/// <param name="callBack">Call back�ص�����</param>
	public void DownLoad(string url, string savePath, string fileName, Action callBack, System.Threading.ThreadPriority threadPriority = System.Threading.ThreadPriority.Normal)
	{
		isStop = false;
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		//�������߳�����,ʹ����������
		thread = new Thread(delegate () {
			stopWatch.Start();
			//�жϱ���·���Ƿ����
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}
			//����Ҫ���ص��ļ���������ӷ���������a.zip��D�̣�������ļ�����test
			string filePath = savePath + "/" + fileName;

			//ʹ���������ļ�
			FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
			//��ȡ�ļ����ڵĳ���
			long fileLength = fs.Length;
			//��ȡ�����ļ����ܳ���
			UnityEngine.Debug.Log(url + " " + fileName);
			long totalLength = GetLength(url);
			Debug.LogFormat("<color=red>�ļ�:{0} ������{1}M��ʣ��{2}M</color>", fileName, fileLength / 1024 / 1024, (totalLength - fileLength) / 1024 / 1024);

			//���û������
			if (fileLength < totalLength)
			{

				//�ϵ��������ģ����ñ����ļ�������ʼλ��
				fs.Seek(fileLength, SeekOrigin.Begin);

				HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

				request.ReadWriteTimeout = ReadWriteTimeOut;
				request.Timeout = TimeOutWait;

				//�ϵ��������ģ�����Զ�̷����ļ�������ʼλ��
				request.AddRange((int)fileLength);

				Stream stream = request.GetResponse().GetResponseStream();
				byte[] buffer = new byte[1024];
				//ʹ������ȡ���ݵ�buffer��
				//ע�ⷽ������ֵ�����ȡ��ʵ�ʳ���,������buffer�ж��stream�ͻ����ȥ����
				int length = stream.Read(buffer, 0, buffer.Length);
				//Debug.LogFormat("<color=red>length:{0}</color>" + length);
				while (length > 0)
				{
					//���Unity�ͻ��˹رգ�ֹͣ����
					if (isStop) break;
					//��������д�뱾���ļ���
					fs.Write(buffer, 0, length);
					//�������
					fileLength += length;
					progress = (float)fileLength / (float)totalLength;
					//UnityEngine.Debug.Log(progress);
					//����β�ݹ�
					length = stream.Read(buffer, 0, buffer.Length);

				}
				stream.Close();
				stream.Dispose();

			}
			else
			{
				progress = 1;
			}
			stopWatch.Stop();
			Debug.Log("��ʱ: " + stopWatch.ElapsedMilliseconds);
			fs.Close();
			fs.Dispose();
			//���������ϣ�ִ�лص�
			if (progress == 1)
			{
				isDone = true;
				if (callBack != null) callBack();
				thread.Abort();
			}
			UnityEngine.Debug.Log("download finished");
		});
		//�������߳�
		thread.IsBackground = true;
		thread.Priority = threadPriority;
		thread.Start();
	}


	/// <summary>
	/// ��ȡ�����ļ��Ĵ�С
	/// </summary>
	/// <returns>The length.</returns>
	/// <param name="url">URL.</param>
	long GetLength(string url)
	{
		UnityEngine.Debug.Log(url);

		HttpWebRequest requet = HttpWebRequest.Create(url) as HttpWebRequest;
		requet.Method = "HEAD";
		HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
		return response.ContentLength;
	}

	public void Close()
	{
		isStop = true;
	}

}

