#include "Util.h"

#include <fstream>

int GetFolderItemCount(const fs::path& path) {
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

uint64_t GetMagicNumber(const fs::path& path) {
    uint64_t magic_number = 0;

    std::ifstream input_file(path.string());
    if (input_file) {
        input_file.read((char*)&magic_number, sizeof(uint64_t));
        input_file.close();
    }

    return magic_number;
}
