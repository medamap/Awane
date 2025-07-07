using System.Collections.Generic;

namespace Awane.Core.Components
{
    public interface IAwaneComponent
    {
        string AwaneId { get; }
        
        string AwaneLocation { get; }
        
        string[] AwaneInterfaces { get; }
        
        string AwaneName { get; }
        
        string AwaneVersion { get; }
        
        Dictionary<string, string> AwaneTags { get; }
        
        T? AwaneAs<T>() where T : class;
    }
}