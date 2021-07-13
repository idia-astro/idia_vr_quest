#ifndef SERVER_UTIL_H
#define SERVER_UTIL_H

#include <filesystem>
#include <string>

namespace fs = std::filesystem;

int GetFolderItemCount(const fs::path& path);
uint64_t GetMagicNumber(const fs::path& path);

#endif //SERVER_UTIL_H
