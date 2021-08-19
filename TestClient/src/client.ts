import * as grpc from "@grpc/grpc-js";
import * as chalk from "chalk";
import {FileBrowserClient} from "../DataApi/DataApi_grpc_pb";
import {FileListRequest, FileList, ImageInfo, FileRequest, OpenImageResponse, CloseFileRequest, GetDataRequest, DataResponse} from "../DataApi/DataApi_pb";
import {ServiceError} from "@grpc/grpc-js";
import {SurfaceCall} from "@grpc/grpc-js/build/src/call";

const serverAddress = process.argv[process.argv.length - 1];
const fileBrowserClient = new FileBrowserClient(serverAddress, grpc.credentials.createInsecure(), {'grpc.max_receive_message_length': 200e6});


function Promisify<Req, Res>(func: (req: Req, callback: (err: ServiceError | null, res?: Res) => void) => SurfaceCall) {
    const boundFunc = func.bind(fileBrowserClient);
    return (req: Req) => {
        return new Promise<Res>((resolve, reject) =>
            boundFunc(req, function (err: ServiceError | null, response?: Res) {
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
}

const GetFileList = Promisify<FileListRequest, FileList>(fileBrowserClient.getFileList);
const GetImageInfo = Promisify<FileRequest, ImageInfo>(fileBrowserClient.getImageInfo);
const OpenImage = Promisify<FileRequest, OpenImageResponse>(fileBrowserClient.openImage);
const CloseFile = Promisify<CloseFileRequest, void>(fileBrowserClient.closeImage);
const GetData = Promisify<GetDataRequest, DataResponse>(fileBrowserClient.getData.bind(fileBrowserClient));

const req = new FileListRequest();
req.setDirectoryname("fits/vr");

GetFileList(req).then(async res => {
    console.log(chalk.bold(res.getDirectoryname()));

    const subdirs = res.getSubdirectoriesList();
    const files = res.getFilesList();
    for (const subdir of subdirs) {
        console.log(`${subdir.getName()}: ${subdir.getNumitems()} items`);
    }

    let firstCubeName = "m81.fits";

    for (const file of files) {
        try {
            const req = new FileRequest();
            req.setDirectoryname(res.getDirectoryname());
            req.setFilename(file.getName());
            // All at once:
            // GetImageInfo(req).then(imageInfo=>{
            //     console.log(chalk.green(`${imageInfo.getFilename()}:`), `${imageInfo.getDimensionsList()?.join("\u00D7")}`);
            // });
            // Sequential
            const imageInfo = await GetImageInfo(req);
            console.log(chalk.green(`${imageInfo.getFilename()}:`), `${imageInfo.getDimensionsList()?.join("\u00D7")}`);
            const header = imageInfo.getHeaderList();
            const unitEntry = header.find(entry => entry.getKey() === "BUNIT");
            console.log(unitEntry?.getValue());
            if (!firstCubeName && imageInfo.getDimensionsList()?.length == 3) {
                firstCubeName = file.getName();
            }
        } catch (err) {
            console.error(err);
        }
    }
    if (firstCubeName) {
        let tStart = performance.now();
        const openReq = new FileRequest();
        openReq.setDirectoryname(res.getDirectoryname());
        openReq.setFilename(firstCubeName);
        const fileId = (await OpenImage(openReq)).getFileid();
        let tEnd = performance.now();
        let dt = tEnd - tStart;
        console.log(`File ${firstCubeName} opened with fileId=${fileId} in ${dt.toFixed(1)} ms`);
        const dataReq = new GetDataRequest();
        dataReq.setFileid(fileId);
        tStart = performance.now();
        const dataResponse = await GetData(dataReq);
        const rawData = dataResponse.getRawdata() as Uint8Array;
        const floatList = dataResponse.getDataList();
        let floatArray: Float32Array;
        if (rawData.byteLength) {
            floatArray = new Float32Array(rawData.slice().buffer);
        } else {
            floatArray = new Float32Array(floatList);
        }
        tEnd = performance.now();
        dt = tEnd - tStart;

        const dataLengthMb = 4 * floatArray.length * 1e-6;
        // MB/s
        const rate = dataLengthMb / dt * 1e3;
        console.log(`Received ${dataLengthMb.toFixed(1)} MB of data for fileId=${fileId} in ${dt.toFixed(1)} ms at ${rate.toFixed(1)} MB/s`);
        console.log(floatArray.slice(0, 5));
        const closeFileReq = new CloseFileRequest();
        closeFileReq.setFileid(fileId)
        await CloseFile(closeFileReq);
    }
}, err => {
    console.error(err);
    return;
});