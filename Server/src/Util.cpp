#include "Util.h"

int GetNumItems(const fs::path& path) {
    try {
        int counter = 0;
        auto it = fs::directory_iterator(path);
        for (const auto& item : it) {
            counter++;
        }
        return counter;
    } catch (std::exception&) {
        return -1;
    }
}

int GetNumItems(const std::string& path) {
    return GetNumItems(fs::path(path));
}