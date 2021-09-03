cmake_minimum_required(VERSION 3.15)

project(nativeFunctions)
set(CMAKE_CXX_STANDARD 17)

add_library(nativeFunctions SHARED NativeFunctions.cc)

find_package(zfp CONFIG REQUIRED)
find_package(OpenMP REQUIRED)

set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} ${OpenMP_C_FLAGS}")
set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${OpenMP_CXX_FLAGS}")
set (CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} ${OpenMP_EXE_LINKER_FLAGS}")

target_link_libraries(nativeFunctions PUBLIC zfp::zfp OpenMP::OpenMP_CXX)

if (WIN32)
    SET(CMAKE_FIND_LIBRARY_PREFIXES "")
    SET(CMAKE_FIND_LIBRARY_SUFFIXES ".dll")
    find_library(ZFP_RUNTIME "zfp")
else()
    SET(CMAKE_FIND_LIBRARY_PREFIXES "lib")
    SET(CMAKE_FIND_LIBRARY_SUFFIXES ".so")
    # Sanitize zfp_DIR input when calling cmake from windows
    string(REPLACE "\\" "/" zfp_DIR ${zfp_DIR})
    SET(ZFP_RUNTIME "${zfp_DIR}/../../libzfp.so")
endif()

if (WIN32)
    SET(TARGET_LIB ${CMAKE_BINARY_DIR}/nativeFunctions.dll)
    SET(DEST_DIR ${PROJECT_SOURCE_DIR}/../Assets/Plugins/x86_64)
    install(FILES ${ZFP_RUNTIME} ${TARGET_LIB} DESTINATION ${DEST_DIR})
else()
    SET(TARGET_LIB ${CMAKE_BINARY_DIR}/libnativeFunctions.so)
    SET(DEST_DIR ${PROJECT_SOURCE_DIR}/../Assets/Plugins/Android/lib/${ANDROID_ABI}/)
    install(FILES ${OpenMP_CXX_LIBRARIES} ${ZFP_RUNTIME} ${TARGET_LIB} DESTINATION ${DEST_DIR})
endif()


