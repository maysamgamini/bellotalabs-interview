using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using BellotaLabInterview.Infrastructure.DependencyInjection;
using Moq;

namespace BellotaLabInterview.Tests
{
    public abstract class TestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IServiceCollection Services { get; private set; }

        protected TestBase()
        {
            Services = new ServiceCollection();
            ConfigureServices(Services);
            ServiceProvider = Services.BuildServiceProvider();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Add default services
            services.AddBellotaLabServices();
        }

        protected T? GetService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        protected T GetRequiredService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        protected Mock<T> RegisterMock<T>() where T : class
        {
            var mock = new Mock<T>();
            Services.AddSingleton(mock.Object);
            return mock;
        }
    }
} 