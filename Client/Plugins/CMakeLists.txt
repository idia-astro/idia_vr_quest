cmake_minimum_required(VERSION 3.15)

project(nativeFunctions)
set(CMAKE_CXX_STANDARD 17)

add_library(nativeFunctions SHARED NativeFunctions.cc)

find_package(zfp REQUIRED)
target_link_libraries(nativeFunctions PUBLIC zfp::zfp)


install(TARGETS nativeFunctions
        RUNTIME
            DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins
        )
#install(FILES ${OPENMP_DLL} ${SEH_DLL} ${PTHREAD_DLL} DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins)