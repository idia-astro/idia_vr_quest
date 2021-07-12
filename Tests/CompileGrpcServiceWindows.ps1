set protoc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\protobuf\protoc.exe'

Write-Host "Compiling protobuf and gRPC files for NodeJS (TypeScript)";
set grpc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\grpc\grpc_node_plugin.exe'
set protoc_gen_ts_path C:\Users\accom\source\repos\idia_vr_quest\Tests\node_modules\.bin\protoc-gen-ts.cmd
set out_dir 'BackendService'
$dir = md $out_dir -ea 0

& $protoc_path -I ..\Protobuf `
    --ts_out="${out_dir}" `
    --grpc_out="${out_dir}" `
    --plugin=protoc-gen-ts=$protoc_gen_ts_path `
    --plugin=protoc-gen-grpc=$grpc_path BackendService.proto

Write-Host "Complete";
