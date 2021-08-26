add_library(zfp_decompression SHARED zfp_decompression.cc)
target_link_options(zfp_decompression PUBLIC "-static-libgcc" "-static-libstdc++")
set_target_properties(zfp_decompression PROPERTIES CXX_STANDARD 17)

find_package(zfp CONFIG REQUIRED)
target_link_libraries(zfp_decompression PRIVATE zfp::zfp)

#
#find_package(OpenMP)
#if(OpenMP_CXX_FOUND)
#    target_link_libraries(data_analysis_tool PUBLIC OpenMP::OpenMP_CXX)
#endif()
#find_file(OPENMP_DLL libgomp-1.dll)
#find_file(SEH_DLL libgcc_s_seh-1.dll)
#find_file(PTHREAD_DLL libwinpthread-1.dll)