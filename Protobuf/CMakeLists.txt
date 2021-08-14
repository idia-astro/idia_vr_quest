cmake_minimum_required(VERSION 3.12)
find_package(Protobuf REQUIRED)
find_package(Threads)

if (WIN32)
    add_compile_options(/wd4251)
endif ()

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

INCLUDE_DIRECTORIES(${PROTOBUF_INCLUDE_DIR})
find_program(PROTOC protoc)
find_program(GRPC_CPP_PLUGIN_EXECUTABLE grpc_cpp_plugin)

# Proto file
get_filename_component(proto_path_abs "./DataApi.proto" ABSOLUTE)
get_filename_component(proto_path "${proto_path_abs}" PATH)
# Generated sources
set(proto_srcs "${CMAKE_BINARY_DIR}/proto/DataApi.pb.cc")
set(proto_hdrs "${CMAKE_BINARY_DIR}/proto/DataApi.pb.h")
set(grpc_srcs "${CMAKE_BINARY_DIR}/proto/DataApi.grpc.pb.cc")
set(grpc_hdrs "${CMAKE_BINARY_DIR}/proto/DataApi.grpc.pb.h")

add_custom_command(
        OUTPUT "${proto_srcs}" "${proto_hdrs}" "${grpc_srcs}" "${grpc_hdrs}"
        COMMAND ${PROTOC}
        ARGS --grpc_out "${CMAKE_BINARY_DIR}/proto"
        --cpp_out "${CMAKE_BINARY_DIR}/proto"
        -I "${proto_path}"
        --plugin=protoc-gen-grpc="${GRPC_CPP_PLUGIN_EXECUTABLE}"
        "${proto_path_abs}"
        DEPENDS "${proto_path_abs}")

add_library(protolib ${proto_hdrs} ${proto_srcs} ${grpc_hdrs} ${grpc_srcs})
target_link_libraries(protolib
        ${PROTOBUF_LIBRARY}
        ${GRPC_LIBS}
        )

target_include_directories(protolib PUBLIC ${CMAKE_CURRENT_BINARY_DIR})