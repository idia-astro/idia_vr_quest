#include <fmt/format.h>
#include <iostream>

#include <grpcpp/grpcpp.h>

#include "FileBrowserService.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;

void RunServer(const std::string& base_path_string) {
    std::string server_address("0.0.0.0:50051");
    FileBrowserService service(base_path_string);

    ServerBuilder builder;
    builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
    builder.RegisterService(&service);
    // Finally, assemble the server.
    std::unique_ptr<Server> server(builder.BuildAndStart());
    fmt::print("Server listening on {} with files from {}\n", server_address, base_path_string);
    server->Wait();
}

int main(int argc, char* argv[]) {
    if (argc < 2) {
        std::cout << "Usage " << argv[0] << " <base folder>" << std::endl;
        return 1;
    }
    RunServer(argv[1]);
    return 0;
}
