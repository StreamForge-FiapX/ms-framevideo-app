// src/infrastructure/configuration/DependencyInjectionConfig.cs
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ms_framevideo_app.src.application.ports;
using ms_framevideo_app.src.domain.services;
using ms_framevideo_app.src.infrastructure.adapters;
using ms_framevideo_app.src.application.usecases;

namespace ms_framevideo_app.src.infrastructure.configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Adiciona Amazon S3 Client
            services.AddAWSService<IAmazonS3>();

            // Serviços de domínio
            services.AddSingleton<FrameGenerationService>();

            // Adapters
            services.AddSingleton<IFrameProcessorPort, FrameProcessingAdapter>();
            services.AddSingleton<IMessagePublisherPort, RabbitMqPublisherAdapter>();
            services.AddSingleton<IStoragePort, S3StorageAdapter>();
            services.AddSingleton<ICachePort, RedisCacheAdapter>();

            // UseCases
            services.AddTransient<ProcessChunkUseCase>();
            services.AddTransient<UpdateFrameMetadataUseCase>();

            return services;
        }
    }
}
