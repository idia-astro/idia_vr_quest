// package: DataApi
// file: DataApi.proto

import * as jspb from "google-protobuf";

export class FileListRequest extends jspb.Message {
  getDirectoryname(): string;
  setDirectoryname(value: string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): FileListRequest.AsObject;
  static toObject(includeInstance: boolean, msg: FileListRequest): FileListRequest.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: FileListRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): FileListRequest;
  static deserializeBinaryFromReader(message: FileListRequest, reader: jspb.BinaryReader): FileListRequest;
}

export namespace FileListRequest {
  export type AsObject = {
    directoryname: string,
  }
}

export class FileList extends jspb.Message {
  getDirectoryname(): string;
  setDirectoryname(value: string): void;

  clearSubdirectoriesList(): void;
  getSubdirectoriesList(): Array<DirectoryInfo>;
  setSubdirectoriesList(value: Array<DirectoryInfo>): void;
  addSubdirectories(value?: DirectoryInfo, index?: number): DirectoryInfo;

  clearFilesList(): void;
  getFilesList(): Array<FileInfo>;
  setFilesList(value: Array<FileInfo>): void;
  addFiles(value?: FileInfo, index?: number): FileInfo;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): FileList.AsObject;
  static toObject(includeInstance: boolean, msg: FileList): FileList.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: FileList, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): FileList;
  static deserializeBinaryFromReader(message: FileList, reader: jspb.BinaryReader): FileList;
}

export namespace FileList {
  export type AsObject = {
    directoryname: string,
    subdirectoriesList: Array<DirectoryInfo.AsObject>,
    filesList: Array<FileInfo.AsObject>,
  }
}

export class DirectoryInfo extends jspb.Message {
  getName(): string;
  setName(value: string): void;

  getNumitems(): number;
  setNumitems(value: number): void;

  getDate(): number;
  setDate(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): DirectoryInfo.AsObject;
  static toObject(includeInstance: boolean, msg: DirectoryInfo): DirectoryInfo.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: DirectoryInfo, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): DirectoryInfo;
  static deserializeBinaryFromReader(message: DirectoryInfo, reader: jspb.BinaryReader): DirectoryInfo;
}

export namespace DirectoryInfo {
  export type AsObject = {
    name: string,
    numitems: number,
    date: number,
  }
}

export class FileInfo extends jspb.Message {
  getName(): string;
  setName(value: string): void;

  getSize(): number;
  setSize(value: number): void;

  getDate(): number;
  setDate(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): FileInfo.AsObject;
  static toObject(includeInstance: boolean, msg: FileInfo): FileInfo.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: FileInfo, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): FileInfo;
  static deserializeBinaryFromReader(message: FileInfo, reader: jspb.BinaryReader): FileInfo;
}

export namespace FileInfo {
  export type AsObject = {
    name: string,
    size: number,
    date: number,
  }
}

export class ImageInfo extends jspb.Message {
  getFilename(): string;
  setFilename(value: string): void;

  getFiletype(): FileTypeMap[keyof FileTypeMap];
  setFiletype(value: FileTypeMap[keyof FileTypeMap]): void;

  clearDimensionsList(): void;
  getDimensionsList(): Array<number>;
  setDimensionsList(value: Array<number>): void;
  addDimensions(value: number, index?: number): number;

  clearHeaderList(): void;
  getHeaderList(): Array<ImageInfo.HeaderEntry>;
  setHeaderList(value: Array<ImageInfo.HeaderEntry>): void;
  addHeader(value?: ImageInfo.HeaderEntry, index?: number): ImageInfo.HeaderEntry;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): ImageInfo.AsObject;
  static toObject(includeInstance: boolean, msg: ImageInfo): ImageInfo.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: ImageInfo, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): ImageInfo;
  static deserializeBinaryFromReader(message: ImageInfo, reader: jspb.BinaryReader): ImageInfo;
}

export namespace ImageInfo {
  export type AsObject = {
    filename: string,
    filetype: FileTypeMap[keyof FileTypeMap],
    dimensionsList: Array<number>,
    headerList: Array<ImageInfo.HeaderEntry.AsObject>,
  }

  export class HeaderEntry extends jspb.Message {
    getKey(): string;
    setKey(value: string): void;

    getValue(): string;
    setValue(value: string): void;

    getComment(): string;
    setComment(value: string): void;

    serializeBinary(): Uint8Array;
    toObject(includeInstance?: boolean): HeaderEntry.AsObject;
    static toObject(includeInstance: boolean, msg: HeaderEntry): HeaderEntry.AsObject;
    static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
    static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
    static serializeBinaryToWriter(message: HeaderEntry, writer: jspb.BinaryWriter): void;
    static deserializeBinary(bytes: Uint8Array): HeaderEntry;
    static deserializeBinaryFromReader(message: HeaderEntry, reader: jspb.BinaryReader): HeaderEntry;
  }

  export namespace HeaderEntry {
    export type AsObject = {
      key: string,
      value: string,
      comment: string,
    }
  }
}

export class ImageInfoRequest extends jspb.Message {
  getDirectoryname(): string;
  setDirectoryname(value: string): void;

  getFilename(): string;
  setFilename(value: string): void;

  getHduname(): string;
  setHduname(value: string): void;

  getHdunum(): number;
  setHdunum(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): ImageInfoRequest.AsObject;
  static toObject(includeInstance: boolean, msg: ImageInfoRequest): ImageInfoRequest.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: ImageInfoRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): ImageInfoRequest;
  static deserializeBinaryFromReader(message: ImageInfoRequest, reader: jspb.BinaryReader): ImageInfoRequest;
}

export namespace ImageInfoRequest {
  export type AsObject = {
    directoryname: string,
    filename: string,
    hduname: string,
    hdunum: number,
  }
}

export interface FileTypeMap {
  UNKNOWN: 0;
  FITS: 1;
  HDF5: 2;
  MHD: 3;
}

export const FileType: FileTypeMap;

