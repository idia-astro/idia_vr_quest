cmake_minimum_required(VERSION 3.12)
project(IdiaVrServer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
include_directories(${CMAKE_SOURCE_DIR}/third-party/cxxopts/include)

find_package(fmt CONFIG REQUIRED)
find_package(spdlog CONFIG REQUIRED)
find_package(zfp CONFIG REQUIRED)
find_package(Boost REQUIRED)

IF (WIN32)
    find_package(unofficial-cfitsio CONFIG REQUIRED)
    add_compile_options(/wd4251)
ELSE ()
    find_package(CFITSIO QUIET)
    if (CFITSIO_FOUND)
        message(STATUS "Found cfitsio using find_package")
    else ()
        find_package(PkgConfig REQUIRED)
        pkg_search_module(CFITSIO REQUIRED cfitsio)
        if (CFITSIO_FOUND)
            message(STATUS "Found cfitsio using pkg-config")
        else ()
            message(FATAL_ERROR "Could not find cfitsio")
        endif ()
    endif()
ENDIF ()

add_executable(IdiaVrServer
        src/Image.cc
        src/FitsImage.cc
        src/FileBrowserService.cc
        src/Main.cc
        src/Util.cc
        src/Compression.cc
        src/Timer/Timer.cc
        )

target_link_libraries(IdiaVrServer
        PRIVATE
        protolib
        PUBLIC
        fmt::fmt
        zfp::zfp
        cfitsio
        )

add_subdirectory(../Protobuf proto)