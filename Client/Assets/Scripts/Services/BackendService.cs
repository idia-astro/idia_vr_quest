using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Models;
using NativeWebSocket;
using UnityEngine;
using Newtonsoft.Json;

namespace Services
{
    public enum MessageType : uint
    {
        Empty = 0,
        FileListRequest = 1,
        FileListResponse = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EventHeader
    {
        public uint RequestId;
        public MessageType MessageType;
    }

    public class BackendService : MonoBehaviour
    {
        public static string Address = "ws://localhost:3000";

        private WebSocket _webSocket;
        private uint _requestId;

        async void Awake()
        {
            _webSocket = new WebSocket(Address);
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnOpen += OnConnect;
            _webSocket.OnError += OnError;
            await _webSocket.Connect();
        }

        async void Connect()
        {
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _webSocket.DispatchMessageQueue();
#endif
        }

        public async Task<FileListResponse> GetFileList(string directory)
        {
            var success = await SendMessage(MessageType.FileListRequest, new FileListRequest {directoryName = directory});
            // TODO: actually wait for response!
            return new FileListResponse {directoryName = directory};
        }
        
        private async Task<bool> SendMessage(MessageType messageType, object obj)
        {
            var header = new EventHeader {RequestId = _requestId, MessageType = messageType};
            _requestId++;

            var headerBytes = ConvertToByteArray(header);
            string json = JsonConvert.SerializeObject(obj);
            var data = Encoding.UTF8.GetBytes(json);

            var messageData = new byte[headerBytes.Length + data.Length];
            System.Buffer.BlockCopy(headerBytes, 0, messageData, 0, headerBytes.Length);
            System.Buffer.BlockCopy(data, 0, messageData, headerBytes.Length, data.Length);
            
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.Send(messageData);
                return true;
            }

            return false;
        }
        
        private static byte[] ConvertToByteArray(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }


        private void OnMessage(byte[] data)
        {
            var message = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("Received OnMessage! (" + data.Length + " bytes) " + message);
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