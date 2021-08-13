import * as grpc from "@grpc/grpc-js";
import * as chalk from "chalk";
import {FileBrowserClient} from "../DataApi/DataApi_grpc_pb";
import {FileListRequest, ImageInfo, ImageInfoRequest} from "../DataApi/DataApi_pb";

const fileBrowserClient = new FileBrowserClient("localhost:50051", grpc.credentials.createInsecure());

async function GetImageInfo(arg: ImageInfoRequest): Promise<ImageInfo> {
    return new Promise((resolve, reject) =>
        fileBrowserClient.getImageInfo(arg, function (err, response) {
            if (err) {
                return reject(err);
            }
            if (!response) {
                return reject("no response");
            }
            resolve(response);
        })
    );
}

const req = new FileListRequest();
req.setDirectoryname("fits");

fileBrowserClient.getFileList(req, async (err, res) => {
    if (err || !res) {
        console.error(err);
        return;
    }

    console.log(chalk.bold(res.getDirectoryname()));

    const subdirs = res.getSubdirectoriesList();
    const files = res.getFilesList();
    for (const subdir of subdirs) {
        console.log(`${subdir.getName()}: ${subdir.getNumitems()} items`);
    }

    for (const file of files) {
        const req = new ImageInfoRequest();
        req.setDirectoryname(res.getDirectoryname());
        req.setFilename(file.getName());
        try {
            const imageInfo = await GetImageInfo(req);
            console.log(chalk.green(`${imageInfo.getFilename()}:`), `${imageInfo.getDimensionsList()?.join("\u00D7")}`);
        } catch (err) {
            console.error(err);
        }
    }
});
