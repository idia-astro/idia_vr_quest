#ifndef IDIAVRSERVER_IMAGE_H
#define IDIAVRSERVER_IMAGE_H

#include <string>
#include <vector>

#include <boost/multi_array.hpp>
#include <DataApi.pb.h>

#define TEMP_FILE_ID -1

typedef boost::multi_array<float, 3> TensorF;
typedef TensorF::index TensorFIndex;
typedef boost::array<TensorFIndex, 3> TensorFShape;

struct HeaderEntry {
    std::string key;
    std::string value;
    std::string comment;
};

struct WcsEntry {
    int axis;
    double ref_pix;
    double ref_val;
    double delta;
};

class Image {
protected:
    std::string _error_message;
    std::string _hdu_name;
    int _hdu_num;
    std::vector<int> _dimensions;
    std::string _unit;
    std::vector<HeaderEntry> _header;
    std::vector<WcsEntry> _wcs;

    TensorF _data_cube;

    int _file_id = TEMP_FILE_ID;

public:
    const std::vector<int>& Dimensions();
    const std::vector<HeaderEntry>& Header();
    const std::string& ErrorMessage();
    bool FillImageInfo(DataApi::ImageInfo* imageInfo);
    bool FillImageData(DataApi::DataResponse& res, int channelOffset, int num_channels, int precision);
    void GetMinMax(float& min_value, float& max_value);
    bool DataLoaded();
    void SetFileId(int id);
    virtual bool IsValid() = 0;
    virtual bool LoadData() = 0;
    virtual ~Image() = default;
};

#endif // IDIAVRSERVER_IMAGE_H
