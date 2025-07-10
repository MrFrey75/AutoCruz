# AutoCruz Logs Directory

This directory contains application logs for the AutoCruz Head Unit System.

## Log Files

### Application Logs
- `autocruz-{date}.log` - Main application logs (daily rotation)
- `autocruz-errors-{date}.log` - Error-only logs
- `autocruz-debug-{date}.log` - Debug logs (development only)

### Component Logs
- `hardware-{date}.log` - Hardware interface logs
- `plugins-{date}.log` - Plugin loading and operation logs
- `performance-{date}.log` - System performance metrics

### System Logs
- `startup.log` - Application startup sequence
- `shutdown.log` - Application shutdown sequence
- `crash-{timestamp}.log` - Crash dumps and stack traces

## Log Levels

| Level | Description | When Used |
|-------|-------------|-----------|
| `TRACE` | Detailed execution flow | Development debugging |
| `DEBUG` | Diagnostic information | Development and troubleshooting |
| `INFO` | General information | Normal operation events |
| `WARN` | Warning conditions | Recoverable errors |
| `ERROR` | Error conditions | Error events that don't stop execution |
| `FATAL` | Fatal error conditions | Unrecoverable errors |

## Log Format

Default log entry format:
```
[TIMESTAMP] [LEVEL] [LOGGER] [THREAD] MESSAGE
[2025-01-15 14:30:25.123] [INFO ] [AutoCruz.Host.Program] [1] Application starting...
```

### Structured Logging (JSON)
In production, logs can be configured for structured JSON format:
```json
{
  "timestamp": "2025-01-15T14:30:25.123Z",
  "level": "INFO",
  "logger": "AutoCruz.Host.Program",
  "thread": 1,
  "message": "Application starting...",
  "properties": {
    "version": "1.0.0",
    "environment": "Production"
  }
}
```

## Log Rotation

### File Rotation Policy
- **Daily Rotation**: New log file created each day
- **Size Limit**: Maximum 10MB per log file
- **Retention**: Keep logs for 7 days (Production) / 30 days (Development)
- **Compression**: Older logs are compressed to save space

### Manual Log Management
```bash
# View latest logs
tail -f autocruz-$(date +%Y%m%d).log

# Search for errors
grep "ERROR\|FATAL" autocruz-*.log

# Clean old logs (manual)
find . -name "autocruz-*.log" -mtime +7 -delete
```

## Configuration

Log settings are configured in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AutoCruz": "Debug"
    },
    "File": {
      "Path": "logs/autocruz-.log",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 7,
      "FileSizeLimitBytes": 10485760
    }
  }
}
```

## Performance Impact

### Log Level Impact
- `TRACE/DEBUG`: High performance impact, use only in development
- `INFO`: Moderate impact, suitable for production monitoring
- `WARN/ERROR`: Low impact, always enabled

### Best Practices
1. Use appropriate log levels for each message
2. Avoid logging in tight loops
3. Use structured logging for easier parsing
4. Monitor log file sizes in production
5. Implement log aggregation for multiple instances

## Monitoring and Alerts

### Log Monitoring
- Monitor error rates and patterns
- Set up alerts for FATAL errors
- Track performance metrics in logs
- Monitor disk space usage

### Common Log Patterns to Monitor
```bash
# High error rates
grep -c "ERROR" autocruz-$(date +%Y%m%d).log

# Application crashes
grep "FATAL\|Unhandled exception" autocruz-*.log

# Performance issues
grep "timeout\|slow\|performance" autocruz-*.log

# Hardware issues
grep -i "hardware\|can\|gpio" hardware-*.log
```

## Troubleshooting

### Common Issues

#### No Logs Generated
1. Check file permissions on logs directory
2. Verify logging configuration in appsettings.json
3. Check if application has write access
4. Look for startup errors in console output

#### Log Files Too Large
1. Reduce log level (INFO instead of DEBUG)
2. Adjust file size limits in configuration
3. Implement log rotation
4. Clean up old log files

#### Missing Log Entries
1. Check log level configuration
2. Verify logger name matches component
3. Check for exceptions in logging pipeline
4. Ensure structured logging is properly configured

### Development Tips

```csharp
// Use structured logging with properties
logger.LogInformation("User {UserId} performed {Action} at {Timestamp}", 
    userId, action, DateTime.UtcNow);

// Include context for debugging
using (logger.BeginScope("Processing request {RequestId}", requestId))
{
    // Operations here will include RequestId in all log entries
}

// Log exceptions with full context
try 
{
    // risky operation
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to process {Operation} for {Context}", 
        operation, context);
}
```

## Log Analysis Tools

### Recommended Tools
- **grep/awk/sed**: Command line text processing
- **Elasticsearch + Kibana**: Log aggregation and visualization
- **Splunk**: Enterprise log analysis
- **seq**: Structured log analysis
- **Grafana**: Log visualization and monitoring

### Sample Analysis Queries
```bash
# Find all plugin loading errors
grep "Plugin.*ERROR" plugins-*.log

# Count errors by component
grep "ERROR" autocruz-*.log | cut -d']' -f3 | sort | uniq -c

# Find performance bottlenecks
grep -E "(slow|timeout|performance)" autocruz-*.log | head -20
```