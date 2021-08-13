cmake_minimum_required(VERSION 3.19)
project(IdiaVrServer)

set(CMAKE_CXX_STANDARD 17)

find_package(fmt CONFIG REQUIRED)
find_package(unofficial-cfitsio CONFIG REQUIRED)
find_package(Protobuf REQUIRED)
find_package(gRPC CONFIG REQUIRED)

include_directories(${PROTOBUF_INCLUDE_DIR})

add_executable(IdiaVrServer
        DataApi/DataApi.grpc.pb.cc
        DataApi/DataApi.pb.cc
        src/FitsImage.cc
        src/FileBrowserImpl.cc
        src/main.cc
        src/Util.cc
        )

target_link_libraries(IdiaVrServer ${PROTOBUF_LIBRARY} gRPC::grpc++ gRPC::grpc++_reflection)
target_link_libraries(IdiaVrServer fmt::fmt cfitsio)