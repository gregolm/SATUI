using SATUI.Models;

namespace SATUI.Services;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
}
