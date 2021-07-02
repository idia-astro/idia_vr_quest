import * as express from 'express';
import * as http from 'http';
import * as WebSocket from 'ws';

const app = express();

const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

enum MessageType {
    Empty = 0,
    FileListRequest = 1,
    FileListResponse = 2
}

interface EventHeader {
    requestId: number;
    messageType: MessageType;
}

interface FileListRequest {
    directoryName: string;
}

interface FileInfo {
    name: string;
    size: number;
    date: number;
}

interface DirectoryInfo {
    name: string;
    numItems: number;
    date: number;
}

interface FileListResponse {
    directoryName: string;
    subDirectories?: DirectoryInfo[];
    files?: FileInfo[];
}

function ObjectFromData(dataArray: Uint8Array) {
    const objData = dataArray.subarray(8);
    const objJson = new TextDecoder().decode(objData);
    return JSON.parse(objJson);
}

function HandleFileListRequest(ws: WebSocket, requestId: number, req: FileListRequest) {
    const res: FileListResponse = {
        directoryName: req.directoryName
    };
    SendEvent(ws, requestId, MessageType.FileListResponse, res);
}

function SendEvent(ws: WebSocket, requestId: number, messageType: MessageType, obj: any) {
    const headerData = new Uint32Array(2);
    headerData[0] = requestId;
    headerData[1] = messageType;
    const dataArray = new TextEncoder().encode(JSON.stringify(obj));
    console.log(headerData);
    console.log(dataArray);
    const messageArray = new Uint8Array(8 + dataArray.length);
    messageArray.set(new Uint8Array(headerData.buffer, 0, 8));
    messageArray.set(dataArray, 8);
    ws.send(messageArray);
}

wss.on("connection", function(ws) {
    console.log("client joined.");
    ws.on("message", function(data: ArrayBuffer) {
        const dataArray = new Uint8Array(data);

        if (dataArray.length >= 8) {
            const headerData = dataArray.slice(0, 8);
            const u32Vals = new Uint32Array(headerData.buffer, 0, 2);
            const header: EventHeader = {
                requestId: u32Vals[0],
                messageType: u32Vals[1]
            }

            if (header.messageType === MessageType.FileListRequest) {
                try {
                    const obj = ObjectFromData(dataArray);
                    HandleFileListRequest(ws, header.requestId, obj as FileListRequest);
                } catch (e) {
                    console.log(`Invalid message of type ${header.messageType}`);
                }
            }


        } else {
            console.log(`Invalid message recieved (length = ${dataArray.length}`);
        }
});

});

server.listen(3000, function() {
    console.log("Listening on http://localhost:3000");
});