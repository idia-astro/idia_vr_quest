# IDIA Oculus Quest Software

Exploratory project for viewing remote image cubes on the Oculus Quest 2. The software uses a client-server approach,
with [gRPC](https://grpc.io/) used for communication. Data is compressed on the backend and streamed to the client. The client includes dynamic step sizes to keep the framerate at the target.


https://github.com/user-attachments/assets/eba938dc-4e41-4316-ae67-9eafbb0ca896


## Server

The server is written in C++, and should work on most Linux distributions, or Windows 10 (_for now_). The build process has been tested on Ubuntu 20.04.2 LTS and Windows 10 (using [vcpkg](https://github.com/microsoft/vcpkg)).

### Requirements

- C++17 compatible compiler (GCC 8+, clang 11, msvc 2017+). Tested on GCC 9.3 and msvc 2019.
- [cmake](https://cmake.org/) 3.12 or newer.
- `make` (Linux) or `nmake` (Windows) in your path.
- pkg-config (`pkg-config` on Debian;  `pkgconf` in vcpkg).
- Protocol buffer libraries and compilers (`libprotobuf-dev` and `protobuf-compiler` on Debian; `protobuf` in vcpkg).
- gRPC and protocol buffer libraries and compilers (`libgrpc++-dev` and `protobuf-compiler-grpc` on Debian; `grpc` in
  vcpkg).
- [fmt](https://github.com/fmtlib/fmt) and [spdlog](https://github.com/gabime/spdlog) libraries (`libfmt-dev`
  and `libspdlog-dev` on Debian; `fmt` and `spdlog` in vcpkg).
- [cfitsio](https://heasarc.gsfc.nasa.gov/fitsio/) library (`libcfitsio-dev` on Debian; `cfitsio` in vcpkg).
- [Boost.MultiArray](https://www.boost.org/doc/libs/1_77_0/libs/multi_array/doc/index.html) template library (`libboost-dev` on Debian; `boost-multi-array` in vcpkg).
- [zfp](https://github.com/LLNL/zfp) library (`zfp` in vcpkg, compile from source on Debian).

### Building

**Note:** when using `cmake` with msvc and `nmake`, you must append the `-G "NMake Makefiles"` argument to each `cmake` call.

- Checkout the repo and all submodules using `git submodule update --init --recursive`.
- Create a build folder and `cd` into it.
- Run `cmake <path_to_source_folder>`. On Windows platforms you may need to specify the vcpkg toolchain file
  using `-DCMAKE_TOOLCHAIN_FILE=C:\<path_to_vcpkg_root_dir>\scripts\buildsystems\vcpkg.cmake`.
- Run `make` (or `nmake` for msvc).

## Client

The client is written in C#, and has only been tested on Windows (building for Android) with Unity 2021.1. 
- [Oculus integration package](https://developer.oculus.com/documentation/unity/unity-import/) needs to be imported.
- The grpc Unity plugin (`grpc_unity_package.2.41.0-dev202109021012.zip` from [here](https://packages.grpc.io/archive/2021/09/7911beacdb6175429828ee5c66eb4f7a86e848e4-358c5903-b1c5-42c0-ba79-f7ef12b4ea63/index.xml)) needs to be downloaded and unzipped to the `Assets/Plugins` directory.
- Run `CompileGrpcService.sh` (Linux) or `CompileGrpcServiceWindows.ps1` (Windows) before running Unity.


### Plugins
The client has a `NativeFunctions` C++ plugin that can be compiled using vcpkg (Windows) or cross-compiled using the Android NDK (On Windows or Linux).
The plugin needs to be built for both Windows (x64) and Android (arm64-v8a). The plugin depends on ZFP and OpenMP.

#### Building for Windows
Compilation for Windows should be similar to that of the server:
- Run `cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_TOOLCHAIN_FILE=C:\<path_to_vcpkg_root_dir>\scripts\buildsystems\vcpkg.cmake <path_to_source_folder>`
- Run `nmake install` to build the plugin and install to the correct `Plugins`  sub-folder, along with dependencies.

#### Building for Android
Compilation for Android requires the Android NDK. You must first compile ZFP using the Android NDK:
- Clone the ZFP repo at branch `0.5.5` (`git clone https://github.com/LLNL/zfp.git -b 0.5.5`).
- Create a build directory `zfp/build` and `cd` to it. 
- Run `cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_TOOLCHAIN_FILE=<path_to_ndk_root>/build/cmake/android.toolchain.cmake -DANDROID_ABI=arm64-v8a -DANDROID_NATIVE_API_LEVEL=23 -DCMAKE_INSTALL_PREFIX=../install_ndk ..` 
- Run `make install` to build and install to the `zfp/install_ndk` folder.

Once this is done:
- Run `cmake -DCMAKE_TOOLCHAIN_FILE=<path_to_ndk_root>/build/cmake/android.toolchain.cmake -DANDROID_ABI=arm64-v8a -DANDROID_NATIVE_API_LEVEL=23 -Dzfp_DIR=<path_to_zfp_install_ndk>/lib/cmake/zfp`.
- Run `make install` (or `nmake install` for msvc) to build the plugin and install to the correct `Plugins` sub-folder, along with dependencies.
### Config
A `config.json` file should be placed in the [persistent data path](https://docs.unity3d.com/2021.1/Documentation/ScriptReference/Application-persistentDataPath.html), in order to specify the server address and file/folder paths. An example config is shown below:

```json
{
  "serverAddress": "localhost:50051",
  "folder": "fits/vr",
  "file": "m81.fits",
  "maxCubeSizeMb": 200,
  "slicesPerMessage": 4,
  "compressionPrecision": 12
}
```

## Tests

The test scripts are written in TypeScript, and require NodeJS and NPM. Run `npm install` and the powershell script `CompileGrpcServiceWindows.ps1`
before running the client tests using `npm run start client`.
