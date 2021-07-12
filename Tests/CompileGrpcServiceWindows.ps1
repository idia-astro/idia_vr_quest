set protoc_path $env:VCPKG_ROOT'\installed\x64-windows\tools\protobuf\protoc.exe'
set out_dir 'DataApi'
$dir = md $out_dir -ea 0

Write-Host "Compiling protobuf and gRPC files for NodeJS (JavaScript & TypeScript)";
node_modules\.bin\grpc_tools_node_protoc `
    -I ..\Protobuf `
    --js_out="import_style=commonjs,binary:${out_dir}" `
    --plugin="protoc-gen-ts=node_modules\.bin\protoc-gen-ts.cmd" `
    --ts_out="service=grpc-node,mode=grpc-js:${out_dir}" `
    --grpc_out="grpc_js:${out_dir}" ..\Protobuf\DataApi.proto

Write-Host "Compiling simplified client files (TypeScript)";
$dir = md "DataApiSimple" -ea 0
npx pbjs -p ..\Protobuf\ -t static-module --w commonjs -o DataApiSimple/index.js ..\Protobuf\DataApi.proto
npx pbts -o .\DataApiSimple\index.d.ts .\DataApiSimple\index.js
Write-Host "Complete";