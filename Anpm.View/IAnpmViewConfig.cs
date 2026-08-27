using Anpm.Core.Models;

namespace Anpm.View;

public interface IAnpmViewConfig
{
    bool TryLoadStatus(out FeedStatusReport? report, out string? v3BaseUrl, out string? error);
}
