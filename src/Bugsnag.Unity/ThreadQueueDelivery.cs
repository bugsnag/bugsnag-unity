using Bugsnag.Unity.Payload;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Bugsnag.Unity
{
  class ThreadQueueDelivery
  {
    private static ThreadQueueDelivery instance = null;
    private static readonly object instanceLock = new object();

    private readonly BlockingQueue<IPayload> _queue;

    private readonly Thread _worker;

    private ThreadQueueDelivery()
    {
      _queue = new BlockingQueue<IPayload>();
      _worker = new Thread(new ThreadStart(ProcessQueue)) { IsBackground = true };
      _worker.Start();
    }

    public static ThreadQueueDelivery Instance
    {
      get
      {
        lock (instanceLock)
        {
          if (instance == null)
          {
            instance = new ThreadQueueDelivery();
          }

          return instance;
        }
      }
    }

    private void ProcessQueue()
    {
      while (true)
      {
        var payload = _queue.Dequeue();
        var request = new UnityWebRequest(payload.Endpoint.OriginalString.ToString(), "POST");

        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        request.SetRequestHeader("Bugsnag-Sent-At", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

        foreach (var header in payload.Headers)
        {
          request.SetRequestHeader(header.Key, header.Value);
        }

        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
        {
          SimpleJson.SimpleJson.SerializeObject(payload, writer);
          writer.Flush();
          request.uploadHandler = new UploadHandlerRaw(stream.ToArray());
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        var response = request.SendWebRequest();

        response.completed += ReportCallback;
      }
    }

    private void ReportCallback(AsyncOperation asyncOperation)
    {
      // do we need to do this?
      asyncOperation.completed -= ReportCallback;
    }

    public void Send(IPayload payload)
    {
      _queue.Enqueue(payload);
    }

    private class BlockingQueue<T>
    {
      private readonly Queue<T> _queue;
      private readonly object _queueLock;

      public BlockingQueue()
      {
        _queueLock = new object();
        _queue = new Queue<T>();
      }

      public void Enqueue(T item)
      {
        lock (_queueLock)
        {
          _queue.Enqueue(item);
          Monitor.Pulse(_queueLock);
        }
      }

      public T Dequeue()
      {
        lock (_queueLock)
        {
          while (_queue.Count == 0)
          {
            Monitor.Wait(_queueLock);
          }

          return _queue.Dequeue();
        }
      }
    }
  }
}
