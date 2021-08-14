cmake_minimum_required(VERSION 3.19)
project(IdiaVrServer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")

find_package(fmt CONFIG REQUIRED)

IF (WIN32)
    find_package(unofficial-cfitsio CONFIG REQUIRED)
ELSE ()
    find_package(cfitsio CONFIG REQUIRED)
ENDIF ()

add_executable(IdiaVrServer
        src/FitsImage.cc
        src/FileBrowserService.cc
        src/main.cc
        src/Util.cc
        )

target_link_libraries(IdiaVrServer
        PRIVATE
        protolib
        PUBLIC
        fmt::fmt
        cfitsio
        )

add_subdirectory(proto)