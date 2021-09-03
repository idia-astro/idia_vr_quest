#!/bin/bash

echo "Compiling protobuf and gRPC files for C#"
mkdir -p Assets/Scripts/DataApi
protoc -I ../Protobuf --csharp_out=Assets/Scripts/DataApi --grpc_out=Assets/Scripts/DataApi --plugin=protoc-gen-grpc=/usr/bin/grpc_csharp_plugin ../Protobuf/DataApi.proto
echo "Complete"

