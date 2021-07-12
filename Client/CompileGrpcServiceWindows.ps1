set protoc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\protobuf\protoc.exe'

Write-Host "Compiling protobuf and gRPC files for C#";
set grpc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\grpc\grpc_csharp_plugin.exe'
$dir = md Assets\Scripts\DataApi -ea 0
& $protoc_path -I ..\Protobuf `
    --csharp_out=Assets\Scripts\DataApi `
    --grpc_out=Assets\Scripts\DataApi `
    --plugin=protoc-gen-grpc=$grpc_path ..\Protobuf\DataApi.proto

Write-Host "Complete";
