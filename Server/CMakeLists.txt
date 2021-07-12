cmake_minimum_required(VERSION 3.19)
project(Server)

set(CMAKE_CXX_STANDARD 17)

find_package(Protobuf REQUIRED)
find_package(gRPC CONFIG REQUIRED)

include_directories(${PROTOBUF_INCLUDE_DIR})

add_executable(Server
        DataApi/DataApi.grpc.pb.cc
        DataApi/DataApi.pb.cc
        src/main.cc
        src/FileBrowserImpl.cpp
        )

target_link_libraries(Server ${PROTOBUF_LIBRARY} gRPC::grpc++ gRPC::grpc++_reflection)