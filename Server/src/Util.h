#ifndef SERVER_UTIL_H
#define SERVER_UTIL_H

#include <filesystem>
#include <string>

namespace fs = std::filesystem;

int GetNumItems(const fs::path& path);
int GetNumItems(const std::string& path);


#endif //SERVER_UTIL_H
