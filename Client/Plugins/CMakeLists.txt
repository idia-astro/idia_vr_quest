cmake_minimum_required(VERSION 3.15)

project(native_plugins)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -static")

add_subdirectory(zfp_decompression)

install(TARGETS zfp_decompression
        RUNTIME
            DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins
        )
install(FILES ${OPENMP_DLL} ${SEH_DLL} ${PTHREAD_DLL} DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins)