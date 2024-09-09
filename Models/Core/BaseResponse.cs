using studymate_backend.Enums;

// ReSharper disable All

namespace studymate_backend.Models.Core;

public class BaseResponse
{
    public BaseResponse(EnumResponseCode response, BaseModelRaw data)
    {
        Code = response.GetCode();
        Message = response.GetName();
        Data = data;
    }

    public BaseResponse(EnumResponseCode response, IEnumerable<BaseModelRaw> data)
    {
        Code = response.GetCode();
        Message = response.GetName();
        Data = data;
    }

    public BaseResponse(EnumResponseCode response)
    {
        Code = response.GetCode();
        Message = response.GetName();
    }

    public string Code { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }
}