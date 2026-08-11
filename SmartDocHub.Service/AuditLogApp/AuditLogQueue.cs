using SmartDocHub.Domain.AuditLog;

using System.Threading.Channels;

namespace SmartDocHub.Web.AuditLog;

public class AuditLogQueue
{
    private readonly Channel<SysLog> _channel;
    // 创建无界通道
    public AuditLogQueue()
    {
        _channel = Channel.CreateBounded<SysLog>(new BoundedChannelOptions(100)
        {
            SingleReader = true
        });
    }

    public void QueueLog(SysLog log)
    {
        _channel.Writer.TryWrite(log);
    }

    public ChannelReader<SysLog> Reader => _channel.Reader;

}
