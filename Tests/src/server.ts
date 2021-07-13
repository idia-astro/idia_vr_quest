import * as grpc from "@grpc/grpc-js";
import * as fs from "fs/promises";
import * as path from "path";
import {sendUnaryData, ServerUnaryCall, UntypedHandleCall} from "@grpc/grpc-js";
import {FileBrowserService, IFileBrowserServer} from "../DataApi/DataApi_grpc_pb";
import {DirectoryInfo, FileInfo, FileList, FileListRequest, FileType, ImageInfo, ImageInfoRequest} from "../DataApi/DataApi_pb";
import {getMagicNumber} from "./util";

let basePath = process.argv[2];

const fitsMagicNumber = 6001412985720217632n;

class FileBrowserServer implements IFileBrowserServer {
    getImageInfo = async (call: ServerUnaryCall<ImageInfoRequest, ImageInfo>, callback: sendUnaryData<ImageInfo>) => {
        const directoryName = call.request.getDirectoryname();
        const fileName = call.request.getFilename();
        const filePath = path.join(basePath, directoryName, fileName);

        try {
            const fd = await fs.open(filePath, "r");
            const magicNum = await getMagicNumber(fd);
            const response = new ImageInfo();
            response.setFilename(fileName);
            response.setDimensionsList([0]);
            response.setFiletype(magicNum === fitsMagicNumber ? FileType.FITS : FileType.UNKNOWN);
            callback(null, response);
            await fd.close();
        } catch (err) {
            const errResponse: Error = {
                name: "File info error",
                message: err
            };
            callback(errResponse, null);
        }
    };

    getFileList = async (call: ServerUnaryCall<FileListRequest, FileList>, callback: sendUnaryData<FileList>) => {
        const directoryName = call.request.getDirectoryname();
        const dirPath = path.join(basePath, directoryName);

        const response = new FileList();
        const subdirs = response.getSubdirectoriesList();
        const files = response.getFilesList();
        response.setDirectoryname(directoryName);
        try {
            const list = await fs.readdir(dirPath);
            for (const f of list) {
                const fullPath = path.join(dirPath, f);
                const stat = await fs.stat(fullPath);
                if (stat.isDirectory()) {
                    const subDir = await fs.readdir(fullPath);
                    const dirInfo = new DirectoryInfo();
                    dirInfo.setName(f);
                    dirInfo.setNumitems(subDir.length);
                    dirInfo.setDate(stat.mtime.getDate());
                    subdirs.push(dirInfo);
                } else if (stat.isFile()) {
                    const fileInfo = new FileInfo();
                    fileInfo.setName(f);
                    fileInfo.setSize(stat.size);
                    fileInfo.setDate(stat.mtime.getDate());
                    files.push(fileInfo);
                }
            }
        } catch (err) {
            console.log(err);
            const errResponse: Error = {
                name: "File list error",
                message: err
            };

            callback(errResponse, null);
        }
        callback(null, response);
    };

    [name: string]: UntypedHandleCall;
}

const server = new grpc.Server();
server.addService(FileBrowserService, new FileBrowserServer());

server.bindAsync("0.0.0.0:50051", grpc.ServerCredentials.createInsecure(), () => {
    console.log("Starting gRPC server");
    server.start();
});

//
//
//
// import * as http from 'http';
// import * as fs from "fs/promises";
// import * as path from "path";
// import * as chalk from "chalk";
// import * as express from 'express';
// import * as WebSocket from 'ws';
//
// import {FileBrowserService} from "./DataApi/DataApi_grpc_pb";
// import {FileListRequest} from "./DataApi/DataApi_pb";
//
// const fbService = new FileBrowserService("http://localhost:50051");
// const req = new FileListRequest();
// req.setDirectoryname("testing");
// fbService.getFileList(req, (err, res)=>{
//     console.log(err);
// })
//
//
//
// let basePath = process.argv[2];
//
// const icdVersion = 1;
// const logMessageCutoff = 150;
//
// const app = express();
//
// const server = http.createServer(app);
// const wss = new WebSocket.Server({server});
//
// enum MessageType {
//     Unknown = 0,
//     Error = 1,
//     FileListRequest = 2,
//     FileListResponse = 3,
//     ExtendedFileInfoRequest = 4,
//     ExtendedFileInfoResponse = 5
// }
//
// const logMap = new Map<MessageType, boolean>([
//     [MessageType.FileListRequest, true],
//     [MessageType.FileListResponse, true],
//     [MessageType.ExtendedFileInfoRequest, true],
//     [MessageType.ExtendedFileInfoResponse, true],
// ]);
//
// class EventHeader {
//     messageType: MessageType;
//     icdVersion: number;
//     requestId: number;
//
//     constructor(messageType: MessageType, icdVersion: number, requestId: number) {
//         this.messageType = messageType;
//         this.icdVersion = icdVersion;
//         this.requestId = requestId;
//     }
//
//     static ToBytes(messageType: MessageType, icdVersion: number, requestId: number) {
//         const headerBuffer = new ArrayBuffer(8);
//         const headerView16 = new Uint16Array(headerBuffer, 0, 2);
//         const headerView32 = new Uint32Array(headerBuffer, 4, 1);
//         headerView16[0] = messageType;
//         headerView16[1] = icdVersion;
//         headerView32[0] = requestId;
//         return new Uint8Array(headerBuffer, 0, 8);
//     }
//
//     static ToBytesFromHeader(eventHeader: EventHeader): Uint8Array {
//         return EventHeader.ToBytes(eventHeader.messageType, eventHeader.icdVersion, eventHeader.requestId);
//     }
//
//     static FromBytes(bytes: Uint8Array): EventHeader {
//         if (bytes.length < 8) {
//             return {messageType: 0, icdVersion: 0, requestId: 0};
//         }
//
//         // Create a temp copy if required
//         if (bytes.byteOffset % 4 !== 0) {
//             bytes = bytes.slice(0, 8);
//         }
//         const headerView16 = new Uint16Array(bytes.buffer, 0, 2);
//         const headerView32 = new Uint32Array(bytes.buffer, 4, 1);
//         return {
//             messageType: headerView16[0] as MessageType,
//             icdVersion: headerView16[1],
//             requestId: headerView32[0]
//         };
//     }
// }
//
// interface FileListRequest {
//     directoryName: string;
// }
//
// interface FileInfo {
//     name: string;
//     size: number;
//     date: number;
// }
//
// interface DirectoryInfo {
//     name: string;
//     numItems: number;
//     date: number;
// }
//
// interface FileListResponse {
//     directoryName: string;
//     subDirectories?: DirectoryInfo[];
//     files?: FileInfo[];
// }
//
// interface ExtendedFileInfoRequest {
//     directoryName: string;
//     fileName: string;
// }
//
// enum FileType {
//     Unknown,
//     Fits = 1,
//     Hdf5 = 2,
//     Mhd = 3
// }
//
// interface ExtendedFileInfoResponse {
//     fileName: string;
//     fileType: FileType;
//     dimensions: number[];
// }
//
// function ObjectFromData(dataArray: Uint8Array) {
//     const objData = dataArray.subarray(8);
//     const objJson = new TextDecoder().decode(objData);
//     return JSON.parse(objJson);
// }
//
// async function HandleFileListRequest(ws: WebSocket, requestId: number, req: FileListRequest) {
// let basePath = process.argv[2];

//     const dirPath = path.join(basePath, req.directoryName);
//
//     const res: FileListResponse = {
//         directoryName: req.directoryName,
//         subDirectories: [],
//         files: []
//     }
//
//     try {
//         const list = await fs.readdir(dirPath);
//         for (const f of list) {
//             const fullPath = path.join(dirPath, f);
//             const stat = await fs.stat(fullPath);
//             if (stat.isDirectory()) {
//                 const subDir = await fs.readdir(fullPath);
//                 res.subDirectories.push({
//                     name: f,
//                     numItems: subDir.length,
//                     date: stat.mtime.getDate()
//                 });
//             } else if (stat.isFile()) {
//                 res.files.push({
//                     name: f,
//                     size: stat.size,
//                     date: stat.mtime.getDate(),
//                 });
//             }
//         }
//         SendEvent(ws, requestId, MessageType.FileListResponse, res);
//     } catch (err) {
//         console.log(err);
//         SendEvent(ws, requestId, MessageType.Error, {message: err.message});
//     }
// }
//
// function HandleExtendedFileInfoRequest(ws: WebSocket, requestId: number, req: ExtendedFileInfoRequest) {
//     const res: ExtendedFileInfoResponse = {
//         fileName: req.fileName,
//         fileType: FileType.Fits,
//         dimensions: [0]
//     };
//     SendEvent(ws, requestId, MessageType.ExtendedFileInfoResponse, res);
// }
//
// function LogEvent(messageType: MessageType, output: string | object, outgoing: boolean) {
//     if (logMap.has(messageType)) {
//         let jsonString: string;
//         if (typeof output == "string") {
//             jsonString = output;
//         } else {
//             jsonString = JSON.stringify(output);
//         }
//
//         if (jsonString.length > logMessageCutoff) {
//             jsonString = jsonString.substr(0, logMessageCutoff) + "...";
//         }
//
//         console.log(`${chalk.bold(MessageType[messageType])} ${outgoing ? chalk.red(">>") : chalk.green("<<")} ${chalk.gray(jsonString)}`);
//     }
// }
//
// function SendEvent(ws: WebSocket, requestId: number, messageType: MessageType, obj: any) {
//     const jsonString = JSON.stringify(obj);
//
//    LogEvent(messageType, obj, true);
//     const dataArray = new TextEncoder().encode(jsonString);
//     const messageArray = new Uint8Array(8 + dataArray.length);
//     messageArray.set(EventHeader.ToBytes(messageType, icdVersion, requestId));
//     messageArray.set(dataArray, 8);
//     ws.send(messageArray);
// }
//
// wss.on("connection", function (ws) {
//     console.log("client joined.");
//     ws.on("message", function (data: ArrayBuffer) {
//         const dataArray = new Uint8Array(data);
//         if (dataArray.length >= 8) {
//             const eventHeader = EventHeader.FromBytes(dataArray);
//             let obj: any;
//
//             if (eventHeader.messageType != MessageType.Unknown && eventHeader.icdVersion === icdVersion) {
//                 try {
//                     obj = ObjectFromData(dataArray);
//                 } catch (e) {
//                     console.log(`Error parsing message of type ${eventHeader.messageType}`);
//                     console.error(e);
//                 }
//             } else {
//                 console.log(`Invalid message of type ${eventHeader.messageType} and ICD version ${eventHeader.icdVersion}`);
//             }
//
//             if (!obj) {
//                 return;
//             }
//
//             LogEvent(eventHeader.messageType, obj, false);
//
//             switch (eventHeader.messageType) {
//                 case MessageType.FileListRequest:
//                     HandleFileListRequest(ws, eventHeader.requestId, obj as FileListRequest);
//                     break;
//                 case MessageType.ExtendedFileInfoRequest:
//                     HandleExtendedFileInfoRequest(ws, eventHeader.requestId, obj as ExtendedFileInfoRequest);
//                     break;
//                 default:
//                     console.log(`No message handler for event type ${eventHeader.messageType}`);
//                     break;
//             }
//         } else {
//             console.log(`Invalid message received (length = ${dataArray.length}`);
//         }
//     });
// });
//
// server.listen(3000, function () {
//     console.log("Listening on http://localhost:3000");
// });