#ifndef IDIAVRSERVER_TIMER_H
#define IDIAVRSERVER_TIMER_H

#include <chrono>
#include <string>
#include <unordered_map>

#include <spdlog/spdlog.h>

// Class adapted from CARTA backend source code
typedef std::chrono::time_point<std::chrono::high_resolution_clock> timer_entry;
typedef std::chrono::duration<double, std::milli> timer_duration;
class Timer {
public:
    void Start(const std::string& timer_name);
    void End(const std::string& timer_name);
    void Clear(const std::string& timer_name = "");
    timer_duration GetMeasurement(const std::string& timer_name, bool clear_after_fetch = false);
    std::string GetMeasurementString(const std::string& timer_name, bool clear_after_fetch = false);
    void Print(const std::string& timer_name = "", bool clear_after_fetch = false, spdlog::level::level_enum level = spdlog::level::debug);

protected:
    std::unordered_map<std::string, timer_entry> _entries;
    std::unordered_map<std::string, std::pair<timer_duration, int>> _measurements;
};


#endif // IDIAVRSERVER_TIMER_H
