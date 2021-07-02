using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Models;
using NativeWebSocket;
using UnityEngine;
using Newtonsoft.Json;

namespace Services
{
    public enum MessageType : ushort
    {
        Unknown = 0,
        Error = 1,
        FileListRequest = 2,
        FileListResponse = 3,
        ExtendedFileInfoRequest = 4,
        ExtendedFileInfoResponse = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EventHeader
    {
        public MessageType MessageType;
        public ushort IcdVersion;
        public uint RequestId;

        public static EventHeader FromBytes(byte[] data)
        {
            if (data.Length < 8)
            {
                return new EventHeader {MessageType = MessageType.Unknown, IcdVersion = 0, RequestId = 0};
            }

            if (data.Length != 8)
            {
                var subarray = new byte[8];
                Array.Copy(data, 0, subarray, 0, 8);
                data = subarray;
            }

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            EventHeader eventHeader;
            try
            {
                eventHeader = (EventHeader) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(EventHeader));
            }
            catch
            {
                return new EventHeader {MessageType = MessageType.Unknown, IcdVersion = 0, RequestId = 0};
            }
            finally
            {
                handle.Free();
            }

            return eventHeader;
        }

        public static byte[] ToBytes(EventHeader eventHeader)
        {
            byte[] arr = new byte[8];
            IntPtr ptr = Marshal.AllocHGlobal(8);

            Marshal.StructureToPtr(eventHeader, ptr, true);
            Marshal.Copy(ptr, arr, 0, 8);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }

    public class BackendService : MonoBehaviour
    {
        public static string Address = "ws://localhost:3000";

        public static readonly ushort IcdVersion = 1;
        private WebSocket _webSocket;
        private uint _requestCounter;

        // There's probably a more elegant way to do this
        private readonly Dictionary<uint, TaskCompletionSource<FileListResponse>> _promiseMapFileListResponse = new Dictionary<uint, TaskCompletionSource<FileListResponse>>();

        private readonly Dictionary<uint, TaskCompletionSource<ExtendedFileInfoResponse>> _promiseMapExtendedFileInfoResponse =
            new Dictionary<uint, TaskCompletionSource<ExtendedFileInfoResponse>>();

        async void Awake()
        {
            _webSocket = new WebSocket(Address);
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnOpen += OnConnect;
            _webSocket.OnError += OnError;
            await Connect();
        }

        async Task Connect()
        {
            await _webSocket.Connect();
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _webSocket.DispatchMessageQueue();
#endif
        }

        public async Task<FileListResponse> GetFileList(string directory)
        {
            var requestId = await SendMessage(MessageType.FileListRequest, new FileListRequest {directoryName = directory});
            var promise = new TaskCompletionSource<FileListResponse>();
            if (requestId >= 0)
            {
                _promiseMapFileListResponse.Add((uint) requestId, promise);
            }

            return await promise.Task;
        }
        
        public async Task<ExtendedFileInfoResponse> GetExtendedFileInfo(string directory, string name)
        {
            var requestId = await SendMessage(MessageType.ExtendedFileInfoRequest, new ExtendedFileInfoRequest {directoryName = directory, fileName = name});
            var promise = new TaskCompletionSource<ExtendedFileInfoResponse>();
            if (requestId >= 0)
            {
                _promiseMapExtendedFileInfoResponse.Add((uint) requestId, promise);
            }

            return await promise.Task;
        }

        private async Task<int> SendMessage(MessageType messageType, object obj)
        {
            var header = new EventHeader {IcdVersion = IcdVersion, RequestId = _requestCounter, MessageType = messageType};
            _requestCounter++;

            var headerBytes = EventHeader.ToBytes(header);
            string json = JsonConvert.SerializeObject(obj);
            var data = Encoding.UTF8.GetBytes(json);

            var messageData = new byte[headerBytes.Length + data.Length];
            System.Buffer.BlockCopy(headerBytes, 0, messageData, 0, headerBytes.Length);
            System.Buffer.BlockCopy(data, 0, messageData, headerBytes.Length, data.Length);

            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.Send(messageData);
                return (int) header.RequestId;
            }

            return -1;
        }

        private void OnMessage(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data, 8, data.Length - 8);
            var eventHeader = EventHeader.FromBytes(data);

            // There's probably a better way to handle this for multiple response types
            if (_promiseMapFileListResponse.TryGetValue(eventHeader.RequestId, out var tFileListResponse))
            {
                HandleFileListResponse(eventHeader, message, tFileListResponse);
                _promiseMapFileListResponse.Remove(eventHeader.RequestId);
            }
            else if (_promiseMapExtendedFileInfoResponse.TryGetValue(eventHeader.RequestId, out var tFileInfoResponse))
            {
                HandleExtendedFileInfoResponse(eventHeader, message, tFileInfoResponse);
                _promiseMapExtendedFileInfoResponse.Remove(eventHeader.RequestId);
            }
            else
            {
                Debug.LogError($"No promise for response with requestId={eventHeader.RequestId}");
            }
        }

        private void HandleFileListResponse(EventHeader eventHeader,string message, TaskCompletionSource<FileListResponse> task)
        {
            if (eventHeader.MessageType == MessageType.FileListResponse)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<FileListResponse>(message);
                    task.SetResult(obj);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    task.SetException(new BackendException("Invalid response type"));
                    return;
                }
            }

            if (eventHeader.MessageType == MessageType.Error)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<Error>(message);
                    task.SetException(new BackendException(obj));
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    task.SetException(new BackendException("Invalid response type"));
                    return;
                }
            }

            task.SetException(new BackendException("Invalid response type"));
        }

        private void HandleExtendedFileInfoResponse(EventHeader eventHeader, string message, TaskCompletionSource<ExtendedFileInfoResponse> task)
        {
            if (eventHeader.MessageType == MessageType.ExtendedFileInfoResponse)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<ExtendedFileInfoResponse>(message);
                    task.SetResult(obj);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    task.SetException(new BackendException("Invalid response type"));
                    return;
                }
            }

            if (eventHeader.MessageType == MessageType.Error)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<Error>(message);
                    task.SetException(new BackendException(obj));
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    task.SetException(new BackendException("Invalid response type"));
                    return;
                }
            }

            task.SetException(new BackendException("Invalid response type"));
        }

        private void OnError(string err)
        {
            Debug.LogError("Websocket error: " + err);
        }

        private void OnConnect()
        {
            Debug.Log("Connected to websocket");
        }

        private void OnClose()
        {
            Debug.Log("Disconnected from websocket");
        }

        private async void OnApplicationQuit()
        {
            await _webSocket.Close();
        }
    }
}