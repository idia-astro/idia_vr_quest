import * as grpc from "@grpc/grpc-js";
import * as chalk from "chalk";
import {FileBrowserClient} from "../DataApi/DataApi_grpc_pb";
import {FileListRequest, ImageInfo, FileRequest} from "../DataApi/DataApi_pb";

const serverAddress = process.argv[process.argv.length - 1];
const fileBrowserClient = new FileBrowserClient(process.argv[process.argv.length - 1], grpc.credentials.createInsecure());

async function GetImageInfo(arg: FileRequest): Promise<ImageInfo> {
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
        const req = new FileRequest();
        req.setDirectoryname(res.getDirectoryname());
        req.setFilename(file.getName());
        try {
            // All at once:
            // GetImageInfo(req).then(imageInfo=>{
            //     console.log(chalk.green(`${imageInfo.getFilename()}:`), `${imageInfo.getDimensionsList()?.join("\u00D7")}`);
            // });
            // Sequential
            const imageInfo = await GetImageInfo(req);
            console.log(chalk.green(`${imageInfo.getFilename()}:`), `${imageInfo.getDimensionsList()?.join("\u00D7")}`);
            const header = imageInfo.getHeaderList();
            const unitEntry = header.find(entry=>entry.getKey()==="BUNIT");
            console.log(unitEntry?.getValue());
        } catch (err) {
            console.error(err);
        }
    }
});
