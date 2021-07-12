// GENERATED CODE -- DO NOT EDIT!

'use strict';
var grpc = require('@grpc/grpc-js');
var DataApi_pb = require('./DataApi_pb.js');

function serialize_DataApi_FileList(arg) {
  if (!(arg instanceof DataApi_pb.FileList)) {
    throw new Error('Expected argument of type DataApi.FileList');
  }
  return Buffer.from(arg.serializeBinary());
}

function deserialize_DataApi_FileList(buffer_arg) {
  return DataApi_pb.FileList.deserializeBinary(new Uint8Array(buffer_arg));
}

function serialize_DataApi_FileListRequest(arg) {
  if (!(arg instanceof DataApi_pb.FileListRequest)) {
    throw new Error('Expected argument of type DataApi.FileListRequest');
  }
  return Buffer.from(arg.serializeBinary());
}

function deserialize_DataApi_FileListRequest(buffer_arg) {
  return DataApi_pb.FileListRequest.deserializeBinary(new Uint8Array(buffer_arg));
}

function serialize_DataApi_ImageInfo(arg) {
  if (!(arg instanceof DataApi_pb.ImageInfo)) {
    throw new Error('Expected argument of type DataApi.ImageInfo');
  }
  return Buffer.from(arg.serializeBinary());
}

function deserialize_DataApi_ImageInfo(buffer_arg) {
  return DataApi_pb.ImageInfo.deserializeBinary(new Uint8Array(buffer_arg));
}

function serialize_DataApi_ImageInfoRequest(arg) {
  if (!(arg instanceof DataApi_pb.ImageInfoRequest)) {
    throw new Error('Expected argument of type DataApi.ImageInfoRequest');
  }
  return Buffer.from(arg.serializeBinary());
}

function deserialize_DataApi_ImageInfoRequest(buffer_arg) {
  return DataApi_pb.ImageInfoRequest.deserializeBinary(new Uint8Array(buffer_arg));
}


var FileBrowserService = exports.FileBrowserService = {
  getFileList: {
    path: '/DataApi.FileBrowser/GetFileList',
    requestStream: false,
    responseStream: false,
    requestType: DataApi_pb.FileListRequest,
    responseType: DataApi_pb.FileList,
    requestSerialize: serialize_DataApi_FileListRequest,
    requestDeserialize: deserialize_DataApi_FileListRequest,
    responseSerialize: serialize_DataApi_FileList,
    responseDeserialize: deserialize_DataApi_FileList,
  },
  getImageInfo: {
    path: '/DataApi.FileBrowser/GetImageInfo',
    requestStream: false,
    responseStream: false,
    requestType: DataApi_pb.ImageInfoRequest,
    responseType: DataApi_pb.ImageInfo,
    requestSerialize: serialize_DataApi_ImageInfoRequest,
    requestDeserialize: deserialize_DataApi_ImageInfoRequest,
    responseSerialize: serialize_DataApi_ImageInfo,
    responseDeserialize: deserialize_DataApi_ImageInfo,
  },
};

exports.FileBrowserClient = grpc.makeGenericClientConstructor(FileBrowserService);
