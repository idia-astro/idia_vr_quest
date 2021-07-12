set protoc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\protobuf\protoc.exe'

Write-Host "Compiling protobuf and gRPC files for C++";
set grpc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\grpc\grpc_cpp_plugin.exe'
$dir = md BackendService -ea 0
& $protoc_path -I ..\Protobuf `
    --cpp_out=BackendService `
    --grpc_out=BackendService `
    --plugin=protoc-gen-grpc=$grpc_path ..\Protobuf\BackendService.proto

Write-Host "Complete";
