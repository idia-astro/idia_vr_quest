cmake_minimum_required(VERSION 3.12)
find_package(Protobuf REQUIRED)
find_package(Threads)

if (WIN32)
    add_compile_options(/wd4251)
endif()

find_package(gRPC CONFIG QUIET)
if (gRPC_FOUND)
    set(GRPC_LIBS
            gRPC::grpc
            gRPC::grpc++
            )
    message(STATUS "gRPC found using FIND_PACKAGE")
else ()
    message(STATUS "gRPC not found using FIND_PACKAGE, using pkg-config instead")
    find_package(PkgConfig REQUIRED)
    pkg_search_module(GRPC REQUIRED grpc)
    if (GRPC_FOUND)
        message(STATUS "gRPC found using pkg-config")
        set(GRPC_LIBS
                grpc
                grpc++
                )
    else ()
        message(FATAL_ERROR "Could not import gRPC library")
    endif ()
endif ()


set(PROTO_FILES
        DataApi.proto
        )

include_directories(${PROTOBUF_INCLUDE_DIR})

add_library(protolib ${PROTO_FILES})

target_link_libraries(protolib
        PUBLIC
        ${PROTOBUF_LIBRARY}
        ${GRPC_LIBS}
        )

target_include_directories(protolib PUBLIC ${CMAKE_CURRENT_BINARY_DIR})
protobuf_generate(TARGET protolib LANGUAGE cpp)
# need to make sure protoc-gen-gprpc points to protoc-grpc_cpp_plugin, or use the "PLUGIN" variable below
protobuf_generate(TARGET protolib LANGUAGE grpc GENERATE_EXTENSIONS .grpc.pb.h .grpc.pb.cc)