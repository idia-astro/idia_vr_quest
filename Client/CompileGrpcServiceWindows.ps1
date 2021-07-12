set protoc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\protobuf\protoc.exe'

Write-Host "Compiling protobuf and gRPC files for C#";
set grpc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\grpc\grpc_csharp_plugin.exe'
$dir = md Assets\Scripts\BackendService -ea 0
& $protoc_path -I ..\Protobuf `
    --csharp_out=Assets\Scripts\BackendService `
    --grpc_out=Assets\Scripts\BackendService `
    --plugin=protoc-gen-grpc=$grpc_path ..\Protobuf\BackendService.proto

Write-Host "Complete";
