using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace APIGateway.Msv.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddGatewayTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceName = "ApiGateway-YARP";

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                        .AddSource("Yarp.ReverseProxy")
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
                        });
                });

            return services;
        }
    }
}
