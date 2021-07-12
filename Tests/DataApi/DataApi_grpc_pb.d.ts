// GENERATED CODE -- DO NOT EDIT!

// package: DataApi
// file: DataApi.proto

import * as DataApi_pb from "./DataApi_pb";
import * as grpc from "@grpc/grpc-js";

interface IFileBrowserService extends grpc.ServiceDefinition<grpc.UntypedServiceImplementation> {
  getFileList: grpc.MethodDefinition<DataApi_pb.FileListRequest, DataApi_pb.FileList>;
  getImageInfo: grpc.MethodDefinition<DataApi_pb.ImageInfoRequest, DataApi_pb.ImageInfo>;
}

export const FileBrowserService: IFileBrowserService;

export interface IFileBrowserServer extends grpc.UntypedServiceImplementation {
  getFileList: grpc.handleUnaryCall<DataApi_pb.FileListRequest, DataApi_pb.FileList>;
  getImageInfo: grpc.handleUnaryCall<DataApi_pb.ImageInfoRequest, DataApi_pb.ImageInfo>;
}

export class FileBrowserClient extends grpc.Client {
  constructor(address: string, credentials: grpc.ChannelCredentials, options?: object);
  getFileList(argument: DataApi_pb.FileListRequest, callback: grpc.requestCallback<DataApi_pb.FileList>): grpc.ClientUnaryCall;
  getFileList(argument: DataApi_pb.FileListRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<DataApi_pb.FileList>): grpc.ClientUnaryCall;
  getFileList(argument: DataApi_pb.FileListRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<DataApi_pb.FileList>): grpc.ClientUnaryCall;
  getImageInfo(argument: DataApi_pb.ImageInfoRequest, callback: grpc.requestCallback<DataApi_pb.ImageInfo>): grpc.ClientUnaryCall;
  getImageInfo(argument: DataApi_pb.ImageInfoRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<DataApi_pb.ImageInfo>): grpc.ClientUnaryCall;
  getImageInfo(argument: DataApi_pb.ImageInfoRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<DataApi_pb.ImageInfo>): grpc.ClientUnaryCall;
}
