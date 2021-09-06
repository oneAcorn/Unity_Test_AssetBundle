using System;
using System.Collections.Generic;
using System.Threading;
/// <summary>
/// Description: �������У����ͣ�
///              ��Ҫʵ���˶���Ϊ��ʱ������������������ʱ�������
/// Author: BruceZhang
/// Date:2017-4-18
/// </summary>
public class BlockingQueue2<T>
{
    #region Fields & Properties
    //��������
    private string m_name;
    //������󳤶�
    private readonly int m_maxSize;
    //FIFO����
    private Queue<T> m_queue;
    //�Ƿ�������
    private bool m_isRunning;
    //����ֶ���λ�¼�
    private ManualResetEvent m_enqueueWait;
    //�����ֶ���λ�¼�
    private ManualResetEvent m_dequeueWait;
    //�����־
    public Action<string> m_actionOutLog;
    /// <summary>
    /// ���г���
    /// </summary>
    public int Count => m_queue.Count;
    #endregion

    #region Ctor
    public BlockingQueue2(int maxSize, string name = "BlockingQueue", bool isRunning = false)
    {
        m_maxSize = maxSize;
        m_name = name;
        m_queue = new Queue<T>(m_maxSize);
        m_isRunning = isRunning;
        m_enqueueWait = new ManualResetEvent(false); // ���źţ����waitOne����
        m_dequeueWait = new ManualResetEvent(false); // ���ź�, ����waitOne����
    }
    #endregion

    #region Private Method

    private void OutLog(string message)
    {
        m_actionOutLog?.Invoke(message);
    }

    #endregion

    #region Public Method

    /// <summary>
    /// ������������
    /// </summary>
    public void Open()
    {
        m_isRunning = true;
    }

    /// <summary>
    /// �ر���������
    /// </summary>
    public void Close()
    {
        // ֹͣ����
        m_isRunning = false;
        // �����źţ�֪ͨ��������waitOne�ɼ���ִ�У��ɽ��г��Ӳ���
        m_dequeueWait.Set();
    }

    /// <summary>
    /// ���
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item)
    {
        if (!m_isRunning)
        {
            // ����ʽһ ��ֱ���׳��쳣
            //throw new InvalidCastException("������ֹ�����������");
            // ����ʽ�� ���������־���������
            OutLog($"{m_name} ������ֹ�����������");
            return;
        }

        while (true)
        {
            lock (m_queue)
            {
                // �������δ�����������
                if (m_queue.Count < m_maxSize)
                {
                    m_queue.Enqueue(item);
                    // ��Ϊ���ź�
                    m_enqueueWait.Reset();
                    // �����źţ�֪ͨ��������waitOne�ɼ���ִ�У��ɽ��г��Ӳ���
                    m_dequeueWait.Set();
                    // �����־
                    OutLog($"{m_name} ��ӳɹ�.");
                    break;
                }
            }
            // ����������������������У�ֹͣ��ӣ��ȴ��ź�
            m_enqueueWait.WaitOne();
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Dequeue(ref T item)
    {
        while (true)
        {
            if (!m_isRunning)
            {
                lock (m_queue) return false;
            }
            lock (m_queue)
            {
                // ������������ݣ���ִ�г���
                if (m_queue.Count > 0)
                {
                    item = m_queue.Dequeue();
                    // ��Ϊ���ź�
                    m_dequeueWait.Reset();
                    // �����źţ�֪ͨ�������waitOne�ɼ���ִ�У��ɽ�����Ӳ���
                    m_enqueueWait.Set();
                    // �����־
                    OutLog($"{m_name} ���ӳɹ�.");
                    return true;
                }
            }
            // ������������ݣ����������У�ֹͣ���ӣ��ȴ��ź�
            m_dequeueWait.WaitOne();
        }
    }
    #endregion
}