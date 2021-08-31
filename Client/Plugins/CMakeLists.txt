cmake_minimum_required(VERSION 3.15)

project(nativeFunctions)
set(CMAKE_CXX_STANDARD 17)

add_library(nativeFunctions SHARED NativeFunctions.cc)

find_package(zfp REQUIRED)
target_link_libraries(nativeFunctions PUBLIC zfp::zfp)

find_package(OpenMP)
if (OPENMP_FOUND)
    set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} ${OpenMP_C_FLAGS}")
    set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${OpenMP_CXX_FLAGS}")
    set (CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} ${OpenMP_EXE_LINKER_FLAGS}")
endif()


install(TARGETS nativeFunctions
        RUNTIME
            DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins
        )
#install(FILES ${OPENMP_DLL} ${SEH_DLL} ${PTHREAD_DLL} DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins)