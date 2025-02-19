using studymate_backend.Libraries.Methods;

namespace studymate_backend.Libraries.Helper;

public class SdmGlobalMiddlewareCacheManagement(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ClearCache();

        await next(context);
    }
    private static void ClearCache()
    {
        SdmSubject.ClearCache();
        SdmCurriculumGroup.ClearCache();
    }
}