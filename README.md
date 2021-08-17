# IDIA Oculus Quest Software

Exploratory project for viewing remote image cubes on the Oculus Quest 2. The software uses a client-server approach,
with [gRPC](https://grpc.io/) used for communication.

## Server

The server is written in C++, and should work on most Linux distributions, or Windows 10 (although this support may be
removed soon). The build process has been tested on Ubuntu 20.04.2 LTS and Windows 10 (
using [vcpkg](https://github.com/microsoft/vcpkg)).

### Requirements

- C++17 compatible compiler (GCC 8+, clang 11, msvc 2017+). Tested on GCC 9.3 and msvc 2019.
- [cmake](https://cmake.org/) 3.12 or newer.
- pkg-config (`pkg-config` on Debian;  `pkgconf` in vcpkg).
- Protocol buffer libraries and compilers (`libprotobuf-dev` and `protobuf-compiler` on Debian; `protobuf` in vcpkg).
- gRPC and protocol buffer libraries and compilers (`libgrpc++-dev` and `protobuf-compiler-grpc` on Debian; `grpc` in
  vcpkg).
- [fmt](https://github.com/fmtlib/fmt) and [spdlog](https://github.com/gabime/spdlog) libraries (`libfmt-dev`
  and `libspdlog-dev` on Debian; `fmt` and `spdlog` in vcpkg).
- [cfitsio](https://heasarc.gsfc.nasa.gov/fitsio/) library (`libcfitsio-dev` on Debian; `cfitsio` in vcpkg).

### Building

- Checkout the repo and all submodules using `git submodule update --init --recursive`.
- Create a build folder and `cd` into it.
- Run `cmake <path_to_source_folder>`. On Windows platforms you may need to specify the vcpkg toolchain file
  using `-DCMAKE_TOOLCHAIN_FILE=C:\<path_to_vcpkg_root_dir>\scripts\buildsystems\vcpkg.cmake`.
- Run `make` (or `nmake` for msvc).

## Client

The client is written in C#, and has only been tested on Windows (building for Android) with Unity 2021.1. The
powershell script `CompileGrpcServiceWindows.ps1` needs to be run before running Unity.

## Tests

The test scripts are written in TypeScript. Run `npm install` and the powershell script `CompileGrpcServiceWindows.ps1`
before running the client tests using `npm run start client`.