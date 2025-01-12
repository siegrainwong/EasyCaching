namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit;

    public class MemoryCachingProviderTest : BaseCachingProviderTest
    {
        public MemoryCachingProviderTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x => x.UseInMemory(options => options.MaxRdSecond = 0));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Deault_MaxRdSecond_Should_Be_0()
        {
            Assert.Equal(0, _provider.MaxRdSecond);
        }


        [Fact]
        public void TrySet_Parallel_Should_Succeed()
        {
            var list = new List<bool>();

            Parallel.For(1, 20, x =>
            {
                list.Add(_provider.TrySet<int>("Parallel", 1, TimeSpan.FromSeconds(1)));
            });

            Assert.Equal(1, list.Count(x => x));

        }

        [Fact]
        public void Exists_After_Expiration_Should_Return_False()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, TimeSpan.FromMilliseconds(200));
            System.Threading.Thread.Sleep(300);
            var flag = _provider.Exists(cacheKey);

            Assert.False(flag);
        }

        [Fact]
        public void Issues105_StackOverflowException_Test()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var cacheValue = new Dictionary<string, IList<MySettingForCaching>>()
            {
                { "ss", new List<MySettingForCaching>{ new MySettingForCaching { Name = "aa" } } }
            };

            // without data retriever
            // _provider.Set(cacheKey, cacheValue, TimeSpan.FromMilliseconds(200));

            // with data retriever
            var res = _provider.Get(cacheKey, () =>
            {
                return cacheValue;
            }, _defaultTs);

            var first = res.Value.First();

            Assert.Equal("ss", first.Key);
            Assert.Equal(1, first.Value.Count);
        }

        [Serializable]
        public class MySettingForCaching
        {
            public string Name { get; set; }
        }
    }
    public class MemoryCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public MemoryCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.UseInMemory();
                x.UseInMemory(SECOND_PROVIDER_NAME);
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultInMemoryName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

    }

    public class MemoryCachingProviderUseEasyCachingTest : BaseUsingEasyCachingTest
    {
        private readonly IEasyCachingProvider _secondProvider;
        private const string SECOND_PROVIDER_NAME = "second";

        public MemoryCachingProviderUseEasyCachingTest()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddEasyCaching(option =>
            {
                option.UseInMemory(EasyCachingConstValue.DefaultInMemoryName);
                option.UseInMemory(SECOND_PROVIDER_NAME);
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultInMemoryName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Sec_Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _secondProvider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _secondProvider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Sec_Set_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _secondProvider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _secondProvider.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }
    }

    public class MemoryCachingProviderUseEasyCachingWithConfigTest : BaseUsingEasyCachingTest
    {
        public MemoryCachingProviderUseEasyCachingWithConfigTest()
        {
            IServiceCollection services = new ServiceCollection();

            var appsettings = @"
{
    'easycaching': {
        'inmemory': {
            'MaxRdSecond': 600,
            'dbconfig': {      
                'SizeLimit' :  50
            }
        }
    }
}";
            var path = TestHelpers.CreateTempFile(appsettings);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);
            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            services.AddEasyCaching(option => { option.UseInMemory(config, "mName"); });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Provider_Information_Should_Be_Correct()
        {
            Assert.Equal(600, _provider.MaxRdSecond);
            //Assert.Equal(99, _provider.Order);
            Assert.Equal("mName", _provider.Name);
        }
    }
}