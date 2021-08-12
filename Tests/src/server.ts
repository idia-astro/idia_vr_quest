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