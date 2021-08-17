#include <iostream>

#include <grpcpp/grpcpp.h>
#include <spdlog/spdlog.h>
#include <cxxopts/cxxopts.hpp>

#include "FileBrowserService.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;

#define VERSION_ID "v0.0.1"

void RunServer(const fs::path& base_path) {
    std::string server_address("0.0.0.0:50051");
    FileBrowserService service(base_path);

    ServerBuilder builder;
    builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
    builder.RegisterService(&service);
    // Finally, assemble the server.
    std::unique_ptr<Server> server(builder.BuildAndStart());
    spdlog::info("Server listening on {} with files from {}", server_address, base_path.string());
    server->Wait();
}

int main(int argc, char* argv[]) {
    std::string folder;
    cxxopts::Options options("IdiaVrServerRemote", "Server-side component of IDIA's Oculus Quest VR software");

    // clang-format off
    options.add_options("basic usage")
        ("v,verbose", "Verbose output")
        ("h,help", "Print usage")
        ("version", "Print version")
        ("folder", "files to load", cxxopts::value<std::string>(folder));
    // clang-format on

    options.positional_help("<base folder>");
    options.parse_positional("folder");
    auto result = options.parse(argc, argv);

    if (result.count("version")) {
        spdlog::info(VERSION_ID);
        return 0;
    }
    if (result.count("help")) {
        std::cout << options.help();
        return 0;
    }

    if (result.count("verbose")) {
        spdlog::set_level(spdlog::level::debug);
        spdlog::info("Verbose output enabled");
    }

    if (folder.empty()) {
        std::cout << options.help({""});
        return 1;
    }

    fs::path base_path(folder);
    if (!fs::exists(base_path) || !fs::is_directory(base_path)) {
        spdlog::critical("Path {} does not exist or is not a folder", base_path.string());
        return 1;
    }

    RunServer(folder);
    return 0;
}
