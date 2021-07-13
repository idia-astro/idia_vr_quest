import {FileBrowserClient} from "../DataApi/DataApi_grpc_pb";
import * as grpc from "@grpc/grpc-js";
import {FileListRequest, ImageInfoRequest} from "../DataApi/DataApi_pb";

const fileBrowserClient = new FileBrowserClient("localhost:50051", grpc.credentials.createInsecure());

const req = new FileListRequest();
req.setDirectoryname("fits");

fileBrowserClient.getFileList(req, (err, res) => {
    if (err || !res) {
        console.error(err);
        return;
    }

    console.log(res.getDirectoryname());

    const subdirs = res.getSubdirectoriesList();
    const files = res.getFilesList();
    for (const subdir of subdirs) {
        console.log(`${subdir.getName()}: ${subdir.getNumitems()} items`);
    }

    for (const file of files) {
        console.log(`${file.getName()}: ${file.getSize()} bytes`);
        const req = new ImageInfoRequest();
        req.setDirectoryname(res.getDirectoryname());
        req.setFilename(file.getName());
        fileBrowserClient.getImageInfo(req, (err1, res1) => {
            if (err1) {
                console.error(err);
            } else if (res1) {
                console.log(`${res1.getFilename()}: ${res1.getFiletype()}`);
            }
        });
    }
});
