find_package(Protobuf REQUIRED)
find_package(gRPC CONFIG REQUIRED)
find_package(Threads)

set(PROTO_FILES
    DataApi.proto
)

include_directories(${PROTOBUF_INCLUDE_DIR})

add_library(protolib ${PROTO_FILES})

target_link_libraries(protolib
    PUBLIC
        ${PROTOBUF_LIBRARY}
        gRPC::grpc
        gRPC::grpc++
)

target_include_directories(protolib PUBLIC ${CMAKE_CURRENT_BINARY_DIR})

get_target_property(grpc_cpp_plugin_location gRPC::grpc_cpp_plugin LOCATION)
protobuf_generate(TARGET protolib LANGUAGE cpp)
protobuf_generate(TARGET protolib LANGUAGE grpc GENERATE_EXTENSIONS .grpc.pb.h .grpc.pb.cc)